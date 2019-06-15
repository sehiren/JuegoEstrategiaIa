using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Only for debug purposes

    public Grid _grid;
    public DrawGrid _drawGrid;
    private Team _team;

    private LayerMask _groundMask;

    private List<GridNode> _selectedNodes;
    private Unit _selectedUnit;
   
    [Header("Hover unit control")]
    public float _hoverTime = 0.1f;
    private float _hoverTimer;
    private float _enemyTimer;
    private bool _movida;
    private Unit _hoveredUnit;
    private Button turnButton;
    private GridNode flagEnemy;
    private GridNode flagAlly;

    [Header("Team references")]
    private int _movementsCounter;
    private int _enemyIndex;
    private int _unitsInTurnBeginning;
    [SerializeField] private bool _isPlayerPlaying;
    public bool isPlayerPlaying { get { return _isPlayerPlaying; } }
    public Team team { get { return _team; } }
    public PlayerType type;
    private LevelController levelScript;
    private PathFinding path;
    private UpgradeAreaManager boostScript;
    private float totalDistance;

    private void Awake()
    {
        turnButton = GameObject.Find("Button").GetComponent<Button>();
        levelScript = GameObject.Find("GameManager").GetComponent<LevelController>();
        path = GameObject.Find("PathFindingManger").GetComponent<PathFinding>();
        boostScript = GameObject.Find("UpgradeAreaManager").GetComponent<UpgradeAreaManager>();
    }
    
    public void InitPlayer(TeamSet tSet)
    {
        _team = GetComponent<Team>();
        _team.InitTeam(tSet);
        
        _drawGrid = DrawGrid.instance;
        _groundMask = LayerMask.GetMask("Ground");

        _hoverTimer = 0;

        flagEnemy = levelScript.players[0]._team.flagPosition;
        flagAlly = _team.flagPosition;
        totalDistance = path.GetDistance(flagAlly, flagEnemy);
    }


    /// <summary>
    /// Update del jugador en cada frame, si es su turno
    /// </summary>
    public void UpdatePlayer()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition); // posición del raycast donde está el mouse
        RaycastHit hit;
        GridNode selectedNode = null;

        if (Physics.Raycast(cameraRay, out hit, 100f, _groundMask))
            selectedNode = PathRequestManager.RequestNodeFromWorldPosition(hit.point); // obtiene el nodo de la grid bajo el mouse

        if (type == PlayerType.User) // Update del equipo del jugador
        {         
            if (selectedNode != null && Input.GetMouseButtonDown(0))
            {
                OnPlayerLeftClick(selectedNode); //Comprobar  input del jugador
            }
        }
        else // Update del equipo controlado por la IA
        {
            Unit chip = team.units[_enemyIndex];

            if (chip != _selectedUnit)
            {
                GridNode selectedNodeChip = chip.GetGridNode(); // obtiene el nodo de la grid bajo de la ficha
                UnitSelection(chip); // _selectedUnit pasa a ser chip y se muestrean las casillas accesibles

                _enemyTimer = 0;
                _movida = false;
            }
            else if (_enemyTimer >= _hoverTime*1.5)
            {                
                if (!_movida)
                {
                    GridNode newNode;
                    newNode = BestNodeSelection(chip); // devuelve la mejor casilla accesible

                    if(newNode == null) chip.EnableUnit(false); // si no hay buenos resultados, no hacer nada
                    else OnSelectedUnit(newNode); // se mueve al nodo elegido y lo deselecciona
                    _movida = true;                                
                }
                _enemyIndex++;                
            }
            else _enemyTimer += Time.deltaTime;
        }

        //Si el mouse del jugador simplemente se encuentra encima de una casilla no vacia
        if(selectedNode != null)
        {
            //Comprobar si esta ocupada por una unidad
            if (selectedNode.isOccupied != null && selectedNode.isOccupied.tag == "Unit") 
            {
                Unit unit = (Unit)selectedNode.isOccupied;

                if (unit != _hoveredUnit) //Comprobar si es una unidad diferente
                {
                    UnitInfoManager.instance.HideLevelInfo();
                    UnitInfoManager.instance.HideAdvantageInfo();

                    _hoverTimer = 0;
                    _hoveredUnit = unit;
                }
                else //Si es el mismo
                {
                    if (_hoverTimer >= _hoverTime) //Ha pasado el tiempo minimo para que
                    {
                        UnitInfoManager.instance.ShowLevelInfo(_hoveredUnit); //Mostrar nivel

                        if (_selectedUnit != null && _selectedUnit.unitTeam != _hoveredUnit.unitTeam) //Si tenemos otra unidad seleccionada y sobre la que  estamos es de otro equipo
                            UnitInfoManager.instance.ShowAdvantageInfo(_selectedUnit, _hoveredUnit); //Mostrar ventaja en combate
                    }
                    else
                    {
                        _hoverTimer += Time.deltaTime;
                    }
                }
            }
            else
            {
                UnitInfoManager.instance.HideAdvantageInfo();
                UnitInfoManager.instance.HideLevelInfo();
            }
        }     
    }

    /// <summary>
    /// Devuelve el nodo más adecuado de la lista de nodos accesibles
    /// </summary>
    /// <param name="_selectedNodes">Lista de nodos aceccesibles</param>
    private GridNode BestNodeSelection(Unit chip)
    {
        // mutate return
        float seed = 0.15f;
        if (chip.role == StrategicRole.Defense) seed = 0.3f;
        else if (chip.role == StrategicRole.Center) seed = 0.2f;
        else if (chip.role == StrategicRole.Forward) seed = 0.1f;

        if (Random.value < seed) return Mutate(chip);

        // comportamientos
        GridNode best = null;
        GridNode objective = flagEnemy;
        List<GridNode> menances = new List<GridNode>();
        UpgradeArea nearestBoost = NearestBoostArea(chip); // saca el upgrade area compatible más cercana
        int len = int.MaxValue;
        bool priority = false;

        foreach(GridNode g in _selectedNodes) // buscar elementos con prioridad
        {
            if (IsNodeOnArea(g) && g.isOccupied != null)
            {
                if (g == flagEnemy) // si es la bandera enemiga
                {
                    return g;
                }
                else if (g.isOccupied.tag == "Unit") // si es una ficha
                {
                    Unit anotherUnit = (Unit)g.isOccupied;
                    if (!_team.IsUnitInTeam(anotherUnit)) // si la unidad no es amiga                     
                    {
                        float power = chip.GetUnitPower(anotherUnit);
                        float powerEnemy = anotherUnit.GetUnitPower(chip);
                        if (power < powerEnemy) // si la enemiga es más fuerte
                        {
                            menances.Add(g);
                            priority = true;
                        }
                        else if (power > powerEnemy) // si la enemiga es más debil
                        {
                            best = g;
                        }
                    }
                }
                else if(nearestBoost != null && g == nearestBoost.GetGridNode())
                {
                    best = g;
                }
            }
        }

        if (priority) // se activa el return prioritario cuando hay amenazas
        {
            // returnea el nodo accesible más lejano al enemigo y más proximo al objetivo
            return path.GetOptimalFurtherNode(_selectedNodes, menances, flagEnemy);
        }
        else if (best != null) return best;
        else // si no se han encontrado elementos con prioridad
        {
            switch (chip.role) // modificar objetivo según rol estratégico
            {
                case StrategicRole.Forward: // modo ataque
                    if (nearestBoost != null)
                    {
                        if (path.GetDistance(chip.GetGridNode(), nearestBoost.GetGridNode()) < totalDistance / 6)
                            objective = nearestBoost.GetGridNode();
                        else objective = flagEnemy;
                    }
                    else objective = flagEnemy;
                    break;

                case StrategicRole.Defense: // modo defensa
                    if (nearestBoost != null)
                    {
                        if (path.GetDistance(chip.GetGridNode(), nearestBoost.GetGridNode()) < totalDistance / 10)
                            objective = nearestBoost.GetGridNode();
                        else objective = flagAlly;
                    }
                    else objective = flagAlly;
                    break;

                default: // modo cobertura                    
                    if (nearestBoost != null)
                    {
                        if (path.GetDistance(chip.GetGridNode(), nearestBoost.GetGridNode()) < totalDistance / 3)
                            objective = nearestBoost.GetGridNode();
                        else objective = flagEnemy;
                    }
                    else objective = flagEnemy;

                    break;
            }            

            foreach (GridNode g in _selectedNodes) // recorrer la lista de nodos accesibles y elegir el camino más corto
            {
                if (IsNodeOnArea(g)) // si el nodo es accesible
                {
                    if (g.isOccupied == null) // si el nodo está vacio
                    {
                        int newLen = path.BestPathLength(g, objective);
                        if (newLen < len) // si el camino actual es mas corto que el mejor registrado
                        {
                            len = newLen;    // actualizar camino mas corto encontrado
                            best = g;        // actualizar mejor nodo
                        }
                    }                    
                }
            }
        }
                
        return best;
    }

    /// <summary>
    /// Randomiza el resultado evitando comportamientos roboticos
    /// </summary>
    private GridNode Mutate(Unit chip) 
    {
        List<GridNode> forward = new List<GridNode>();

        int min = int.MaxValue;
        int max = 0;

        foreach (GridNode g in _selectedNodes)
        {
            int len = path.BestPathLength(g, flagEnemy);
            if (len < min) min = len;
            if (len > max) max = len;
        }

        foreach (GridNode g in _selectedNodes)
        {
            if (g.isOccupied == null || g.isOccupied != null && g.isOccupied.tag == "Boost")
            {
                int len = path.BestPathLength(g, flagEnemy);
                if (len <= min + Mathf.Abs((max - min) / 2)) forward.Add(g);
            }
        }

        if (forward.Count > 0) return forward[Random.Range(0, forward.Count)];
        return null;
    }

    /// <summary>
    /// Encuentra la Upgrade Area más próxima a la unidad
    /// </summary>
    /// <param name="selectedNode">Nodo sobre el que se ha clickado</param>
    private UpgradeArea NearestBoostArea(Unit chip)
    {
        UpgradeArea[] boostAreas = boostScript._upgradeAreas;

        int min = int.MaxValue;
        UpgradeArea nearest = null;

        foreach(UpgradeArea boost in boostAreas)
        {
            if(boost.isAvaible && boost.GetGridNode() != null && chip.GetGridNode() != null)
            {
                if (boost.currentClass == chip.unitClass || boost.currentClass == UnitClass.Neutral)
                {                          
                    int len = path.BestPathLength(chip.GetGridNode(), boost.GetGridNode());
                    if (len < min)
                    {
                        min = len;
                        nearest = boost;
                    }
                }
            }
        }
        return nearest;
    }

    /// <summary>
    /// Acciones a ejecutarcuando el jugador haga click izquierdo
    /// </summary>
    /// <param name="selectedNode">Nodo sobre el que se ha clickado</param>
    private void OnPlayerLeftClick(GridNode selectedNode)
    {
        //Si el jugador tiene una unidad seleccionada
        if(_selectedUnit != null)
        {
            OnSelectedUnit(selectedNode);
        }
        else //Si el jugador no tiene ninguna unidad seleccionada
        {
            OnNotSelectedUnit(selectedNode);
        }
    }

    /// <summary>
    /// Funcion a ejecutar mientras sea el turno del jugador i no tenga seleccionada ninguna unidad
    /// </summary>
    private void OnNotSelectedUnit(GridNode selectedNode)
    {
        if (selectedNode.isOccupied != null && selectedNode.isOccupied.tag == ("Unit")) //Comprobar si el nodo esta ocupado por una unidad
        {
            Unit nodeUnit = (Unit)selectedNode.isOccupied;
            if (_team.IsUnitInTeam(nodeUnit) && nodeUnit.isAvailable) //Comprobar que la unidad sea del mismo equipo
                UnitSelection(nodeUnit);
        }
    }

    /// <summary>
    /// Acciones a ejecutar mientras el jugador tenga seleccionada una unidad
    /// </summary>
    private void OnSelectedUnit(GridNode selectedNode)
    {
        //Si el nodo seleccionado esta fuera del area accesible para la unidad seleccionada
        if (!IsNodeOnArea(selectedNode))
        {
            if (selectedNode.isOccupied != null && selectedNode.isOccupied.tag == "Unit") //Si esta ocupado por una unidad
            {
                Unit nodeUnit = (Unit)selectedNode.isOccupied;

                if (_team.IsUnitInTeam(nodeUnit) && nodeUnit.isAvailable) //Si la unidad es amiga
                    UnitSelection(nodeUnit);
                else //Seleccionar la unidad
                    DeselectSelectedUnit();
            }
            else //Si no
            {
                DeselectSelectedUnit();
            }
        }
        else //Si el nodo se encuentra dentro del area accesible
        {
            if (selectedNode.isOccupied != null) //Comprobar si el nodo actual esta ocupado
            {
                //Si se trata de la bandera enemiga 
                if (selectedNode.isOccupied.tag == "Flag" && selectedNode != _team.flagPosition) // es bandera y no es la del propio equipo
                {
                    _selectedUnit.Move(selectedNode);
                    DeselectSelectedUnit();
                }
                // Comprobar si el nodo actual esta ocupado por una unidad
                else if (selectedNode.isOccupied != null && selectedNode.isOccupied.tag == "Unit")
                {
                    Unit nodeUnit = (Unit)selectedNode.isOccupied; // recoge la unidad que ocupa dicho nodo

                    if (_team.IsUnitInTeam(nodeUnit) && nodeUnit.isAvailable) // si la unidad es amiga            
                            UnitSelection(nodeUnit);           
                    else
                    {
                        // cuando la unidad es enemiga
                        _selectedUnit.FightUnit(nodeUnit);
                        _movementsCounter++;
                        DeselectSelectedUnit();
                    }
                }
            }
            else //Si el nodo no esta ocupado por nada -> desplazamiento
            {
                _selectedUnit.Move(selectedNode);
                _movementsCounter++;
                DeselectSelectedUnit();
            }
        }
    }

    /// <summary>
    /// Comprobar si el jugador ha acabado su turno
    /// </summary>
    /// <returns></returns>
    public bool HasPlayerFinished()
    {
        if(type == PlayerType.NPC) return _enemyIndex < team.units.Count;

        return _movementsCounter < _unitsInTurnBeginning && _isPlayerPlaying;
    }

    /// <summary>
    /// Emepezar turno del jugador
    /// </summary>
    public void StartTurn()
    {
        //Enable all units
        _team.EnableUnits(true);

        _movementsCounter = 0;
        _enemyIndex = 0;
        _unitsInTurnBeginning = _team.units.Count;
        _isPlayerPlaying = true;
        if(type == PlayerType.User) turnButton.interactable = true;
    }

    /// <summary>
    /// Acabar turno del jugador
    /// </summary>
    public void FinishTurn()
    {
        _isPlayerPlaying = false;
        _team.EnableUnits(false);
        turnButton.interactable = false;
    }

    /// <summary>
    /// Seleccionar unidad
    /// </summary>
    /// <param name="nodeUnit">Unidad a seleccionar</param>
    private void UnitSelection(Unit nodeUnit)
    {
        _selectedUnit = nodeUnit;
        List<GridNode> newNodes = PathRequestManager.RequestAvaibleNodes(nodeUnit.currentNode, _selectedUnit.moveDistance, _team); // Lista de nodos vecinos accesibles que se muestran tras la selección

        // ELEGIR CASILLA ACCESIBLE DE LA LISTA NEWNODES

        if (newNodes != _selectedNodes)
        {
            if (_selectedNodes != null)
                foreach (GridNode node in _selectedNodes)
                    _drawGrid.HideNode(node.gridX, node.gridY);

            foreach (GridNode node in newNodes)
            {
                NodeState nodeState = NodeState.Available;
                if(node.isOccupied != null && node.isOccupied.tag == "Unit")//Nodo ocupat per una unitat
                {
                    if (_team.IsUnitInTeam((Unit)node.isOccupied)) //Si la unitat forma part del nostre equipo
                        nodeState = NodeState.FriendlyUnit;
                    else
                        nodeState = NodeState.EnemyUnit;
                }
                else if(node.isOccupied != null) //Si esta ocupado y no es una unidad, entonces es una bandera
                {
                    if (node == _team.flagPosition)
                        nodeState = NodeState.FriendlyFlag;
                    else
                        nodeState = NodeState.EnemyFlag;
                }
                if(type == PlayerType.User) _drawGrid.DrawNode(node.gridX, node.gridY, nodeState);
            }

            _selectedNodes = newNodes;
        }
    }

    /// <summary>
    /// Deseleccionar a la unidad seleccionada actualmente
    /// </summary>
    private void DeselectSelectedUnit()
    {
        if (_selectedNodes != null)
            foreach (GridNode node in _selectedNodes)
                _drawGrid.HideNode(node.gridX, node.gridY);

        _selectedUnit = null;
    }

    /// <summary>
    /// Comprobar si el nodo se encuentra dentro del area de la unidad seleccionada
    /// </summary>
    /// <param name="node">Node a comprobar</param>
    /// <returns></returns>
    private bool IsNodeOnArea(GridNode node)
    {
        if(_selectedNodes != null)
            return _selectedNodes.Contains(node);

        return false;
    }

    /// <summary>
    /// Saca la posición de la unidad más avanzada del player correspondiente
    /// </summary>
    public Vector3 CameraInitialTurnPosition()
    {
        int len = int.MaxValue;
        Unit best = null;
        Vector3 res;
        foreach(Unit u in team.units)
        {
            int newLen = path.BestPathLength(u.GetGridNode(), flagEnemy);
            if (newLen < len)
            {
                len = newLen; 
                best = u; 
            }
        }
        if (best == null || best.GetGridNode() == null) res = flagAlly.worldPosition;
        else
        {
            //res = best.GetGridNode().worldPosition;
            res = new Vector3(flagAlly.worldPosition.x, best.GetGridNode().worldPosition.y, best.GetGridNode().worldPosition.z);
        }
        return res;
    }

    public int GetTeamCount()
    {
        return _team.units.Count;
    }


}

public enum PlayerType
{
    NPC, User
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Unit _hoveredUnit;

    [Header("Team references")]
    private int _movementsCounter;
    private int _unitsInTurnBeginning;
    [SerializeField] private bool _isPlayerPlaying;
    public bool isPlayerPlaying { get { return _isPlayerPlaying; } }
    public Team team { get { return _team; } }

    public void InitPlayer(TeamSet tSet)
    {
        _team = GetComponent<Team>();
        _team.InitTeam(tSet);
        
        _drawGrid = DrawGrid.instance;
        _groundMask = LayerMask.GetMask("Ground");

        _hoverTimer = 0;
    }


    /// <summary>
    /// Update del  jugador en cada frame, si  es su turno
    /// </summary>
    public void UpdatePlayer()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GridNode selectedNode = null;

        if (Physics.Raycast(cameraRay, out hit, 100f, _groundMask))
            selectedNode = PathRequestManager.RequestNodeFromWorldPosition(hit.point);

        if(selectedNode != null)
        {
            //Comprobar  input del jugador
            if (Input.GetMouseButtonDown(0)) //Si el jugador se encuentra 
            {
                OnPlayerLeftClick(selectedNode);
            }

            //Si el jugador simplemente se encuentra encima
            if(selectedNode.isOccupied != null && selectedNode.isOccupied.tag == "Unit") //Comprobar si esta ocupada por una unidad
            {
                Unit unit = (Unit)selectedNode.isOccupied;

                if (unit != _hoveredUnit)
                {//Comprobar si es una unidad diferente
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
    /// Acciones a ejecutar cuando el jugador se coloca sobre un nodo y espera
    /// </summary>
    /// <param name="selectedNode"></param>
    private void OnMouseHoverWithouSelectedUnit(GridNode selectedNode)
    {

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
        //Si el nodo seleccionado esta  fuera del area
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
        else //Si el nodo se encuentra dentro del area
        {
            if (selectedNode.isOccupied != null) //Comprobar si el nodo actual esta ocupado
            {
                if (selectedNode.isOccupied.tag == "Flag" && selectedNode != _team.flagPosition) //Si  se trata de la bandera enemiga 
                {
                    _selectedUnit.Move(selectedNode);
                    DeselectSelectedUnit();
                }
                else if (selectedNode.isOccupied != null && selectedNode.isOccupied.tag == "Unit")// Comprobar si el nodo actual esta ocupado por una unidad
                {
                    Unit nodeUnit = (Unit)selectedNode.isOccupied;

                    if (_team.IsUnitInTeam(nodeUnit))
                    {
                        if (nodeUnit.isAvailable)
                            UnitSelection(nodeUnit);
                    }
                    else
                    {
                        _selectedUnit.FightUnit(nodeUnit);
                        _movementsCounter++;
                        DeselectSelectedUnit();
                    }

                }
            }
            else //Si el nodo no esta ocupado por nada
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
        _unitsInTurnBeginning = _team.units.Count;
        _isPlayerPlaying = true;
    }

    /// <summary>
    /// Acabar turno del jugador
    /// </summary>
    public void FinishTurn()
    {
        _isPlayerPlaying = false;
        _team.EnableUnits(false); 
    }

    /// <summary>
    /// Seleccionar unidad
    /// </summary>
    /// <param name="nodeUnit">Unidad a seleccionar</param>
    private void UnitSelection(Unit nodeUnit)
    {
        _selectedUnit = nodeUnit;
        List<GridNode> newNodes = PathRequestManager.RequestAvaibleNodes(nodeUnit.currentNode, _selectedUnit.moveDistance, _team);

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
                else if(node.isOccupied != null) //Si esta  ocupado y no es una unidad, entonces es unabandera
                {
                    if (node == _team.flagPosition)
                        nodeState = NodeState.FriendlyFlag;
                    else
                        nodeState = NodeState.EnemyFlag;
                }
                _drawGrid.DrawNode(node.gridX, node.gridY, nodeState);
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

    public int GetTeamCount()
    {
        return _team.units.Count;
    }
}

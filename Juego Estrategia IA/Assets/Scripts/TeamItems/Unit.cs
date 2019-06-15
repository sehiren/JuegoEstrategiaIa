using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : TeamItem
{
    [SerializeField] private UnitClass _unitClass;
    [SerializeField] private StrategicRole _role;
    private Team _unitTeam;
    [SerializeField] private int _unitLevel = 1;
    [SerializeField] private int _initialMoveDistance = 5;

    private MeshRenderer[] _renderers;
    private SpriteRenderer _classSprite;

    private bool _isAvailable = true;

    public Team unitTeam { get { return _unitTeam; } }
    public int unitLevel { get { return _unitLevel; } set { _unitLevel = value; } }
    public StrategicRole role { get { return _role; } set { _role = value; } }
    public int moveDistance { get { return _initialMoveDistance - (_unitLevel - 1); } }
    public bool isAvailable { get { return _isAvailable; } }
    public UnitClass unitClass{get { return _unitClass; } }

    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>();
        _classSprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void Move(GridNode newNode)
    {
        //Desplazar unidad a la posicion del nuevo nodo
        Vector3 newPos = newNode.worldPosition;
        newPos.y = transform.position.y;
        transform.position = newPos;

        //Asignar nuevo nodo
        currentNode.isOccupied = null; //Dejar nodo actual desocupado
        newNode.isOccupied = this; //Ocupar nuevo nodo
        currentNode = newNode; //Guardar el nuevo nodo como actual

        EnableUnit(false);
    }

    /// <summary>
    /// Ejecutar combate contra una unidad del equipo rival
    /// </summary>
    /// <param name="enemy">Unidad contra la que  se va a combatir</param>
    public void FightUnit(Unit enemy)
    {
        //Calcular fuerzas de combate  de las unidades
        float thisUnitPower = this.GetUnitPower(enemy);
        float enemyPower = enemy.GetUnitPower(this);

        if (thisUnitPower > enemyPower) //Si la unidad gana
        {
            Move(enemy.currentNode); //Moverse a la posicion del enemigo
            enemy.Die(); //Matar al enemigo
        }
        else if (thisUnitPower < enemyPower) //Si la unidad pierde
        {
            this.Die();  //Matar esta unidad
        }
        else //En caso de  empate ambas unidades son muertas
        {
            enemy.Die(); 
            this.Die();
        }
    }

    /// <summary>
    /// Habilitar o deshabilitar unidad
    /// </summary>
    /// <param name="isEnabled">Está habilitada o no</param>
    public void EnableUnit(bool isEnabled)
    {
        Color color = (isEnabled) ? _unitTeam.teamColor : (_unitTeam.teamColor * 0.5f);

        foreach (MeshRenderer r in _renderers)
            r.material.color = color;

        _isAvailable = isEnabled;
    }

    /// <summary>
    /// Hacer que la unidad se muera
    /// </summary>
    public void Die()
    {
        _unitTeam.KillUnit(this);
        Destroy(this.gameObject);
        if(_unitTeam.teamId != 0) _unitTeam.RoleChanges();
    }

    /// <summary>
    /// Inicializar la unidad
    /// </summary>
    /// <param name="teamMaterial">Set de colores del equipo</param>
    /// <param name="team">Referencia al propio equipo</param>
    public void InitUnit(Team team)
    {
        _unitTeam = team;

        EnableUnit(false);
    }

    /// <summary>
    /// Calcular poder de combate  de una  unidad de acuerdo a la clase de su enemiga
    /// </summary>
    /// <param name="enemyUnit">Unidad contra la que se combate</param>
    /// <returns></returns>
    public float GetUnitPower(Unit enemyUnit)
    {
        float unitPower = _unitLevel;

        switch (_unitClass)
        {
            case UnitClass.Sword:
                if (enemyUnit.unitClass == UnitClass.Shield)
                    unitPower *= 1.25f;
                break;
            case UnitClass.Shield:
                if (enemyUnit.unitClass == UnitClass.Lance)
                    unitPower *= 1.25f;
                break;
            case UnitClass.Lance:
                if (enemyUnit.unitClass == UnitClass.Sword)
                    unitPower *= 1.25f;
                break;
        }
        

        return unitPower;
    }

    /// <summary>
    /// Devuelve la casilla de la grid donde se encuentra la unidad 
    /// </summary>
    /// <param name="cameraRayChip">Rayo que saldrá de la camara en la posición de la unidad</param>
    /// <returns></returns>
    public GridNode GetGridNode()
    {
        return PathRequestManager.RequestNodeFromWorldPosition(this.transform.position);
    }

    /// <summary>
    /// Indica si es del mismo equipo que otra unidad
    /// </summary>
    /// <param name="unitTeam">Equipo al que pertenece la unidad</param>
    public bool SameTeam(Unit other)
    {
        return unitTeam == other.unitTeam;
    }

    //Actualizar unidad cuando se cambia algo en el editor
    private void OnValidate()
    {
        if (_classSprite == null)
            _classSprite = GetComponentInChildren<SpriteRenderer>();
        
        switch (_unitClass)
        {
            case UnitClass.Sword:
                _classSprite.sprite = LevelController.SwordSprite;
                break;
            case UnitClass.Shield:
                _classSprite.sprite = LevelController.ShieldSprite;
                break;
            default: 
                _classSprite.sprite = LevelController.LanceSprite;
                break;
        }
    }
}

public enum UnitClass
{
    Sword = 1, Shield = 2, Lance = 3, Neutral = 0
}

public enum StrategicRole
{
    Forward, Defense, Center
}



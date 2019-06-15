using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    //------------------ Información del equipo ---------------------
    [Tooltip("Posición actual de la bandera")]
    [SerializeField]private Transform _flagPosition;
    [Tooltip("Lista inicial de las unidades del equipo")]
    [SerializeField] private List<Unit> _initialUnits;
    //Identificador del equipo
    private TeamEnum _teamId;
    private Color _teamColor;
    //Unidades durante la partida del equipo
    private List<Unit> _currentUnits;
    //---------------------------------------------------------------

    //------------------ Getters y Setters --------------------------
    public GridNode flagPosition { get { return PathRequestManager.RequestNodeFromWorldPosition(_flagPosition.position); } }
    public List<Unit> units { get { return _currentUnits; } set { _currentUnits = value; } }
    public TeamEnum teamId { get { return _teamId; } }
    public Color teamColor { get { return _teamColor; } }
    //---------------------------------------------------------------
    private void Awake()
    {
        //Asignar unidades
        _currentUnits = _initialUnits;
    }

    /// <summary>
    /// Inicializar equipo
    /// </summary>
    public void InitTeam(TeamSet tSet)
    {
        //Init team values
        _teamId = tSet.teamId;
        _teamColor = tSet.teamColor;

        //init units
        foreach (Unit u in units) u.InitUnit(this);
    }

    /// <summary>
    /// Eliminar unidad del equipo cuando esta muere
    /// </summary>
    /// <param name="unit">unidad a eliminar</param>
    public void KillUnit(Unit unit)
    {
        _currentUnits.Remove(unit);
    }

    /// <summary>
    /// Comprobar si una unidad pertenece a un equipo concreto
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool IsUnitInTeam(Unit unit)
    {
        if (_currentUnits.Count > 0)
            return _currentUnits.Contains(unit);

        return false;
    }

    /// <summary>
    /// Habilitar todas las unidades del equipo
    /// </summary>
    public void EnableUnits(bool isEnabled)
    {
        foreach(Unit u in _currentUnits)
        {
            u.EnableUnit(isEnabled);
        }
    }

    /// <summary>
    /// Cuenta las unidades y roles para barajar cambios
    /// </summary>
    public void RoleChanges()
    {
        int n = _currentUnits.Count;
        int atk = 0;
        int def = 0;
        int med = 0;
        foreach (Unit u in _currentUnits)
        {
            switch (u.role)
            {
                case StrategicRole.Forward:
                    atk++;
                    break;
                case StrategicRole.Defense:
                    def++;
                    break;
                default:
                    med++;
                    break;
            }
        }
        if(med > 0)
        {
            if (atk < 6) AssignNewRole(StrategicRole.Center, StrategicRole.Forward);
            if (def < 6) AssignNewRole(StrategicRole.Center, StrategicRole.Defense);
        }
    }

    private void AssignNewRole(StrategicRole prevRole, StrategicRole newRole)
    {
        List<Unit> prevRoleUnits = new List<Unit>();
        List<Unit> newRoleUnits = new List<Unit>();

        foreach (Unit u in _currentUnits)
        {
            if (u.role == prevRole) prevRoleUnits.Add(u);
            else if (u.role == newRole) newRoleUnits.Add(u);
        }

        int sword = 0;
        int shield = 0;
        int lance = 0;
        UnitClass newUnit = UnitClass.Sword;

        foreach (Unit u in newRoleUnits)
        {
            switch (u.unitClass)
            {
                case UnitClass.Sword:
                    sword++;
                    break;
                case UnitClass.Shield:
                    shield++;
                    break;
                default:
                    lance++;
                    break;
            }
        }

        if (sword < shield || sword < lance) newUnit = UnitClass.Sword;
        else if (shield < sword || shield < lance) newUnit = UnitClass.Shield;
        else newUnit = UnitClass.Lance;

        Unit other = null;
        bool changed = false;

        foreach (Unit u in prevRoleUnits)
        {
            if (u.unitClass == newUnit)
            {
                u.role = newRole;
                changed = true;
                break;
            }
            else other = u;
        }

        if (!changed) other.role = newRole;
    }
}

[System.Serializable]
public class TeamSet : System.Object
{
    public TeamEnum teamId;
    public Color teamColor;
}

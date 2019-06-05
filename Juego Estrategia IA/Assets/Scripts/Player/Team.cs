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
        foreach (Unit u in units)
            u.InitUnit(this);
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
}

[System.Serializable]
public class TeamSet : System.Object
{
    public TeamEnum teamId;
    public Color teamColor;
}

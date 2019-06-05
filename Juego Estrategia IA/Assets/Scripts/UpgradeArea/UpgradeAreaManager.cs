using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeAreaManager : MonoBehaviour
{
    public static UpgradeAreaManager instance;


    public UpgradeArea[] _upgradeAreas;
    public UpgradeArea[] _initialAreas;

    [Tooltip("Rango de tiempo para activar el area")]
    public Vector2Int _activateAreaTime;

    public int _maxActiveAreas = 3;
    private int _activeAreasCounter;

    public int _currentTurn;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _currentTurn = 0;
        DisableAllAreas();
        ActivateAreas(_initialAreas);
    }

    public void UpdateManager(int currentTurn)
    {
        if(_activeAreasCounter < _maxActiveAreas)
        {
            ActivateAreas(_maxActiveAreas - _activeAreasCounter);
        }

        _currentTurn = currentTurn;
    }

    private void ActivateAreas(int n)
    {
        for(int i = 0; i < n; i++)
        {
            UpgradeArea area = GetRandomDisabledArea();
            UnitClass uClass = (UnitClass)Random.Range(0, 4);
            area.SetAreaClass(uClass);

            int turns = Random.Range(_activateAreaTime.x, _activateAreaTime.y);

            StartCoroutine(ActivateArea(turns, area));
            _activeAreasCounter++;
        }
    }

    private void ActivateAreas(UpgradeArea[] areas)
    {
        foreach(UpgradeArea area in areas)
        {
            UnitClass uClass = (UnitClass)Random.Range(0, 4);
            area.SetAreaClass(uClass);

            StartCoroutine(ActivateArea(0, area));
            _activeAreasCounter++;
        }
    }

    private void DisableAllAreas()
    {
        foreach (UpgradeArea area in _upgradeAreas)
            area.EnableArea(false);
    }

    public void DisableArea(UpgradeArea area)
    {
        area.EnableArea(false);
        _activeAreasCounter--;
    }

    private UpgradeArea GetRandomDisabledArea()
    {
        UpgradeArea area = null;

        while(area == null)
        {
            int i = Random.Range(0, _upgradeAreas.Length);

            if (!_upgradeAreas[i].isAvaible)
                area = _upgradeAreas[i];
        }

        return area;
    }

    private IEnumerator ActivateArea(int turns, UpgradeArea area)
    {
        int activatingTurn = _currentTurn + turns;

        while(_currentTurn < activatingTurn)
        {
            yield return null;
        }

        area.EnableArea(true);
    }


}

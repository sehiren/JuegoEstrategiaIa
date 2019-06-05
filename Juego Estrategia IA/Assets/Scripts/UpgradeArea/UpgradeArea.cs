using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeArea : MonoBehaviour
{
    public Sprite swordClassSprite;
    public Sprite shieldClassSprite;
    public Sprite lanceClassSprite;

    public UnitClass settingClass;

    private Orb areaOrb;
    private UnitClass currentClass;

    [SerializeField]private bool _isAvaiable;
    public bool isAvaible { get { return _isAvaiable; } }

    private Animator anim;

    private void Awake()
    {
        areaOrb = GetComponentInChildren<Orb>();
        anim = GetComponent<Animator>();
    }

    public void EnableArea(bool isEnabled)
    {
        anim.SetBool("IsActive", isEnabled);
        _isAvaiable = isEnabled;
    }

    public void SetAreaClass(UnitClass unitClass)
    {
        switch (unitClass)
        {
            case UnitClass.Sword:
                areaOrb.SetOrbType(swordClassSprite);
                break;
            case UnitClass.Shield:
                areaOrb.SetOrbType(shieldClassSprite);
                break;
            case UnitClass.Lance:
                areaOrb.SetOrbType(lanceClassSprite);
                break;
            default:
                areaOrb.SetOrbType(null);
                break;
        }

        currentClass = unitClass;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_isAvaiable && other.tag == "Unit")
        {
            Unit unit = other.GetComponent<Unit>();

            if(currentClass == UnitClass.Neutral || unit.unitClass == currentClass)
            {
                unit.unitLevel++;
                UpgradeAreaManager.instance.DisableArea(this);
            }
        }
    }

    private void OnValidate()
    {
        if(areaOrb == null)
            areaOrb = GetComponentInChildren<Orb>();

        SetAreaClass(settingClass);
    }
}
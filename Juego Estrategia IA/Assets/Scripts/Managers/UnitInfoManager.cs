using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitInfoManager : MonoBehaviour
{
    public static UnitInfoManager instance;

    [SerializeField] private GameObject _infoMenu;

    [Header("Unit Level Info references")]
    [SerializeField] private GameObject _levelUI;
    [SerializeField] private TMP_Text _levelText;

    [Header("Unit advantage info references")]
    [SerializeField] private GameObject _advantageUI;
    [SerializeField] private GameObject _equalsSprite;
    [SerializeField] private GameObject _greaterSprite;
    [SerializeField] private GameObject _loweSprite;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        HideAdvantageInfo();
        HideLevelInfo();
    }

    public void ShowLevelInfo(Unit unit)
    {
        _levelText.text = unit.unitLevel.ToString();

        Vector3 unitPos = unit.transform.position;
        unitPos.y = _infoMenu.transform.position.y;

        _infoMenu.transform.position = unitPos;
        _levelUI.SetActive(true);
    }

    public void HideLevelInfo()
    {
        _levelUI.SetActive(false);
    }

    public void ShowAdvantageInfo(Unit attacker, Unit defender)
    {
        float attackerPower = attacker.GetUnitPower(defender);
        float defenderPower = defender.GetUnitPower(attacker);

        if (attackerPower > defenderPower)
        {
            _equalsSprite.SetActive(false);
            _loweSprite.SetActive(false);
            _greaterSprite.SetActive(true);
        }
        else if (defenderPower > attackerPower)
        {
            _greaterSprite.SetActive(false);
            _equalsSprite.SetActive(false);
            _loweSprite.SetActive(true);
        }
        else
        {
            _greaterSprite.SetActive(false);
            _loweSprite.SetActive(false);
            _equalsSprite.SetActive(true);
        }

        _advantageUI.SetActive(true);
    }

    public void HideAdvantageInfo()
    {
        _greaterSprite.SetActive(false);
        _loweSprite.SetActive(false);
        _equalsSprite.SetActive(false);

        _advantageUI.SetActive(false);
    }
}

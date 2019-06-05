using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : TeamItem
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Unit")
        {
            Unit winnerUnit = other.GetComponent<Unit>();
            LevelController.instance.PlayerInEnemyFlag(winnerUnit);
        }
    }
}

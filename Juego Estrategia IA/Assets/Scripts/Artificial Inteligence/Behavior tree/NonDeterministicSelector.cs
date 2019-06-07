using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Random Selector", menuName = Task.MENU_NAME + Task.COMPOSITES + "Random Selector", order = 1)]
public class NonDeterministicSelector : Task
{
    public override TaskResult Run()
    {
        throw new System.NotImplementedException();
    }
}

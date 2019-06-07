using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Random Sequence", menuName = Task.MENU_NAME + Task.COMPOSITES + "Random Sequence", order = 1)]
public class NonDeterministicSequence : Task
{
    public override TaskResult Run()
    {
        throw new System.NotImplementedException();
    }
}

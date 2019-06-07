using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Random Selector", menuName = Task.MENU_NAME + Task.COMPOSITES + "Random Selector", order = 1)]
public class NonDeterministicSelector : Task
{
    [SerializeField] private Task[] _children;

    public override TaskResult Run()
    {
        Task[] shuffled = Task.Shuffle(_children);

        foreach (Task child in shuffled)
            if (child.Run() == TaskResult.Success)
                return TaskResult.Success;

        return TaskResult.Failure;
    }
}

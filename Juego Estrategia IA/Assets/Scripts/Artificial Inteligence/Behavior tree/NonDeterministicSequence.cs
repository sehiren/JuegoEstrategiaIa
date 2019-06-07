using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Random Sequence", menuName = Task.MENU_NAME + Task.COMPOSITES + "Random Sequence", order = 1)]
public class NonDeterministicSequence : Task
{
    [SerializeField] private Task[] _children;

    public override TaskResult Run()
    {
        Task[] shuffled = Task.Shuffle(_children);

        foreach (Task child in _children)
            if (child.Run() == TaskResult.Failure)
                return TaskResult.Failure;

        return TaskResult.Success;
    }
}

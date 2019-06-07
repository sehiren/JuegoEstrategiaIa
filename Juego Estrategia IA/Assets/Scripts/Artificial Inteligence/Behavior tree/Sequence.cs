using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Sequencer", menuName = Task.MENU_NAME + Task.COMPOSITES + "Sequence", order = 0)]
public class Sequence : Task
{
    [Tooltip("Tareas a ejecutar por el sequence"), SerializeField]
    private Task[] _children;

    public override TaskResult Run()
    {
        foreach (Task child in _children)
            if (child.Run() == TaskResult.Failure)
                return TaskResult.Failure;

        return TaskResult.Success;
    }
}

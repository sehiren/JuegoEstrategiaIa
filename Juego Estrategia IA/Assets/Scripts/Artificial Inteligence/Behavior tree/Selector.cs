using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Selector", menuName = Task.MENU_NAME + Task.COMPOSITES + "Selector", order = 0)]
public class Selector : Task
{
    [Tooltip("Tareas hijas a ejecutar por el selector"), SerializeField]
    private Task[] _children;

    public override TaskResult Run()
    {
        foreach (Task child in _children)
            if (child.Run() == TaskResult.Success)
                return TaskResult.Success;

        return TaskResult.Failure;
    }
}

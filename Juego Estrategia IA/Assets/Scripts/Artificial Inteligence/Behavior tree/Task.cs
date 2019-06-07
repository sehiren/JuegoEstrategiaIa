using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task : ScriptableObject
{
    public const string MENU_NAME = "Behavior Tree/";
    public const string COMPOSITES = "Composites/";
    public const string CONDITIONS = "Conditions/";
    public const string ACTIONS = "Actions/";

    public abstract TaskResult Run();

    public static Task[] Shuffle (Task[] original)
    {
        Task[] list = (Task[]) original.Clone();
        int n = list.Length;
        
        while(n > 1)
        {
            int k = Random.Range(0, n);
            n--;

            Task aux = list[n];
            list[n] = list[k];
            list[k] = aux;
        }

        return list;
    }
}


public enum TaskResult
{
    Success, Failure
}

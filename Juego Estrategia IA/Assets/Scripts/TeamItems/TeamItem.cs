using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamItem : MonoBehaviour
{
    private GridNode _currentNode;
    public GridNode currentNode { get { return _currentNode; } set { _currentNode = value; } }
}

public enum TeamEnum
{
    Red, Blue
}

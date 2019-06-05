using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawGrid : MonoBehaviour
{
    [Header("Tile Colors")]
    public Color disabledColor;
    public Color avaibleColor;
    public Color occupiedNodeColor;
    public Color targetNodeColor;

    [Header("References")]
    public GameObject nodePrefab;
    public GameObject gridLayout;

    private Image[] nodesSprites;

    private Grid _grid;
    private int _gridSizeX;
    private int _gridSizeY;

    public static DrawGrid instance;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        bool endApplication = false;
        if (nodePrefab == null)
        {
            Debug.LogError("Falta referencia al prefab de los Tiles");
            endApplication = true;
        }

        if (gridLayout == null)
        {
            Debug.LogError("Falta referencia a la gridLayout");
            endApplication = true;
        }

        if (endApplication)
            Application.Quit();
    }

    public void CreateGrid(Grid grid, int gridSizeX, int gridSizeY)
    {
        _grid = grid;
        _gridSizeY = gridSizeY;
        _gridSizeX = gridSizeX;

        nodesSprites = new Image[_grid.MaxSize];

        for (int i = 0; i < _grid.MaxSize; i++)
        {
            
            GameObject tile = Instantiate(nodePrefab, gridLayout.transform);

            nodesSprites[i] = tile.GetComponent<Image>();
            nodesSprites[i].color = disabledColor;         
        }
    }

    public void DrawNode(int x, int y, NodeState state)
    {
        int i = (_gridSizeX * y) + x;
        Color nodeColor;

        switch(state)
        {
            case NodeState.Available:
            case NodeState.EnemyFlag:
                nodeColor = avaibleColor;
                break;
            case NodeState.FriendlyFlag:
            case NodeState.FriendlyUnit:
                nodeColor = disabledColor;
                break;
            case NodeState.EnemyUnit:
                nodeColor = occupiedNodeColor;
                break;
            default:
                nodeColor = disabledColor;
                break;
        }

        nodesSprites[i].color = nodeColor; ;
    }

    public void HideNode(int x, int y)
    {
        int i = (_gridSizeX * y) + x;
        nodesSprites[i].color = disabledColor;
    }
}

public enum NodeState
{
    Available, EnemyUnit, FriendlyUnit, EnemyFlag, FriendlyFlag
}

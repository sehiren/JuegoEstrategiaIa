using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDebug : MonoBehaviour
{
    private Grid _grid;
    public bool showGizmos = false;
    public bool showCenter = false;





    private void OnDrawGizmos()
    {
        if (_grid == null)
            _grid = GetComponent<Grid>();

        int gridSizeX = Mathf.FloorToInt(_grid.gridWorldSize.x / (_grid.nodeRadius * 2));
        int gridSizeY = Mathf.RoundToInt(_grid.gridWorldSize.y / (_grid.nodeRadius * 2));

        if (showGizmos)
        {
            Vector3 worldBottomLeft = transform.position - Vector3.right * _grid.gridWorldSize.x / 2 - Vector3.forward * _grid.gridWorldSize.y / 2;

            for(int x = 0; x < gridSizeX; x++)
            {
                for(int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPos = worldBottomLeft + Vector3.right * (x * _grid.nodeRadius * 2 + _grid.nodeRadius) + Vector3.forward * (y * _grid.nodeRadius * 2 + _grid.nodeRadius);

                    Gizmos.color = Color.gray;
                    Gizmos.DrawCube(worldPos, new Vector3(_grid.nodeRadius * 2 - .1f, .1f, _grid.nodeRadius * 2 - .1f));
                }
            }
        }
        if (showCenter)
        {
            Gizmos.color = Color.red;
            //Vertical middle points
            Gizmos.DrawLine(transform.position + transform.forward * gridSizeY,
                transform.position - transform.forward * gridSizeY);
            
            //Horizontal middle points
            Gizmos.DrawLine(transform.position + transform.right * gridSizeX,
                transform.position - transform.right * gridSizeX);
        }
    }
}

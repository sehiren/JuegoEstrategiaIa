using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class PathFinding : MonoBehaviour
{
    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)//Is now a corutine
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        GridNode startNode = grid.NodeFromWorldPoint(request.pathStart); //the set of nodes to be evaluated
        GridNode targetNode = grid.NodeFromWorldPoint(request.pathEnd); //the set of nodes already evaluated
        startNode.parent = startNode;

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<GridNode> openSet = new Heap<GridNode>(grid.MaxSize);
            HashSet<GridNode> closedSet = new HashSet<GridNode>();
            openSet.Add(startNode); //the star position, normally the one where the player is

            while (openSet.Count > 0)
            {

                GridNode currentnode = openSet.RemoveFirst();
                closedSet.Add(currentnode);

                if (currentnode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (GridNode neighbour in grid.GetNeighbours(currentnode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))//if neighbour is not traversable or if it is in CLOSED
                    {
                        continue;//skip to the next 
                    }

                    int newMovementCostToNeighbour = currentnode.gCost + GetDistance(currentnode, neighbour) + neighbour.movementPenalty;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) //if it comes up that the new calculation is lesser than the one done before, we change the value to show this (we are acceding this node from a shorter path, and this is now the currently optimal for the neighboar node -> we update it)
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentnode;

                        if (!openSet.Contains(neighbour))//now we have to recalculate this node, even if we alredy did, because the weight has chan+ged
                            openSet.Add(neighbour);

                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Length > 0;
        }
        callback(new PathResult(waypoints, pathSuccess, request.callback));

    }

    public int BestPathLength(GridNode startNode, GridNode targetNode)//Is now a corutine
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        int res = int.MaxValue;

        startNode.parent = startNode;

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<GridNode> openSet = new Heap<GridNode>(grid.MaxSize);
            HashSet<GridNode> closedSet = new HashSet<GridNode>();
            openSet.Add(startNode); //the star position, normally the one where the player is

            while (openSet.Count > 0)
            {
                GridNode currentnode = openSet.RemoveFirst();
                closedSet.Add(currentnode);

                if (currentnode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (GridNode neighbour in grid.GetNeighbours(currentnode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))//if neighbour is not traversable or if it is in CLOSED
                    {
                        continue;//skip to the next 
                    }

                    int newMovementCostToNeighbour = currentnode.gCost + GetDistance(currentnode, neighbour) + neighbour.movementPenalty;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) //if it comes up that the new calculation is lesser than the one done before, we change the value to show this (we are acceding this node from a shorter path, and this is now the currently optimal for the neighboar node -> we update it)
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentnode;

                        if (!openSet.Contains(neighbour))//now we have to recalculate this node, even if we alredy did, because the weight has chan+ged
                            openSet.Add(neighbour);

                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
            res = closedSet.Count;
        }

        if (pathSuccess) return res;
        else return int.MaxValue;
    }

    public GridNode GetOptimalFurtherNode(List<GridNode> nodes, List<GridNode> menances, GridNode objective)
    {
        int maxDis = 0;
        int minDis = int.MaxValue;
        GridNode best = null;
        List<GridNode> furtherNodes = new List<GridNode>();

        foreach(GridNode g in nodes) // Guardar los nodos más alejados de la amenaza
        {
            Unit u = (Unit)g.isOccupied;
            bool leap = false;

            if (u != null)
            {
                if (u.tag == "Flag") leap = true; // si es bandera salta al siguiente nodo
                if (u.tag == "Unit")
                    if (!u.SameTeam((Unit)menances[0].isOccupied)) leap = true;  // si es aliado salta al siguiente nodo
            }

            if (!leap)
            {
                int dis = 0;

                foreach (GridNode menance in menances)
                {
                    dis += BestPathLength(g, menance);
                }

                if (dis > maxDis)
                {
                    maxDis = dis;
                    if (furtherNodes.Count != 0) furtherNodes.RemoveRange(0, furtherNodes.Count);
                    furtherNodes.Add(g);
                }
                else if (dis == maxDis) furtherNodes.Add(g);
            }
        }            

        // de los nodos más alejados de la amenaza, seleccionar el más cercano al objetivo
        if (furtherNodes.Count > 1)
        {
            foreach (GridNode g in furtherNodes)
            {
                int dis = BestPathLength(g, objective);
                if (dis < minDis)
                {
                    minDis = dis;
                    best = g;
                }
            }
        }
        else best = furtherNodes[0];

        return best;
    }

    public List<GridNode> GetAvaibleNodes(GridNode initialNode, int radius, Team team)
    {
        List<GridNode> avaliableNodes = new List<GridNode>();
        int distance = (radius - 1) * 10;

        Heap<GridNode> openSet = new Heap<GridNode>(grid.MaxSize);
        initialNode.gCost = 0;
        initialNode.parent = initialNode;
        openSet.Add(initialNode);

        while(openSet.Count > 0)
        {
            GridNode currentNode = openSet.RemoveFirst();
            avaliableNodes.Add(currentNode);

            foreach(GridNode neighbor in grid.GetNeighbours(currentNode))
            {
                if (!neighbor.walkable || avaliableNodes.Contains(neighbor))
                    continue;

                if(currentNode.isOccupied != null && currentNode != initialNode)
                {
                    if(currentNode.isOccupied.tag != "Unit")
                    {
                        if (team.flagPosition.worldPosition == currentNode.worldPosition)
                        {
                            continue;
                        }
                            
                    }
                    else
                    {
                        Unit unit = (Unit)currentNode.isOccupied;
                        if (unit.unitTeam == team)
                            continue;
                    }
                }

                int nodeCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                if(nodeCost < distance && (nodeCost < neighbor.gCost || !openSet.Contains(neighbor)))
                {
                    neighbor.gCost = nodeCost;
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else
                        openSet.UpdateItem(neighbor);
                }
            }
        }

        return avaliableNodes;
    }

    public GridNode GetNodeFromWorldPosition(Vector3 worldPos)
    {
        return grid.NodeFromWorldPoint(worldPos);
    }

    Vector3[] RetracePath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = endNode;

        while (currentNode != startNode) //recrorremos el path al reves, desde el final, para ir de padre en padre
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints); //ponemos el path bien 
        return waypoints;
    }

    Vector3[] SimplifyPath(List<GridNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    public int GetDistance(GridNode nodeA, GridNode nodeB)//get the distance between two given nodes
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 20 * dstY + 10 * (dstX - dstY); // 14 = al pes de distancia en diagonal, 10 = al pes de menejarse horizontal o vertical
        return 20 * dstX + 10 * (dstY - dstX);
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public LayerMask layers;
    public Vector2 size;
    public float nodeRadius;
    Node[,] grid;
    [HideInInspector]
    public List<Node> listNodes = new List<Node>();
    [HideInInspector]
    public List<Node> path;

    [HideInInspector]
    public float nodeDiameter;
    int sizeX;
    int sizeY;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        sizeX = Mathf.RoundToInt(size.x / nodeDiameter);
        sizeY = Mathf.RoundToInt(size.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[sizeX, sizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * size.x / 2 - Vector3.forward * size.y / 2;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, layers));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
                listNodes.Add(grid[x, y]);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.x + x;
                int checkY = node.y + y;

                if (checkX >= 0 && checkX < sizeX && checkY >= 0 && checkY < sizeY)
                    neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public Node GetClosestNode(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + size.x / 2) / size.x;
        float percentY = (worldPosition.z + size.y / 2) / size.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((sizeX - 1) * percentX);
        int y = Mathf.RoundToInt((sizeY - 1) * percentY);

        return grid[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 1, size.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.position, Vector3.one * (nodeDiameter - .1f));
            }
        }
    }
}
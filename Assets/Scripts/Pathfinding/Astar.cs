using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour
{
    public Grid grid;
    public List<Node> myPath = new List<Node>();
    Player _player;

    void Awake()
    {
        _player = FindObjectOfType<Player>();
    }

    public IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node start = grid.GetClosestNode(startPos);
        Node target = grid.GetClosestNode(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            Node node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
                if (openSet[i].F < node.F || openSet[i].F == node.F)
                    if (openSet[i].H < node.H)
                        node = openSet[i];

            openSet.Remove(node);
            closedSet.Add(node);

            if (node == target)
            {
                ThetaStar(start, target);

                yield return new WaitForEndOfFrame();

                _player.astarPath.AddRange(myPath);
            }

            foreach (Node n in grid.GetNeighbours(node))
            {
                if (!n.walkable || closedSet.Contains(n))
                    continue;

                int cost = node.G + GetDistance(node, n);
                if (cost < n.G || !openSet.Contains(n))
                {
                    n.G = cost;
                    n.H = GetDistance(n, target);
                    n.parent = node;

                    if (!openSet.Contains(n))
                        openSet.Add(n);
                }
            }
        }
    }

    void ThetaStar(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        myPath.Clear();

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            myPath.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        myPath.Reverse();
        grid.path = path;
    }

    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.x - b.x);
        int dstY = Mathf.Abs(a.y - b.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (var n in myPath)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(n.position, Vector3.one * (grid.nodeDiameter - .1f));
            }
        }
    }
}

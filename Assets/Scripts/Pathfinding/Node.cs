using UnityEngine;
using System.Collections;

public class Node
{

    public bool walkable;
    public Vector3 position;
    public int x;
    public int y;

    public int G;
    public int H;

    public Node parent;

    public Node(bool w, Vector3 pos, int X, int Y)
    {
        walkable = w;
        position = pos;
        x = X;
        y = Y;
    }

    public int F
    {
        get
        {
            return G + H;
        }
    }
}

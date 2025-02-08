using UnityEngine;

public class AStarNode
{
    public Vector3 position;
    public Vector2Int gridPosition;
    public AStarNode parent;
    public float g;
    public float h;
    public float f => g + h;

    public AStarNode(Vector3 pos, Vector2Int gridPos)
    {
        position = pos;
        gridPosition = gridPos;
    }
}
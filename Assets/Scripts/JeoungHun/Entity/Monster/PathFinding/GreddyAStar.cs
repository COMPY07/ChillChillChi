using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GreedyAStar
{
    #region Fields
    public Transform agent;
    public bool state;
    public float speed = 2f;
    public AStarNode prev;
    
    private Transform target;
    private Transform pathTarget;
    private float pathUpdateInterval = 0.1f;
    private const float NODE_RADIUS = 0.25f;
    private const int MAX_ITERATIONS = 1000;
    private Vector2Int? lastDirection = null;
    private float directionChangeThreshold = 0.5f;
    
    private static readonly Vector2Int[] DIRECTIONS = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1)
    };
    
    private readonly List<AStarNode> openSet = new List<AStarNode>();
    private readonly HashSet<AStarNode> closedSet = new HashSet<AStarNode>();
    private readonly Dictionary<Vector2Int, AStarNode> nodeCache = new Dictionary<Vector2Int, AStarNode>();
    private const float MAX_PATH_DISTANCE = 100f;
    private const int GRID_SEARCH_LIMIT = 1000;
    #endregion

    #region Public Methods
    public void Set(Transform agent, Transform target, Transform pathTarget)
    {
        this.agent = agent;
        this.target = target;
        this.pathTarget = pathTarget;
    }

    public void SetPathUpdateInterval(float interval)
    {
        pathUpdateInterval = Mathf.Clamp(interval, 0.05f, 0.5f);
    }

    public IEnumerator Chase()
    {
        while (state && agent != null && target != null)
        {
            Vector3 nextPosition = CalculateNextPosition();
            if (nextPosition != agent.position) MoveTowardsPosition(nextPosition);
            yield return new WaitForSeconds(pathUpdateInterval);
        }
    }
    #endregion

    #region Pathfinding
    private Vector3 CalculateNextPosition()
    {
        Vector3 targetPos = target.position;
        Vector3 agentPos = agent.position;
        
        AStarNode startNode = GetNode(agentPos);
        AStarNode targetNode = GetNode(targetPos);
        
        List<AStarNode> path = FindPath(startNode, targetNode);
        if (path != null && path.Count > 0)
        {
            prev = path[0];
            return SmoothPosition(path);
        }
        
        return agentPos;
    }
    private float Vector2IntDot(Vector2Int a, Vector2Int b)
    {
        return a.x * b.x + a.y * b.y;
    }
    private List<Vector2Int> GetValidDirections(Vector2Int currentPos, Vector2Int? preferredDirection = null)
    {
        List<Vector2Int> validDirections = new List<Vector2Int>();
    
        var sortedDirections = DIRECTIONS
            .OrderBy(dir => preferredDirection.HasValue ? 
                -Vector2IntDot(dir, preferredDirection.Value) : 0)
            .ToList();
    
        foreach (var direction in sortedDirections)
        {
            Vector2Int nextPos = currentPos + direction;
            Vector3 worldPos = new Vector3(nextPos.x * NODE_RADIUS, nextPos.y * NODE_RADIUS, 0);
        
            if (!IsPositionValid(worldPos)) continue;
        
            if (direction.x != 0 && direction.y != 0)
            {
                Vector2Int horizontal = currentPos + new Vector2Int(direction.x, 0);
                Vector2Int vertical = currentPos + new Vector2Int(0, direction.y);
            
                Vector3 horizontalPos = new Vector3(horizontal.x * NODE_RADIUS, horizontal.y * NODE_RADIUS, 0);
                Vector3 verticalPos = new Vector3(vertical.x * NODE_RADIUS, vertical.y * NODE_RADIUS, 0);
            
                if (!IsPositionValid(horizontalPos) || !IsPositionValid(verticalPos))
                {
                    continue;
                }
            }
        
            validDirections.Add(direction);
        }
    
        return validDirections;
    }

    private List<AStarNode> FindPath(AStarNode start, AStarNode end)
    {
        if (Vector3.Distance(start.position, end.position) < NODE_RADIUS)
        {
            return new List<AStarNode> { end };
        }

        openSet.Clear();
        closedSet.Clear();
        
        start.g = 0;
        start.h = GetHeuristicCost(start, end);
        openSet.Add(start);
        
        Vector2Int targetDirection = new Vector2Int(
            Mathf.RoundToInt(Mathf.Sign(end.position.x - start.position.x)),
            Mathf.RoundToInt(Mathf.Sign(end.position.y - start.position.y))
        );
        
        int iterations = 0;
        
        while (openSet.Count > 0 && iterations < GRID_SEARCH_LIMIT)
        {
            iterations++;
            AStarNode current = GetNodeWithLowestFCost();
            
            if (current == end || Vector3.Distance(current.position, end.position) < NODE_RADIUS)
            {
                var path = ReconstructPath(start, current);
                if (path.Count >= 2)
                {
                    var first = path[0];
                    var second = path[1];
                    lastDirection = new Vector2Int(
                        Mathf.RoundToInt(Mathf.Sign(second.position.x - first.position.x)),
                        Mathf.RoundToInt(Mathf.Sign(second.position.y - first.position.y))
                    );
                }
                return path;
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            Vector2Int preferredDirection = lastDirection ?? targetDirection;
            List<Vector2Int> validDirections = GetValidDirections(current.gridPosition, preferredDirection);
            
            foreach (var direction in validDirections)
            {
                Vector2Int neighborPos = current.gridPosition + direction;
                Vector3 worldPos = new Vector3(
                    neighborPos.x * NODE_RADIUS,
                    neighborPos.y * NODE_RADIUS,
                    0
                );
                
                AStarNode neighbor = GetNode(worldPos);
                if (closedSet.Contains(neighbor)) continue;
                
                float directionChangePenalty = 0;
                if (lastDirection.HasValue)
                {
                    float dotProduct = Vector2.Dot(direction, lastDirection.Value);
                    directionChangePenalty = (1 - dotProduct) * directionChangeThreshold;
                }
                
                float movementCost = direction.x != 0 && direction.y != 0 ? 1.4f : 1.0f;
                float tentativeG = current.g + movementCost + directionChangePenalty;
                
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                    neighbor.parent = current;
                    neighbor.g = tentativeG;
                    neighbor.h = GetHeuristicCost(neighbor, end);
                }
                else if (tentativeG < neighbor.g)
                {
                    neighbor.parent = current;
                    neighbor.g = tentativeG;
                }
            }
        }
        
        return null;
    }
    #endregion

    #region Movement
    private void MoveTowardsPosition(Vector3 targetPosition)
    {
        if (agent == null) return;
        
        Vector3 direction = (targetPosition - agent.position).normalized;
        float currentSpeed = speed;
        
        if (lastDirection.HasValue)
        {
            Vector2 currentDir2D = new Vector2(direction.x, direction.y);
            Vector2 lastDir2D = new Vector2(lastDirection.Value.x, lastDirection.Value.y);
            float dotProduct = Vector2.Dot(currentDir2D.normalized, lastDir2D.normalized);
            float speedMultiplier = Mathf.Lerp(0.5f, 1f, (dotProduct + 1f) / 2f);
            currentSpeed *= speedMultiplier;
        }
        
        Vector3 newPosition = agent.position + direction * currentSpeed * Time.deltaTime;
        agent.position = newPosition;
    }

    private Vector3 SmoothPosition(List<AStarNode> path)
    {
        if (path.Count <= 1) return path[0].position;
    
        Vector3 currentPos = agent.position;
        Vector3 nextPos = path[0].position;
        Vector3 nextNextPos = path.Count > 1 ? path[1].position : nextPos;
        
        float distanceToNext = Vector3.Distance(currentPos, nextPos);
        float t = Mathf.Clamp01(NODE_RADIUS / distanceToNext);
    
        Vector3 smoothedPos = Vector3.Lerp(
            Vector3.Lerp(currentPos, nextPos, t),
            Vector3.Lerp(nextPos, nextNextPos, t),
            t
        );
    
        return smoothedPos;
    }
    #endregion

    #region Utility
    private float GetHeuristicCost(AStarNode a, AStarNode b)
    {
        int dx = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int dy = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
        return 1.0f * (dx + dy) + (1.4f - 2 * 1.0f) * Mathf.Min(dx, dy);
    }

    private bool IsPositionValid(Vector3 worldPos)
    {
        Vector2 pos2D = new Vector2(worldPos.x, worldPos.y);
        int wallLayer = LayerMask.GetMask("Wall");
        var hitCollider = Physics2D.OverlapPoint(pos2D, wallLayer);
        return hitCollider == null;
    }

    private AStarNode GetNode(Vector3 worldPosition)
    {
        Vector2Int gridPosition = new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / NODE_RADIUS),
            Mathf.RoundToInt(worldPosition.y / NODE_RADIUS)
        );
    
        if (!nodeCache.TryGetValue(gridPosition, out AStarNode node))
        {
            node = new AStarNode(
                new Vector3(
                    gridPosition.x * NODE_RADIUS,
                    gridPosition.y * NODE_RADIUS,
                    0
                ),
                gridPosition
            );
            nodeCache[gridPosition] = node;
        }
    
        return node;
    }

    private AStarNode GetNodeWithLowestFCost()
    {
        return openSet.OrderBy(n => n.f).ThenBy(n => n.h).First();
    }

    private List<AStarNode> ReconstructPath(AStarNode start, AStarNode end)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode current = end;
        
        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        
        path.Reverse();
        return path;
    }
    #endregion
}
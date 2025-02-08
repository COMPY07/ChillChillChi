// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class GridPathfinding : MonoBehaviour
// {
//     private class PathNode : IComparable<PathNode>
//     {
//         public Vector2Int GridPosition { get; private set; }
//         public Vector2 WorldPosition => new Vector2(
//             GridPosition.x * GRID_SIZE,
//             GridPosition.y * GRID_SIZE
//         );
//         
//         public float GCost { get; set; }
//         public float HCost { get; set; }
//         public float FCost => GCost + HCost;
//         public PathNode Parent { get; set; }
//         
//         public PathNode(Vector2 gridPos) {
//             
//             GridPosition = gridPos;
//         }
//
//         public int CompareTo(PathNode other)
//         {
//             int comparison = FCost.CompareTo(other.FCost);
//             return comparison != 0 ? comparison : HCost.CompareTo(other.HCost);
//         }
//     }
//     // 그리드 설정
//     private const float GRID_SIZE = 1f;
//     private const float MAX_SEARCH_DISTANCE = 50f;
//     private const int MAX_ITERATIONS = 1000;
//     
//     // 성능 최적화 설정
//     private const float PATH_UPDATE_INTERVAL = 0.2f;
//     private const float POSITION_UPDATE_THRESHOLD = 0.5f;
//     private const int PATH_CACHE_TIME = 2;
//
//     // 이동 및 경로 관련 변수
//     private List<Vector2> currentPath;
//     private int pathIndex;
//     private Vector2 lastTargetPosition;
//     private Vector2 lastAgentPosition;
//     private bool isMoving;
//     private Vector2 currentMoveDirection;
//
//     // 컴포넌트 참조
//     public Transform target;
//     private Transform agentTransform;
//
//     // 속성들
//     public float moveSpeed = 5f;
//     public bool HasValidPath => currentPath != null && currentPath.Count > 0;
//     public Vector2 CurrentMoveDirection => currentMoveDirection;
//     public bool IsMoving => isMoving;
//     public int CurrentPathLength => currentPath?.Count ?? 0;
//     public int CurrentPathIndex => pathIndex;
//
//     private void Start()
//     {
//         InitializeComponents();
//         StartCoroutines();
//     }
//
//     private void InitializeComponents()
//     {
//         agentTransform = transform;
//         lastTargetPosition = target ? target.position : Vector2.zero;
//         lastAgentPosition = transform.position;
//         currentPath = new List<Vector2>();
//     }
//
//     private void StartCoroutines()
//     {
//         StartCoroutine(PathfindingRoutine());
//         StartCoroutine(MovementRoutine());
//     }
//
//     private bool IsWalkable(Vector2 position)
//     {
//         int wallLayer = LayerMask.GetMask("Wall");
//         return !Physics2D.OverlapCircle(position, GRID_SIZE * 0.4f, wallLayer);
//     }
//
//     private List<Vector2> FindPath(Vector2 start, Vector2 end)
//     {
//         if (Vector2.Distance(start, end) > MAX_SEARCH_DISTANCE)
//             return null;
//
//         List<Vector2> path = new List<Vector2>();
//         HashSet<Vector2> visited = new HashSet<Vector2>();
//         PriorityQueue<PathNode> frontier = new PriorityQueue<PathNode>();
//
//         // 시작 노드 초기화
//         PathNode startNode = new PathNode(start);
//         frontier.Enqueue(startNode);
//
//         Dictionary<Vector2, PathNode> cameFrom = new Dictionary<Vector2, PathNode>();
//         Dictionary<Vector2, float> costSoFar = new Dictionary<Vector2, float>();
//
//         cameFrom[start] = null;
//         costSoFar[start] = 0;
//
//         int iterations = 0;
//         while (frontier.Count > 0 && iterations < MAX_ITERATIONS)
//         {
//             PathNode current = frontier.Dequeue();
//             Vector2 currentPos = current.WorldPosition;
//
//             if (Vector2.Distance(currentPos, end) < GRID_SIZE * 0.5f)
//             {
//                 // 경로 찾음, 역추적
//                 Vector2 backtrackPos = currentPos;
//                 while (backtrackPos != start)
//                 {
//                     path.Add(backtrackPos);
//                     backtrackPos = cameFrom[backtrackPos].WorldPosition;
//                 }
//                 path.Add(start);
//                 path.Reverse();
//                 return path;
//             }
//
//             // 8방향 탐색
//             for (int x = -1; x <= 1; x++)
//             {
//                 for (int y = -1; y <= 1; y++)
//                 {
//                     if (x == 0 && y == 0) continue;
//
//                     Vector2 nextPos = currentPos + new Vector2(x, y) * GRID_SIZE;
//                     
//                     if (!IsWalkable(nextPos) || visited.Contains(nextPos))
//                         continue;
//
//                     float newCost = costSoFar[currentPos];
//                     if (x != 0 && y != 0)
//                         newCost += 1.414f * GRID_SIZE; // 대각선 비용
//                     else
//                         newCost += GRID_SIZE;
//
//                     if (!costSoFar.ContainsKey(nextPos) || newCost < costSoFar[nextPos])
//                     {
//                         costSoFar[nextPos] = newCost;
//                         float priority = newCost + Vector2.Distance(nextPos, end);
//                         PathNode nextNode = new PathNode(nextPos) { GCost = newCost };
//                         frontier.Enqueue(nextNode);
//                         cameFrom[nextPos] = current;
//                     }
//                 }
//             }
//
//             visited.Add(currentPos);
//             iterations++;
//         }
//
//         return null;
//     }
//
//     private IEnumerator PathfindingRoutine()
//     {
//         var wait = new WaitForSeconds(PATH_UPDATE_INTERVAL);
//         
//         while (enabled)
//         {
//             if (target != null)
//             {
//                 Vector2 currentPos = transform.position;
//                 Vector2 targetPos = target.position;
//
//                 // 위치가 충분히 변경되었을 때만 새 경로 계산
//                 if (Vector2.Distance(targetPos, lastTargetPosition) > POSITION_UPDATE_THRESHOLD ||
//                     Vector2.Distance(currentPos, lastAgentPosition) > POSITION_UPDATE_THRESHOLD)
//                 {
//                     var newPath = FindPath(currentPos, targetPos);
//                     if (newPath != null && newPath.Count > 0)
//                     {
//                         currentPath = newPath;
//                         pathIndex = 0;
//                         isMoving = true;
//                     }
//
//                     lastTargetPosition = targetPos;
//                     lastAgentPosition = currentPos;
//                 }
//             }
//             
//             yield return wait;
//         }
//     }
//
//     private IEnumerator MovementRoutine()
//     {
//         while (enabled)
//         {
//             if (isMoving && currentPath != null && pathIndex < currentPath.Count)
//             {
//                 Vector2 currentPos = transform.position;
//                 Vector2 targetPos = currentPath[pathIndex];
//                 
//                 float distance = Vector2.Distance(currentPos, targetPos);
//                 
//                 if (distance < GRID_SIZE * 0.1f)
//                 {
//                     pathIndex++;
//                     if (pathIndex >= currentPath.Count)
//                     {
//                         isMoving = false;
//                         currentMoveDirection = Vector2.zero;
//                     }
//                 }
//                 else
//                 {
//                     currentMoveDirection = (targetPos - currentPos).normalized;
//                     transform.position += (Vector3)currentMoveDirection * moveSpeed * Time.deltaTime;
//                 }
//             }
//             else
//             {
//                 currentMoveDirection = Vector2.zero;
//             }
//             
//             yield return null;
//         }
//     }
//
//     private void OnDrawGizmos()
//     {
//         if (!Application.isPlaying) return;
//
//         // 현재 경로 표시
//         if (currentPath != null && currentPath.Count > 0)
//         {
//             Gizmos.color = Color.yellow;
//             for (int i = 0; i < currentPath.Count - 1; i++)
//             {
//                 Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
//             }
//         }
//
//         // 이동 방향 표시
//         Gizmos.color = Color.blue;
//         Gizmos.DrawRay(transform.position, currentMoveDirection);
//
//         // 그리드 표시
//         Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
//         Vector2 center = transform.position;
//         float size = 10f;
//         
//         for (float x = -size; x <= size; x += GRID_SIZE)
//         {
//             for (float y = -size; y <= size; y += GRID_SIZE)
//             {
//                 Vector2 pos = new Vector2(
//                     Mathf.Round(center.x / GRID_SIZE) * GRID_SIZE + x,
//                     Mathf.Round(center.y / GRID_SIZE) * GRID_SIZE + y
//                 );
//                 if (IsWalkable(pos))
//                 {
//                     Gizmos.DrawWireCube(pos, Vector3.one * GRID_SIZE * 0.9f);
//                 }
//             }
//         }
//     }
// }
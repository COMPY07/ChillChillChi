using UnityEngine;

public class BaseMonster : BaseEntity
{
    [Header("Basic Settings")]
    public float detectionRange = 10f;
    public float moveSpeed = 5f;
    public float chaseSpeed = 7f;
    public float rotationSpeed = 5f;

    [Header("AI Behavior")]
    public bool usePatrol = true;
    public float patrolRadius = 5f;
    public float waypointStopDistance = 0.1f;
    public float pathValidationInterval = 0.2f;

    [Header("Combat Settings")]
    public bool canAttack = true;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private LayerMask wallLayer;
    public float pathCheckRadius = 0.5f;

    private bool showDebugLogs = false;

    protected Transform player;
    protected Vector2 startPosition;
    protected bool isChasing;

    private Vector2 currentWaypoint;
    private float lastAttackTime;
    private float lastPathCheckTime;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool currentPathIsValid;

    protected virtual void Start()
    {
        InitializeComponents();
        SetupRigidbody();
        InitializeStartPosition();
        FindPlayer();

        LogDebug("Initialization", $"Monster initialized at position: {transform.position}, Floor Layer: {floorLayer.value}, Wall Layer: {wallLayer.value}");
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

        if (rb == null || col == null)
        {
            LogError("Components", "Missing required components! Rigidbody2D or CircleCollider2D not found!");
        }
    }

    private void SetupRigidbody()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            LogDebug("Rigidbody", "Rigidbody2D setup completed");
        }
    }

    private void InitializeStartPosition()
    {
        startPosition = transform.position;
        SetNewPatrolPoint();
        LogDebug("Position", $"Start position set to: {startPosition}, Initial waypoint: {currentWaypoint}");
    }

    private void FindPlayer()
    {
        var playerObj = StageManager.Instance.GetPlayerObject().transform.GetChild(1).transform;
        if (playerObj != null)
        {
            player = playerObj.transform;
            LogDebug("Player", $"Player found at position: {player.position}");
        }
        else
        {
            LogError("Player", "Player not found in scene!");
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!IsPlayerValid())
        {
            LogWarning("Update", "Player reference is invalid, skipping update");
            return;
        }

        UpdatePathValidation();
        HandleMovement();

        if (showDebugLogs)
        {
            LogDebug("Status", $"Position: {transform.position}, IsChasing: {isChasing}, PathValid: {currentPathIsValid}");
        }
    }

    private bool IsPlayerValid()
    {
        return player != null;
    }

    private void UpdatePathValidation()
    {
        if (Time.time >= lastPathCheckTime + pathValidationInterval)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            bool inRange = distanceToPlayer <= detectionRange;
            bool pathToPlayerValid = inRange && ValidateMovePath(player.position);
            
            bool currentMoveValid = !isChasing || pathToPlayerValid;
            
            LogDebug("PathValidation", 
                $"Distance to player: {distanceToPlayer:F2}, " +
                $"In Range: {inRange}, " +
                $"Path to Player Valid: {pathToPlayerValid}, " +
                $"Current Move Valid: {currentMoveValid}");
            
            currentPathIsValid = currentMoveValid;
            lastPathCheckTime = Time.time;
        }
    }

    private void HandleMovement()
    {
        if (currentPathIsValid)
        {
            isChasing = true;
            ChasePlayer();
        }
        else
        {
            isChasing = false;
            if (usePatrol)
            {
                Patrol();
            }
        }
    }

    protected bool ValidateMovePath(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPosition);

        // 현재 위치에서의 바닥 체크
        bool hasCurrentFloor = Physics2D.OverlapCircle(transform.position, pathCheckRadius, floorLayer);
        // 목표 위치에서의 바닥 체크
        bool hasTargetFloor = Physics2D.OverlapCircle(targetPosition, pathCheckRadius, floorLayer);
        // 벽 체크 - 약간 여유를 두어 체크
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, direction, distance + 0.1f, wallLayer);

        LogDebug("PathCheck", 
            $"Target: {targetPosition}, " +
            $"Current Floor: {hasCurrentFloor}, " +
            $"Target Floor: {hasTargetFloor}, " +
            $"Wall Hit: {wallHit.collider != null}, " +
            $"Distance: {distance:F2}, " +
            $"Direction: {direction}");

        return hasCurrentFloor && hasTargetFloor && wallHit.collider == null;
    }

    protected virtual void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        Vector2 nextPosition = rb.position + direction * chaseSpeed * Time.fixedDeltaTime;

        LogDebug("Chase", 
            $"Current Pos: {rb.position}, " +
            $"Next Pos: {nextPosition}, " +
            $"Direction: {direction}, " +
            $"Speed: {chaseSpeed}");

        if (ValidateMovePath(nextPosition))
        {
            MoveToPosition(nextPosition, direction);
            TryAttack();
        }
        else
        {
            LogWarning("Chase", "Cannot move to next position - path invalid");
        }
    }

    protected virtual void Patrol()
    {
        float distanceToWaypoint = Vector2.Distance(transform.position, currentWaypoint);

        LogDebug("Patrol", 
            $"Current Waypoint: {currentWaypoint}, " +
            $"Distance to Waypoint: {distanceToWaypoint:F2}");

        if (distanceToWaypoint <= waypointStopDistance)
        {
            LogDebug("Patrol", "Reached waypoint, setting new patrol point");
            SetNewPatrolPoint();
            return;
        }

        Vector2 direction = (currentWaypoint - (Vector2)transform.position).normalized;
        float moveDistance = moveSpeed * Time.fixedDeltaTime;
        
        // 남은 거리가 이동 거리보다 작다면, 이동 거리를 조정
        moveDistance = Mathf.Min(moveDistance, distanceToWaypoint);
        
        Vector2 nextPosition = ((Vector2)transform.position) + direction * moveDistance;

        // 다음 위치가 현재 위치와 같지 않은지 확인
        if (Vector2.Distance(nextPosition, transform.position) > 0.01f)
        {
            LogDebug("Patrol", $"Moving towards waypoint - Next Position: {nextPosition}, Move Distance: {moveDistance:F3}");
            MoveToPosition(nextPosition, direction);
        }
        else
        {
            LogWarning("Patrol", "Movement too small, finding new patrol point");
            SetNewPatrolPoint();
        }
    }

    private void MoveToPosition(Vector2 position, Vector2 direction)
    {
        Vector2 oldPosition = transform.position;
        
        // 실제 이동 거리 계산
        float moveDistance = Vector2.Distance(oldPosition, position);
        
        // 최소 이동 거리 체크
        if (moveDistance < 0.01f)
        {
            LogWarning("Movement", "Movement distance too small, skipping");
            return;
        }

        // 이동 적용
        rb.position = position;
        transform.position = position;

        // 회전 적용
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0, 0, angle),
            rotationSpeed * Time.deltaTime
        );

        LogDebug("Movement", 
            $"Old Pos: {oldPosition}, " +
            $"New Pos: {position}, " +
            $"Movement Delta: {moveDistance:F3}, " +
            $"Actual Position: {transform.position}, " +
            $"Rigidbody Position: {rb.position}");
    }

    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0, 0, angle),
            rotationSpeed * Time.deltaTime
        );
    }

    protected virtual void TryAttack()
    {
        if (!canAttack || Time.time < lastAttackTime + attackCooldown) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        LogDebug("Attack", $"Distance to player: {distanceToPlayer:F2}, Attack Range: {attackRange}");

        if (distanceToPlayer <= attackRange)
        {
            PerformAttack();
        }
    }

    protected virtual void PerformAttack()
    {
        LogDebug("Attack", $"Performing attack at time: {Time.time:F2}");
        lastAttackTime = Time.time;
    }

    private void SetNewPatrolPoint()
    {
        const int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector2 newWaypoint = GenerateRandomWaypoint();
            LogDebug("Patrol", $"Attempt {attempts + 1}: Testing waypoint {newWaypoint}");

            if (ValidateMovePath(newWaypoint))
            {
                currentWaypoint = newWaypoint;
                LogDebug("Patrol", $"New valid waypoint set: {currentWaypoint}");
                return;
            }
            attempts++;
        }

        LogWarning("Patrol", "Failed to find valid waypoint, returning to start position");
        currentWaypoint = startPosition;
    }

    private Vector2 GenerateRandomWaypoint()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(0f, patrolRadius);
        Vector2 offset = new Vector2(
            Mathf.Cos(randomAngle),
            Mathf.Sin(randomAngle)
        ) * randomDistance;

        return startPosition + offset;
    }

    private void LogDebug(string category, string message) {
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[{gameObject.name}][{category}]</color> {message}");
        }
    }

    private void LogWarning(string category, string message)
    {
        Debug.LogWarning($"<color=yellow>[{gameObject.name}][{category}]</color> {message}");
    }

    private void LogError(string category, string message)
    {
        Debug.LogError($"<color=red>[{gameObject.name}][{category}]</color> {message}");
    }

    protected virtual void OnDrawGizmosSelected()
    {
        DrawRangeGizmos();
        DrawPatrolGizmos();
        DrawWaypointGizmos();
    }

    private void DrawRangeGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void DrawPatrolGizmos()
    {
        if (usePatrol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, patrolRadius);
        }
    }

    private void DrawWaypointGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentWaypoint, 0.3f);
            Gizmos.DrawLine(transform.position, currentWaypoint);
        }
    }
}
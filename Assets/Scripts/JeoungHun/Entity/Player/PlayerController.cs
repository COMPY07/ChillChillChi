
using UnityEngine;

public class PlayerController : BaseEntity {
    
    [Tooltip("달리기 속도 배수 (Shift 키를 누를 때)")]
    public float runSpeedMultiplier = 2f;
    
    [Header("Visual Feedback")]
    [Tooltip("캐릭터가 바라보는 방향을 나타내는 스프라이트")]
    public SpriteRenderer characterSprite;
    
    [Header("Collision Settings")]
    [Tooltip("벽 체크를 위한 레이캐스트 거리")]
    public float wallCheckDistance = 0.1f;
    
    private bool isFacingRight = true;
    private Vector2 moveDirection;
    private float currentSpeed;
    private LayerMask wallLayer; // Wall 레이어 마스크
    private Collider2D playerCollider; // 플레이어의 콜라이더

    private void Start()
    {
        if (characterSprite == null)
            characterSprite = GetComponent<SpriteRenderer>();
        
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
            Debug.LogError("Player needs a Collider2D component!");
            
        
        wallLayer = LayerMask.GetMask("Wall");
        currentSpeed = speed;
    }

    private void Update() {

        HandleInput();

        HandleMovement();

        UpdateVisuals();
    }

    private void HandleInput()
    {
        moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();
        

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            currentSpeed = speed * runSpeedMultiplier;
        else
            currentSpeed = speed;
        
    }

    private void HandleMovement()
    {
        Vector2 movement = moveDirection * currentSpeed * Time.deltaTime;
        
        // 이동하기 전에 벽 체크
        if (CanMove(movement))
        {
            transform.Translate(movement, Space.World);
        }
    }
    private bool CanMove(Vector2 movement)
    {
        if (movement.magnitude < 0.01f) return true; // 움직임이 없으면 true 반환
        
        // 플레이어의 현재 위치
        Vector2 currentPosition = transform.position;
        
        // 이동하려는 방향으로 레이캐스트 실행
        RaycastHit2D hit = Physics2D.Raycast(
            currentPosition,
            movement.normalized,
            movement.magnitude + wallCheckDistance,
            wallLayer
        );

        // 디버그용 레이캐스트 시각화
        Debug.DrawRay(
            currentPosition,
            movement.normalized * (movement.magnitude + wallCheckDistance),
            hit ? Color.red : Color.green,
            0.1f
        );

        if (hit)
        {
            // 벽과의 충돌이 감지되면
            float distanceToWall = hit.distance;
            
            // 벽까지의 거리가 매우 가까우면 이동 불가
            if (distanceToWall <= wallCheckDistance)
            {
                return false;
            }
            
            // 벽과 충돌하지 않는 거리만큼은 이동 가능
            if (movement.magnitude > distanceToWall - wallCheckDistance)
            {
                Vector2 safeMovement = movement.normalized * (distanceToWall - wallCheckDistance);
                transform.Translate(safeMovement, Space.World);
                return false;
            }
        }

        return true;
    }

    private void UpdateVisuals()
    {
        if (moveDirection.x != 0 && characterSprite != null) {
            if (moveDirection.x > 0 && !isFacingRight)
                FlipSprite();
            else if (moveDirection.x < 0 && isFacingRight)
                FlipSprite();
        }
    }

    private void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = characterSprite.transform.localScale;
        scale.x *= -1;
        characterSprite.transform.localScale = scale;
    }

    public Vector2 MovementDirection
    {
        get { return moveDirection; }
    }

    public float CurrentSpeed
    {
        get { return currentSpeed; }
    }

    public void SetPosition(Vector2 newPosition)
    {
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }
}
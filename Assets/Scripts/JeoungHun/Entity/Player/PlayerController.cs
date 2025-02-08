
using UnityEngine;

public class PlayerController : BaseEntity {
    
    [Tooltip("달리기 속도 배수 (Shift 키를 누를 때)")]
    public float runSpeedMultiplier = 2f;
    
    [Header("Visual Feedback")]
    [Tooltip("캐릭터가 바라보는 방향을 나타내는 스프라이트")]
    public SpriteRenderer characterSprite;
    
    private bool isFacingRight = true;
    private Vector2 moveDirection;
    private float currentSpeed;

    private void Start()
    {
        
        if (characterSprite == null)
            characterSprite = GetComponent<SpriteRenderer>();
        speed *= 5;
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

        transform.Translate(movement, Space.World);
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
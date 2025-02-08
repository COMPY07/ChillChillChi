using UnityEngine;

public class ChasePlayerCamera : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform target;

    [Header("카메라 기본 설정")]
    [Range(0.1f, 2f)]
    public float smoothTime = 0.15f;
    
    [Header("동적 카메라 설정")]
    [Tooltip("방향 전환 감지를 위한 최소 속도")]
    [Range(0.001f, 3f)]
    public float minSpeedThreshold = 0.005f;
    
    [Tooltip("방향 전환 감지를 위한 각도")]
    [Range(0f, 180f)]
    public float directionChangeAngle = 60f;
    
    [Range(0.1f, 2f)]
    public float turnEffect = 0.8f;
    
    [Range(0f, 1f)]
    public float shakeAmount = 0.1f;

    [Header("앞서보기 설정")]
    [Range(0f, 5f)]
    public float lookAheadDistance = 2f;
    [Range(0.1f, 5f)]
    public float lookAheadSpeed = 2f;

    [Header("디버그")]
    public bool debug = false;
    

    private Camera cam;
    private Vector3 previousPosition;
    private Vector3 moveDirection;
    private Vector3 previousDirection;
    private Vector3 velocity;
    private Vector3 lookAheadPos;
    private Vector3 currentShake;
    private float currentRotation;
    private float targetRotation;
    private float directionChangeTimer;
    private float movementAccumulator;
    private Vector3 lastShakeDirection; 

    private void Start()
    {
        cam = GetComponent<Camera>();
        previousPosition = target ? target.position : Vector3.zero;
        transform.position = new Vector3(previousPosition.x, previousPosition.y, -10f);
        lastShakeDirection = Vector3.zero;
    }

    private void Update()
    {
        if (!target) return;

        Vector3 currentPosition = target.position;
        Vector3 movement = currentPosition - previousPosition;
        float movementMagnitude = movement.magnitude;
        
        movementAccumulator += movementMagnitude;

        if (movementMagnitude > minSpeedThreshold)
        {
            moveDirection = movement.normalized;
            
            if (previousDirection != Vector3.zero && movementAccumulator > 0.05f)
            {
                float angle = Vector3.Angle(moveDirection, previousDirection);
                
                if (angle > directionChangeAngle && directionChangeTimer <= 0)
                {
                    float turnDirection = Mathf.Sign(Vector3.Cross(previousDirection, moveDirection).z);
                    AddDirectionalCameraEffect(turnDirection, moveDirection);
                    directionChangeTimer = 0.3f;
                    movementAccumulator = 0;
                }
            }
            
            previousDirection = moveDirection;
        }

        if (directionChangeTimer > 0)
            directionChangeTimer -= Time.deltaTime;
        

        if (currentShake.magnitude > 0.01f)
        {
            if (moveDirection != Vector3.zero)
            {
                Vector3 targetShakeDir = Vector3.Lerp(
                    lastShakeDirection,
                    moveDirection * currentShake.magnitude,
                    Time.deltaTime * 5f
                );
                currentShake = Vector3.Lerp(currentShake, targetShakeDir, Time.deltaTime * 5f);
                lastShakeDirection = currentShake.normalized;
            }
            else
            {
                currentShake = Vector3.Lerp(currentShake, Vector3.zero, Time.deltaTime * 5f);
            }
        }

        UpdateLookAheadPosition();
        UpdateCameraTransform();
        previousPosition = currentPosition;

        if (debug)
            Debug.Log($"Movement: {movement.magnitude:F4}, Direction: {moveDirection}, Shake: {currentShake}");
        
    }

    private void UpdateLookAheadPosition()
    {
        Vector3 targetLookAheadPos = moveDirection * lookAheadDistance;
        lookAheadPos = Vector3.Lerp(lookAheadPos, targetLookAheadPos, Time.deltaTime * lookAheadSpeed);
    }

    private void UpdateCameraTransform()
    {
        Vector3 targetPos = target.position + lookAheadPos + currentShake;
        targetPos.z = -10f;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * 5f);
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        targetRotation = Mathf.LerpAngle(targetRotation, 0, Time.deltaTime * 3f);
    }

    private void AddDirectionalCameraEffect(float direction, Vector3 moveDir)
    {
        targetRotation += direction * 5f * turnEffect;

        Vector3 shakeVector = new Vector3(
            moveDir.x * Random.Range(-0.5f, 0.5f),
            moveDir.y * Random.Range(-0.5f, 0.5f),
            0
        ) * shakeAmount;

        currentShake = shakeVector;
        lastShakeDirection = shakeVector.normalized;
    }

    private void OnDrawGizmos() {
        if (!debug || !target) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(target.position, target.position + moveDirection);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(target.position, target.position + currentShake * 5f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position + lookAheadPos, 0.5f);
    }
}
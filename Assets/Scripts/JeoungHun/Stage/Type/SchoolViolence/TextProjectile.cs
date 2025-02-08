using UnityEngine;

public class TextProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float moveSpeed;
    
    public void Initialize(Vector3 direction, float speed) {
        moveDirection = direction;
        moveSpeed = speed;
    }
    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 7f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌 처리
        if (collision.CompareTag("Player"))
        {
            // 플레이어 데미지 처리
            collision.GetComponent<BaseEntity>()?.Damaged(10);
            Destroy(gameObject);
        }
    }
}
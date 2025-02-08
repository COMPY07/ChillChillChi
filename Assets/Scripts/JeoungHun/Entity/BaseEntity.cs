using UnityEngine;



/// <summary>
/// hp, speed, name, description,
/// method: destroy, damaged, 머 더 없겟지
///
/// 
/// </summary>
public class BaseEntity : MonoBehaviour
{
    public string entityName;
    public string description;

    protected float hp;
    [Header("Movement Settings")]
    [Range(1, 100)] [SerializeField] protected int speed;

    
    public bool Damaged(float damage)
    {
        if (hp - damage <= 0) return false;
        hp -= damage;
        return true;
    }

    public float GetHp()
    {
        return hp;
    }

    public int GetSpeed()
    {
        return speed;
    }

    public void Destroy(){
        Destroy(this);
    }


}

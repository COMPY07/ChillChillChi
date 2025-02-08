using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [SerializeField] private float Speed = 5;
    [SerializeField] private Vector2 MoveDir;
    void Update()
    {
        MoveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        MoveDir *= Speed;
        gameObject.transform.position = new Vector2(MoveDir.x * Time.deltaTime, MoveDir.y * Time.deltaTime);
    }
}

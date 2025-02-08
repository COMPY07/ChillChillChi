using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private float Speed = 5;
    [SerializeField] private Vector2 MoveDir;
    [SerializeField] private Image Fade;
    
    private Rigidbody2D rb;

    [SerializeField] private int Scene_Num;
    private bool check;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        check = false;
    }
    void Update()
    {
        MoveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        MoveDir *= Speed;

        // Vector2를 Vector3로 변환하여 이동 처리
        rb.linearVelocity= MoveDir;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!check)
        {
            switch (other.tag)
            {
                // 3 -> redDDAK
                // 2-> school
                // 1 -> loop
                // 0 -> family
                case "1":
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Fade.DOFade(1.0f, 0.9f);
                        Scene_Num = Convert.ToInt32(other.tag) - 1;
                        Invoke("Scene_Change", 0.9f);
                        switch (other.tag)
        {
            // 3 -> redDDAK
            // 2-> school
            // 1 -> loop
            // 0 -> family
            case "1":
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Fade.DOFade(1.0f, 0.9f);
                    Scene_Num = Convert.ToInt32(other.tag) - 1;
                    Invoke("Scene_Change", 0.9f);
                    check = true;
                }

                break;
            case "2":
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Fade.DOFade(1.0f, 0.9f);
                    Scene_Num = Convert.ToInt32(other.tag) - 1;
                    Invoke("Scene_Change", 0.9f);
                    check = true;
                }

                break;
            case "3":
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Fade.DOFade(1.0f, 0.9f);
                    Scene_Num = Convert.ToInt32(other.tag) - 1;
                    Invoke("Scene_Change", 0.9f);
                    check = true;
                }

                break;
            case "4":
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Fade.DOFade(1.0f, 0.9f);
                    Scene_Num = Convert.ToInt32(other.tag) - 1;
                    Invoke("Scene_Change", 0.9f);
                    check = true;
                }

                break;
                
        }
                    }

                    break;
                case "2":
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Fade.DOFade(1.0f, 0.9f);
                        Scene_Num = Convert.ToInt32(other.tag) - 1;
                        Invoke("Scene_Change", 0.9f);
                    }

                    break;
                case "3":
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Fade.DOFade(1.0f, 0.9f);
                        Scene_Num = Convert.ToInt32(other.tag) - 1;
                        Invoke("Scene_Change", 0.9f);
                    }

                    break;
                case "4":
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Fade.DOFade(1.0f, 0.9f);
                        Scene_Num = Convert.ToInt32(other.tag) - 1;
                        Invoke("Scene_Change", 0.9f);
                    }

                    break;
                
            }
        }
        
    }

    void Scene_Change()
    {
        StageManager.Instance.LoadStage(Scene_Num);
    }
}
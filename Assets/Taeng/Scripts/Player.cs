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
    [SerializeField] private GameObject[] Inter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        for (int i = 0; i < 4; i++)
        {
            Inter[i].gameObject.GetComponent<SpriteRenderer>().DOFade(0, 0f);
        }
        
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
                    other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                    other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(1, 0.3f);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Fade.DOFade(1.0f, 0.9f);
                    Scene_Num = Convert.ToInt32(other.tag) - 1;
                    Invoke("Scene_Change", 0.9f);
                    check = true;
                }

                break;
            case "2":
                    other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(1, 0.3f);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Fade.DOFade(1.0f, 0.9f);
                    Scene_Num = Convert.ToInt32(other.tag) - 1;
                    Invoke("Scene_Change", 0.9f);
                    check = true;
                }

                break;
            case "3":
                    other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                    other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(1, 0.3f);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Fade.DOFade(1.0f, 0.9f);
                    Scene_Num = Convert.ToInt32(other.tag) - 1;
                    Invoke("Scene_Change", 0.9f);
                    check = true;
                }

                break;
            case "4":
                    other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                    other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(1, 0.3f);
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
        
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (!check)
        {
            switch (other.tag)
            {
            case "1":
                    other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(0, 0.3f);

                break;
            case "2":
                other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(0, 0.3f);

                break;
            case "3":
                    other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(0, 0.3f);

                break;
            case "4":
                    other.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(0, 0.3f);

                break;
                
        }
        }
        
    }

    void Scene_Change()
    {
        check = false;
        StageManager.Instance.LoadStage(Scene_Num);
        
    }
}
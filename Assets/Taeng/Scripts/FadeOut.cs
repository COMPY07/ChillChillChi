using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText; // 대사 텍스트
    [SerializeField] private float fadeDuration = 1f; // 페이드 인/아웃 시간
    [SerializeField] private float displayDuration = 3f; // 대사 유지 시간
    [SerializeField] private float typingSpeed = 0.05f; // 타이핑 속도

    private Dialogue[] dialogues; // 대사 목록
    private int currentDialogueIndex = 0; // 현재 대사의 인덱스
    [SerializeField] GameObject fadeImage;

    void Start()
    {
        if(RoomManager._instance.start == 0)
        {
            LoadDialogue(); // 대사 로드
            ShowDialogue(); // 첫 번째 대사 출력
        }
        else
        {
            fadeImage.GetComponent<Image>().DOFade(0f, 0.9f);
        }
        
    }

    private void OnEnable()
    {
        Invoke("Fade_Enable", 0.5f);
        
        
    }

    void Fade_Enable()
    {
        if (RoomManager._instance.start == 1)
        {
            fadeImage.GetComponent<Image>().DOFade(0f, 0.9f);
        }
    }

    private void OnDisable()
    {
        fadeImage.GetComponent<Image>().DOFade(1f, 0.9f);
    }

    // JSON 파일에서 대사 로드
    void LoadDialogue()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "dial.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            DialogueData dialogueData = JsonConvert.DeserializeObject<DialogueData>(json);
            dialogues = dialogueData.dialogues;
        }
        else
        {
            Debug.LogError("Dialogue file not found!");
        }
    }

    // 대사 출력 함수
    void ShowDialogue()
    {
        if (currentDialogueIndex < dialogues.Length)
        {
            Dialogue currentDialogue = dialogues[currentDialogueIndex];
            dialogueText.text = ""; // 초기화
            dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 0); // 투명도 0
            
            // 페이드 인 후 타이핑 효과 실행
            dialogueText.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                StartCoroutine(TypeText(currentDialogue.text));
            });
        }
        else
        {
            Debug.Log("Dialogue ended.");
        }
    }

    // 한 글자씩 출력하는 코루틴
    IEnumerator TypeText(string text)
    {
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        Invoke("StartFadeOut", displayDuration);
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartFadeOut();
        }
    }

    void StartFadeOut()
    {
        dialogueText.DOFade(0f, fadeDuration).OnComplete(NextDialogue);
    }

    void NextDialogue()
    {
        currentDialogueIndex++;
        if (currentDialogueIndex < dialogues.Length)
        {
            ShowDialogue(); // 다음 대사 출력
        }
        else
        {
            fadeImage.GetComponent<Image>().DOFade(0f, 0.9f);
            RoomManager._instance.start = 1;
        }
    }
}

// 대사 데이터 클래스
[System.Serializable]
public class Dialogue
{
    public int id;
    public string character;
    public string text;
}

// JSON 데이터를 감싸는 클래스
[System.Serializable]
public class DialogueData
{
    public Dialogue[] dialogues;
}

using Manager;
using UnityEngine;

public class EndCredits : MonoBehaviour
{
    public RectTransform contentTransform; // ContentContainer의 RectTransform
    public float duration;         // 30초 동안 이동
    private Vector2 startPos;
    private Vector2 endPos;
    private float elapsedTime = 0f;



    [SerializeField] private AudioClip clip;
    void Start()
    {
        // 초기 시작 위치와 끝 위치 설정
        startPos = contentTransform.anchoredPosition;
        endPos = new Vector2(startPos.x, 0f); // posY를 0으로 설정

        Manager.SoundManager.Instance.NextSoundAdd(0, new SoundClip(clip));
        Manager.SoundManager.Instance.NextSoundForce(0);
    }

    void Update()
    {
        if (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration; // 0에서 1까지의 시간 비율
            contentTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t); // 선형 보간
        }
        
        if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Return))
        {
            // LoadingController.LoadScene("StartScene");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class RedDDAK : BaseStage
{
    public GameObject objectPrefab; // UI 오브젝트 프리팹 (예: Image)
    public RectTransform canvasTransform; // 캔버스의 RectTransform
    public Slider timeSlider; // 슬라이더 UI
    public TextMeshProUGUI clickCountText; // 클릭된 개수를 표시할 Text UI
    public Button startButton; // 시작 버튼 UI
    public float spawnInterval = 2f; // 생성 간격
    public int requiredClicks = 30; // 필요한 클릭 수
    private int clickedCount = 0; // 클릭된 개수 추적
    private bool gameActive = false; // 게임 활성화 여부
    

    [SerializeField] private Image Fade;
    [SerializeField] private Image Fade_false;


    public void Awake()
    {
        
    }
    void Start()
    {
        Begin();
    }

    public override void Begin()
    {
        startButton.onClick.AddListener(StartGame); // 시작 버튼에 리스너 추가
        clickCountText.text = "클릭 횟수: 0"; // 초기 클릭 횟수
        timeSlider.gameObject.SetActive(false); // 게임 시작 전에는 슬라이더를 숨김
    }

    public override void End()
    {
        GameObject newObj = Instantiate(objectPrefab, canvasTransform);
        RectTransform objRect = newObj.GetComponent<RectTransform>();

        // 랜덤 위치 설정 (캔버스 내부)
        float x = Random.Range(-canvasTransform.rect.width / 2, canvasTransform.rect.width / 2);
        float y = Random.Range(-canvasTransform.rect.height / 2, canvasTransform.rect.height / 2);

        objRect.anchoredPosition = new Vector2(x, y);

        // 클릭하면 삭제하고 카운트 증가
        newObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            clickedCount++;
            clickCountText.text = "클릭 횟수: " + clickedCount; // 클릭 횟수 업데이트
            Destroy(newObj);
        });
        StartCoroutine(DestroyObjectAfterTime(newObj, 1f));
        base.End();
    }

    void StartGame()
    {
        // 게임 시작 시, 시작 버튼 숨기기
        startButton.gameObject.SetActive(false);

        gameActive = true;
        clickedCount = 0; // 클릭 횟수 초기화
        clickCountText.text = "클릭 횟수: 0"; // 클릭 횟수 UI 초기화
        timeSlider.gameObject.SetActive(true); // 슬라이더 보이기
        InvokeRepeating("SpawnObject", 1f, spawnInterval); // 오브젝트 생성 시작
        StartCoroutine(Timer()); // 타이머 시작
    }

    void SpawnObject()
    {
        if (!gameActive) return;

        
        // 1초 뒤에 오브젝트를 자동으로 삭제하는 코루틴 실행
        
    }

    // 오브젝트를 일정 시간 후에 삭제하는 코루틴
    IEnumerator DestroyObjectAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj); // 1초 뒤에 오브젝트 삭제
    }

    IEnumerator Timer()
    {
        float timeRemaining = 30f; // 30초 제한 시간

        while (timeRemaining > 0 && gameActive)
        {
            timeRemaining -= Time.deltaTime;
            timeSlider.value = timeRemaining / 30f; // 슬라이더 값 업데이트
            yield return null;
        }

        if (clickedCount >= requiredClicks)
        {
            RoomManager._instance.OnColor_Plus(1);
            Debug.Log("게임 클리어!");
            clickCountText.text = "게임 클리어! 클릭 횟수: " + clickedCount;
            Fade.gameObject.SetActive(true);
            Fade.GetComponent<Image>().DOFade(1.0f, 0.9f);
            Invoke("BbalTtack", 1.0f);
        }
        else
        {
            // 게임 실패
            Debug.Log("게임 실패! 클릭 횟수를 완료하지 못했습니다.");
            clickCountText.text = "게임 실패! 클릭 횟수: " + clickedCount;
            Fade_false.gameObject.SetActive(true);
            Fade_false.GetComponent<Image>().DOFade(1.0f, 0.9f);
        }

        gameActive = false; // 게임 종료
    }
    
}

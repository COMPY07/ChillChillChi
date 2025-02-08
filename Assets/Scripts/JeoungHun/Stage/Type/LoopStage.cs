using UnityEngine;

public class LoopStage: BaseStage {
        
    [SerializeField] public SpawnSetting playerSpawnSetting;

    private Vector3 originPosition;


    private bool check;


    [SerializeField] private GameObject question;
    private GameObject currentQuestion;
    public void Awake() {
        base.Awake();
        check = false;
    }

    public void Start() {
        Generate();
        Begin();
    }

    public  void Generate() {
        
        base.Generate();
        
    }


    public override void Begin() {
        GameObject player = StageManager.Instance.getPlayerPrefabs();

        if (player == null) {
            Debug.LogError("The player object is null in the StageManager!");
            return;
        }

        if (!bsp.isDone()) {
            Invoke("Begin", 1f);
            return;
        }
        
        GameObject playerObject = bsp.SpawnPlayer(player, playerSpawnSetting);
        StageManager.Instance.SetPlayerObject(playerObject);

        originPosition = playerObject.transform.GetChild(1).transform.position;
        base.Begin();
    }

    public override void End() {
        if (!check) {
            check = !check;
            teleportPlayer();
            return;
        }
        // 여기 머지 이후 이동
    }

    private void teleportPlayer()
    {
        StageManager.Instance.GetPlayerObject().transform.GetChild(1).transform.position = originPosition;
        showQuestion();
        
    }


    private void showQuestion() {
        currentQuestion = Instantiate(question);
        currentQuestion.transform.SetParent(StageManager.Instance.GetPlayerObject().transform);
        Invoke("hideQuestion", 1f);
    }
    private void hideQuestion() {
        Destroy(currentQuestion);
    }
    
}
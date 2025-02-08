using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : Singleton<StageManager>
{

    [Header("Player")] [SerializeField] private GameObject playerObject;
    [Header("StageInfo")] [SerializeField] private BaseStage current;
    [SerializeField] private GameObject currentPlayer;
    [SerializeField] private GameObject[] stages;

    [SerializeField] private GameObject Room;
    
    public GameObject getPlayerPrefabs()
    {
        return playerObject;
    }


    public void Awake()
    {
        if (Room == null)
        {
            Debug.LogError("the Room object is null!!");
        }
        
        Room.SetActive(true);
        base.Awake();
    }

    public bool LoadStage(string name)
    {
        foreach (GameObject obj in stages)
        {
            BaseStage stage = obj.GetComponent<BaseStage>();
            if (!stage.stageName.Equals(name)) continue;
            Load(obj);
            return true;
        }

        return false;
    }

    public bool LoadStage(int idx)
    {

        if (idx < 0 || idx >= stages.Length) return false;


        Load(stages[idx]);

        return true;
    }


    private void Load(GameObject stageObject)
    {
        if (stageObject == null) return;
        if (current != null) Destroy(current.gameObject); // 생각보다 많이 헤비한 연산임.. 여태까지 생성된 놈들 다 삭제여서..


        BaseStage stage = Instantiate(stageObject).gameObject.GetComponent<BaseStage>();
        if (stage == null) return;
        current = stage;
    }

    public void EndStage() {
        if (current != null) Destroy(current.gameObject);
        Room.SetActive(true);    
    }

    
[CanBeNull]
    public BaseStage getCurrentStage() {
        return current;
    }

    public void SetPlayerObject(GameObject player)
    {
        this.currentPlayer = player;
    }

    public GameObject GetPlayerObject()
    {
        return currentPlayer;
    }


}
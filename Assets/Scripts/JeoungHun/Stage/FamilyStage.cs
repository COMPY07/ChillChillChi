using System;
using UnityEngine;

public class FamilyStage  : BaseStage
{


    [SerializeField] public SpawnSetting playerSpawnSetting;
    
    public void Awake() {
        base.Awake();
    }

    public void Start() {
        Debug.Log("Generate");
        Generate();
        Begin();
    }

    public  void Generate() {
        
        base.Generate();
        Debug.Log("Generate Complete");
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
        base.Begin();
    }

    public override void End() {
        
    }

}
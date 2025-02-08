using UnityEngine;

public class FamilyStage  : BaseStage
{


    [SerializeField] public SpawnSetting playerSpawnSetting;
    
    public void Awake() {
        base.Awake();
    }


    public void Generate() {
        base.Generate();
    }


    public override void Begin() {
        GameObject player = StageManager.Instance.getPlayerObject();

        if (player == null) {
            Debug.LogError("The player object is null in the StageManager!");
            return;
        }

        if (!bsp.isDone()) {
            Invoke("Begin", 1f);
            return;
        }
        
        bsp.SpawnPlayer(player, playerSpawnSetting);
        base.Begin();
    }

    public override void End() {
        
    }

}
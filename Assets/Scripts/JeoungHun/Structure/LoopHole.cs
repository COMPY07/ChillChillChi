using UnityEngine;

public class LoopHole : MonoBehaviour
{


    private Transform player;
    private bool check;
    
    public void Start() {
        check = false;
        player = StageManager.Instance.GetPlayerObject()?.transform.GetChild(1).transform;
    }
    
    public void Update() {


        if (player == null) 
            player = StageManager.Instance.GetPlayerObject()?.transform.GetChild(1).transform;
        if (Vector3.Distance(player.position, this.transform.position) >= 2) {
            check = false;
            return;
        }

        if (check) return;
        check = true;
        
        
        StageManager.Instance.getCurrentStage()?.End();
    }
    
    

}
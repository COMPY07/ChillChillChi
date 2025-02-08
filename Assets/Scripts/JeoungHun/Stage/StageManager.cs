using UnityEngine;

public class StageManager : Singleton<StageManager>
{

    [SerializeField] private GameObject playerObject;


    public GameObject getPlayerObject()
    {
        return playerObject;
    }
    


}
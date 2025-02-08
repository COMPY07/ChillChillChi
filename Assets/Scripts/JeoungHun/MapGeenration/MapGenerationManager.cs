using Unity.VisualScripting;
using UnityEngine;

public class MapGenerationManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private GeneratorSetting setting;

    [Header("Generation Type")] [SerializeField]
    private GenerationType type = GenerationType.BSP; 
    
    [Header("SpawnSettings")]
    [SerializeField] private GameObject player;
    
    private BSPGenerator bsp;
    private GameObject currentPlayer;
    private void Awake() {
        if (setting == null)
            Debug.LogError("Setting is null");
        else {
            if (type == GenerationType.BSP)
            {
                bsp = this.AddComponent<BSPGenerator>();

                bsp.GeneratorInit(setting);
            }
        }
    }

    private void Start() {
        if (type == GenerationType.BSP) {
            bsp.GenerateMap();
        }
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            SpawnSetting setting = new SpawnSetting();
            setting.spawnName = "Player";
            currentPlayer = bsp.SpawnPlayer(player, setting);
        }
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     
        //     bsp.CreatePlayerInRoom(bsp.GetRoomFromPosition(currentPlayer.transform.position), currentPlayer, RoomPosition.Corner);
        //     
        // }
        
    }
}

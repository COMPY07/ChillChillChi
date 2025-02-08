using UnityEngine;



[CreateAssetMenu(fileName = "StageSpawnData", menuName = "Game/Stage Spawn Data")]
public class StageSpawnData : ScriptableObject
{
    [Header("Monster Spawn Info")]
    public SpawnPair[] spawnPairs;
}
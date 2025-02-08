[System.Serializable]
public class SpawnSetting {
    public string spawnName;
    public RoomPosition preferredRoomPosition = RoomPosition.Center;
    public MapPosition preferredMapPosition = MapPosition.Random;
    public float minDistanceFromWalls = 1f;
    public float minDistanceFromOtherSpawns = 5f;
    public float minDistanceFromPlayer = 10f;  // 몬스터용
    public bool requireLineOfSight = false;    // 시야 확보 필요 여부
    public int maxAttempts = 100;             // 최대 시도 횟수
}
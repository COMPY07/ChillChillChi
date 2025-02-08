using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class BSPGenerator : MonoBehaviour{
    private enum WallType
    {
        Single,
        Top,
        Bottom,
        Left,
        Right,
        CornerOuterTopLeft,
        CornerOuterTopRight,
        CornerOuterBottomLeft,
        CornerOuterBottomRight,
        CornerInnerTopLeft,
        CornerInnerTopRight,
        CornerInnerBottomLeft,
        CornerInnerBottomRight,
        EndTop,
        EndBottom,
        EndLeft,
        EndRight,
        Default
    }
    private int mapWidth = 100;
    private int mapHeight = 100;
    private int minRoomSize = 10;
    private int maxIterations = 4; 
    
    private string wallLayerName = "Wall";
    
    private GameObject wallPrefab; 
    private GameObject floorPrefab; 

    private List<BSPRoom> rooms = new List<BSPRoom>();
    private int[,] map;      // 0: 빈 공간, 1: 벽
    private int wallLayerId;

    private GeneratorSetting setting;
    private StructurePlacer placer;
    private MapSpawnManager spawnManager;
    
    
    private const bool debug = false;

    private bool generateDone;
    
    public void GeneratorInit(GeneratorSetting setting)
    {
        generateDone = false;
        InitWithSettings(setting);
        
        wallLayerId = LayerMask.NameToLayer(wallLayerName);
        
        if (wallLayerId == -1) {
            Debug.LogError($"Layer '{wallLayerName}' not found! 이거 추가좀 이거 왜 안고쳐짐?");
            wallLayerId = 0;
        }

        this.setting = setting;
    }

    private void InitWithSettings(GeneratorSetting setting) {
        mapWidth = setting.mapWidth;
        mapHeight = setting.mapHeight;
        minRoomSize = setting.minRoomSize;
        maxIterations = setting.maxIterations;
        wallLayerName = setting.wallLayerName;
        floorPrefab = setting.floorPrefab;
    }
    
    public void GenerateMap() {
        map = new int[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x, y] = 1;
            }
        }

        SplitSpace(0, 0, mapWidth, mapHeight, 0);
        
        ConnectRooms();
        
        InstantiateTiles();

        PlaceStructure();

        PlaceSpawnPoints();
        generateDone = true;
    }

    private void PlaceStructure() {
        this.placer = new StructurePlacer(map, setting);
        placer.PlaceStructures(transform);
    }
    private void PlaceSpawnPoints() {
        spawnManager = new MapSpawnManager(map, rooms);
    }
    

    private void SplitSpace(int x, int y, int width, int height, int iteration)
    {
        if (iteration >= maxIterations || width < minRoomSize * 2 || height < minRoomSize * 2) {
            int roomWidth = Random.Range(minRoomSize, width - 4);
            int roomHeight = Random.Range(minRoomSize, height - 4);
            int roomX = x + Random.Range(2, width - roomWidth - 2);
            int roomY = y + Random.Range(2, height - roomHeight - 2);

            CreateRoom(roomX, roomY, roomWidth, roomHeight);
            rooms.Add(new BSPRoom(roomX, roomY, roomWidth, roomHeight));
            return;
        }

        bool splitHorizontal = Random.Range(0f, 1f) > 0.5f;
        if (width > height && width / height >= 1.25f) splitHorizontal = false;
        else if (height > width && height / width >= 1.25f) splitHorizontal = true;

        if (splitHorizontal) {
            int splitAt = Random.Range(minRoomSize, height - minRoomSize);
            SplitSpace(x, y, width, splitAt, iteration + 1);
            SplitSpace(x, y + splitAt, width, height - splitAt, iteration + 1);
        }
        else {
            int splitAt = Random.Range(minRoomSize, width - minRoomSize);
            SplitSpace(x, y, splitAt, height, iteration + 1);
            SplitSpace(x + splitAt, y, width - splitAt, height, iteration + 1);
        }
    }

    private void CreateRoom(int x, int y, int width, int height)
    {
        for (int i = x; i < x + width; i++) {
            for (int j = y; j < y + height; j++)
                if (i >= 0 && i < mapWidth && j >= 0 && j < mapHeight) map[i, j] = 0;
        }
    }

    private void ConnectRooms() {
        for (int i = 0; i < rooms.Count - 1; i++) {
            BSPRoom roomA = rooms[i];
            BSPRoom roomB = rooms[i + 1];

            Vector2Int pointA = new Vector2Int(
                roomA.x + roomA.width / 2,
                roomA.y + roomA.height / 2
            );
            Vector2Int pointB = new Vector2Int(
                roomB.x + roomB.width / 2,
                roomB.y + roomB.height / 2
            );
            
            CreateCorridor(pointA.x, pointA.y, pointB.x, pointA.y);
            CreateCorridor(pointB.x, pointA.y, pointB.x, pointB.y);
        }
    }

    private void CreateCorridor(int startX, int startY, int endX, int endY) {
        int x = startX;
        int y = startY;

        while (x != endX || y != endY)
        {
            if (x < endX) x++;
            else if (x > endX) x--;
            
            if (y < endY) y++;
            else if (y > endY) y--;

            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight) {
                map[x, y] = 0;
                
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int nx = x + i;
                        int ny = y + j;
                        if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                            map[nx, ny] = 0;
                    }
                }
            }
        }
    }

    private void InstantiateTiles()
    {
        GameObject wallParent = new GameObject("Walls");
        GameObject floorParent = new GameObject("Floors");
        wallParent.transform.parent = transform;
        floorParent.transform.parent = transform;

        wallParent.layer = LayerMask.NameToLayer("Wall");
        floorParent.layer = LayerMask.NameToLayer("Floor");

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                if (map[x, y] == 1)
                {
                    GameObject wall = Instantiate(setting.tilePrefab, position, Quaternion.identity, wallParent.transform);
                    wall.layer = LayerMask.NameToLayer("Wall");

                    SpriteRenderer wallRenderer = wall.GetComponent<SpriteRenderer>();
                    if (wallRenderer != null)
                    {
                        WallType wallType = DetermineWallType(x, y);
                        ApplyWallSprite(wallRenderer, wallType);
                        
                        wallRenderer.sortingLayerName = setting.wallTileSettings.sortingLayerName;
                        wallRenderer.sortingOrder = setting.wallTileSettings.sortingOrder;
                    }

                    Collider2D collider = wall.GetComponent<Collider2D>();
                    if (collider != null)
                        collider.isTrigger = false;
                }
                else
                {
                    GameObject floor = Instantiate(setting.floorPrefab, position, Quaternion.identity, floorParent.transform);
                    floor.layer = LayerMask.NameToLayer("Floor");

                    SpriteRenderer floorRenderer = floor.GetComponent<SpriteRenderer>();
                    if (floorRenderer != null)
                    {
                        floorRenderer.sortingLayerName = "Floor";
                        floorRenderer.sortingOrder = 0;
                    }
                }
            }
        }
    }

    private WallType DetermineWallType(int x, int y)
    {
        bool up = IsWall(x, y + 1);
        bool down = IsWall(x, y - 1);
        bool left = IsWall(x - 1, y);
        bool right = IsWall(x + 1, y);
        
        // 대각선 방향도 확인
        bool upLeft = IsWall(x - 1, y + 1);
        bool upRight = IsWall(x + 1, y + 1);
        bool downLeft = IsWall(x - 1, y - 1);
        bool downRight = IsWall(x + 1, y - 1);

        // 단독 벽
        if (!up && !down && !left && !right)
            return WallType.Single;

        // 내부 모서리 검사 (벽이 안쪽으로 꺾이는 경우)
        if (up && right && !upRight) return WallType.CornerInnerTopRight;
        if (up && left && !upLeft) return WallType.CornerInnerTopLeft;
        if (down && right && !downRight) return WallType.CornerInnerBottomRight;
        if (down && left && !downLeft) return WallType.CornerInnerBottomLeft;

        // 외부 모서리 검사 (벽이 바깥쪽으로 꺾이는 경우)
        if (!up && !left && right && down) return WallType.CornerOuterTopLeft;
        if (!up && !right && left && down) return WallType.CornerOuterTopRight;
        if (!down && !left && right && up) return WallType.CornerOuterBottomLeft;
        if (!down && !right && left && up) return WallType.CornerOuterBottomRight;

        // 끝부분 검사
        if (up && !down && !left && !right) return WallType.EndBottom;
        if (!up && down && !left && !right) return WallType.EndTop;
        if (!up && !down && left && !right) return WallType.EndRight;
        if (!up && !down && !left && right) return WallType.EndLeft;

        // 테두리 검사
        if (!up && down) return WallType.Top;
        if (up && !down) return WallType.Bottom;
        if (!left && right) return WallType.Left;
        if (left && !right) return WallType.Right;

        return WallType.Default;
    }

    private void ApplyWallSprite(SpriteRenderer renderer, WallType type)
    {
        var sprites = setting.wallTileSettings.sprites;
        
        switch (type)
        {
            case WallType.Single:
                renderer.sprite = sprites.singleWall;
                break;
            case WallType.Top:
                renderer.sprite = sprites.wallTop;
                break;
            case WallType.Bottom:
                renderer.sprite = sprites.wallBottom;
                break;
            case WallType.Left:
                renderer.sprite = sprites.wallLeft;
                break;
            case WallType.Right:
                renderer.sprite = sprites.wallRight;
                break;
            case WallType.CornerOuterTopLeft:
                renderer.sprite = sprites.cornerOuterTopLeft;
                break;
            case WallType.CornerOuterTopRight:
                renderer.sprite = sprites.cornerOuterTopRight;
                break;
            case WallType.CornerOuterBottomLeft:
                renderer.sprite = sprites.cornerOuterBottomLeft;
                break;
            case WallType.CornerOuterBottomRight:
                renderer.sprite = sprites.cornerOuterBottomRight;
                break;
            case WallType.CornerInnerTopLeft:
                renderer.sprite = sprites.cornerInnerTopLeft;
                break;
            case WallType.CornerInnerTopRight:
                renderer.sprite = sprites.cornerInnerTopRight;
                break;
            case WallType.CornerInnerBottomLeft:
                renderer.sprite = sprites.cornerInnerBottomLeft;
                break;
            case WallType.CornerInnerBottomRight:
                renderer.sprite = sprites.cornerInnerBottomRight;
                break;
            case WallType.EndTop:
                renderer.sprite = sprites.endTop;
                break;
            case WallType.EndBottom:
                renderer.sprite = sprites.endBottom;
                break;
            case WallType.EndLeft:
                renderer.sprite = sprites.endLeft;
                break;
            case WallType.EndRight:
                renderer.sprite = sprites.endRight;
                break;
            default:
                renderer.sprite = sprites.defaultWall;
                break;
        }
        
    }

    private bool IsWall(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
            return false;
        return map[x, y] == 1;
    }

   

    public bool isDone()
    {
        return generateDone;
    }
    

   /// <summary>
    /// 플레이어 오브젝트를 스폰 레이어와 정렬 순서가 자동으로 설정/ 혹시나 따로 하려면 인스펙터에서 수정 좀 해주셔요 그리고 하실때는 말씀해주시고.
    /// 바꾸지 않는게 베스트긴 합니다.
    ///
    /// 현재 player sorting order 10으로 하드코딩 되어있음.
    /// </summary>
    /// <param name="playerObject">플레이어 prefab</param>
    /// <param name="setting">스폰 설정</param>
    /// <returns>생성된 플레이어 오브젝트. 스폰 실패시 null</returns>
    public GameObject SpawnPlayer(GameObject playerObject, SpawnSetting setting)
    {
        if (playerObject == null)
        {
            Debug.LogError("Player prefab is null!");
            return null;
        }

        Vector2? playerSpawn = spawnManager.FindPlayerSpawnPoint(setting);
        if (!playerSpawn.HasValue)
        {
            Debug.LogError("Could not find valid player spawn point!!!");
            return null;
        }

        Vector3 spawnPos = new Vector3(playerSpawn.Value.x, playerSpawn.Value.y, 0);
        GameObject player = Instantiate(playerObject, spawnPos, Quaternion.identity);

        try
        {
            player.layer = LayerMask.NameToLayer("Player");

            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "Player";
                renderer.sortingOrder = 10;
            }

            foreach (Transform child in player.transform)
            {
                child.gameObject.layer = player.layer;
                SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
                if (childRenderer != null)
                {
                    childRenderer.sortingLayerName = "Player";
                    childRenderer.sortingOrder = 10;
                }
            }
        }
        catch (System.Exception e){ Debug.LogError($"Error setting up player object: {e.Message}"); }

        return player;
    }

    /// <summary>
    /// 여러 몬스터를 한번에 스폰합니다.스폰 레이어와 정렬 순서가 자동으로 설정/ 혹시나 따로 하려면 인스펙터에서 수정 좀 해주셔요 그리고 하실때는 말씀해주시고.
    /// 바꾸지 않는게 베스트긴 합니다.
    ///
    /// 현재 monster sorting order 5으로 하드코딩 되어있음.
    /// </summary>
    /// <param name="monsterObjects">몬스터 prefab 리스트</param>
    /// <param name="setting">스폰 설정</param>
    /// <returns>생성된 몬스터 오브젝트 리스트</returns>
    public List<GameObject> SpawnMonsters(List<GameObject> monsterObjects, SpawnSetting setting)
    {
        if (monsterObjects == null || monsterObjects.Count == 0)
        {
            Debug.LogError("Monster prefab list is empty(null일수도)!");
            return new List<GameObject>();
        }

        List<GameObject> spawnedMonsters = new List<GameObject>();
        List<Vector2> monsterSpawns = spawnManager.FindMonsterSpawnPoints(setting, monsterObjects.Count);

        if (monsterSpawns == null || monsterSpawns.Count == 0)
        {
            Debug.LogError("Could not find valid monster spawn points!");
            return spawnedMonsters;
        }

        for (int i = 0; i < Mathf.Min(monsterObjects.Count, monsterSpawns.Count); i++)
        {
            Vector3 spawnPos = new Vector3(monsterSpawns[i].x, monsterSpawns[i].y, 0);
            GameObject monster = Instantiate(monsterObjects[i], spawnPos, Quaternion.identity);

            try
            {
                monster.layer = LayerMask.NameToLayer("Monster");

                SpriteRenderer renderer = monster.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerName = "Monster";
                    renderer.sortingOrder = 5;
                }

                foreach (Transform child in monster.transform)
                {
                    child.gameObject.layer = monster.layer;
                    SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
                    if (childRenderer != null)
                    {
                        childRenderer.sortingLayerName = "Monster";
                        childRenderer.sortingOrder = 5;
                    }
                }

                spawnedMonsters.Add(monster);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up monster object {i}: {e.Message}");
            }
        }

        return spawnedMonsters;
    }

    /// <summary>
    /// 단일 몬스터를 스폰합니다.스폰 레이어와 정렬 순서가 자동으로 설정/ 혹시나 따로 하려면 인스펙터에서 수정 좀 해주셔요 그리고 하실때는 말씀해주시고.
    ///  바꾸지 않는게 베스트긴 합니다.
    ///
    /// 현재 monster sorting order 5으로 하드코딩 되어있음.
    /// </summary>
    /// <param name="monsterObject">몬스터 prefab</param>
    /// <param name="setting">스폰 설정</param>
    /// <returns>생성된 몬스터 오브젝트. 스폰 실패시 null</returns>
    public GameObject SpawnMonster(GameObject monsterObject, SpawnSetting setting)
    {
        if (monsterObject == null)
        {
            Debug.LogError("Monster prefab is null!");
            return null;
        }

        List<Vector2> monsterSpawns = spawnManager.FindMonsterSpawnPoints(setting, 1);
        if (monsterSpawns == null || monsterSpawns.Count == 0)
        {
            Debug.LogError("Could not find valid monster spawn point!");
            return null;
        }

        Vector3 spawnPos = new Vector3(monsterSpawns[0].x, monsterSpawns[0].y, 0);
        GameObject monster = Instantiate(monsterObject, spawnPos, Quaternion.identity);

        try {
            
            monster.layer = LayerMask.NameToLayer("Monster");

            
            SpriteRenderer renderer = monster.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "Monster";
                renderer.sortingOrder = 5;
            }
            
            foreach (Transform child in monster.transform)
            {
                child.gameObject.layer = monster.layer;
                SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
                if (childRenderer != null) {
                    childRenderer.sortingLayerName = "Monster";
                    childRenderer.sortingOrder = 5;
                }
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Error setting up monster object: {e.Message}");
            return null;
        }

        return monster;
    }

    private void SetupObjectHierarchy(GameObject obj, string typeName) {
        Transform parentTransform = GameObject.Find(typeName)?.transform;
        if (parentTransform == null) {
            GameObject parent = new GameObject(typeName);
            parentTransform = parent.transform;
        }
        obj.transform.SetParent(parentTransform);
    }
    
    public BSPRoom GetRoomFromPosition(Vector2 position)
    {
        if (!generateDone)
        {
            Debug.LogWarning("Map generation is not complete. Cannot find room.");
            return null;
        }

        return rooms.FirstOrDefault(room =>
            position.x >= room.x && position.x < room.x + room.width &&
            position.y >= room.y && position.y < room.y + room.height);
    }


    public BSPRoom GetRoomFromTransform(Transform transform)
    {
        if (transform == null) return null;
        return GetRoomFromPosition(new Vector2(transform.position.x, transform.position.y));
    }


    public BSPRoom GetRoomFromGameObject(GameObject obj) {
        if (obj == null) return null;
        return GetRoomFromTransform(obj.transform);
    }
    
    public List<GameObject> SpawnMonstersInRoom(BSPRoom room, List<GameObject> monsterPrefabs, SpawnSetting spawnSetting)
    {
        if (!generateDone || room == null || monsterPrefabs == null || monsterPrefabs.Count == 0)
        {
            Debug.LogError("Cannot spawn monsters: Invalid parameters or map not generated");
            return new List<GameObject>();
        }

        // 룸 크기에 기반한 최대 몬스터 수 계산 물론 내가 임의로
        int maxMonstersForRoom = Mathf.FloorToInt((room.width * room.height) / 20f);
        int monstersToSpawn = Mathf.Min(monsterPrefabs.Count, maxMonstersForRoom);

        var roomSpawnSetting = new SpawnSetting
        {
            spawnName = spawnSetting.spawnName,
            preferredRoomPosition = spawnSetting.preferredRoomPosition,
            preferredMapPosition = MapPosition.Random, // 룸 내부에서만 스폰되므로 맵 위치는 무관
            minDistanceFromWalls = spawnSetting.minDistanceFromWalls,
            minDistanceFromOtherSpawns = spawnSetting.minDistanceFromOtherSpawns,
            minDistanceFromPlayer = spawnSetting.minDistanceFromPlayer,
            requireLineOfSight = spawnSetting.requireLineOfSight,
            maxAttempts = spawnSetting.maxAttempts
        };

        List<GameObject> spawnedMonsters = new List<GameObject>();
        List<Vector2> spawnPoints = FindSpawnPointsInRoom(room, monstersToSpawn, roomSpawnSetting);

        GameObject monstersParent = GameObject.Find("Monsters");
        if (monstersParent == null)
        {
            monstersParent = new GameObject("Monsters");
            monstersParent.transform.parent = transform;
        }

        foreach (var spawnPoint in spawnPoints)
        {
            int prefabIndex = spawnedMonsters.Count % monsterPrefabs.Count;
            Vector3 spawnPos = new Vector3(spawnPoint.x, spawnPoint.y, 0);
            
            GameObject monster = Instantiate(monsterPrefabs[prefabIndex], spawnPos, 
                Quaternion.identity, monstersParent.transform);
            
            SetupMonsterProperties(monster);
            spawnedMonsters.Add(monster);
        }

        return spawnedMonsters;
    }
    
    public GameObject CreateStructureInRoom(BSPRoom room, GameObject structurePrefab, 
        Vector2? preferredPosition = null, float minDistanceFromWalls = 1f)
    {
        if (!generateDone || room == null || structurePrefab == null)
        {
            Debug.LogError("Cannot create structure: Invalid parameters or map not generated");
            return null;
        }

        Renderer structureRenderer = structurePrefab.GetComponent<Renderer>();
        if (structureRenderer == null)
        {
            Debug.LogError("Structure prefab must have a Renderer component");
            return null;
        }

        Vector2 structureSize = structureRenderer.bounds.size;
        Vector2 spawnPosition;

        if (preferredPosition.HasValue) {
            if (!IsPositionInRoom(preferredPosition.Value, room))
            {
                Debug.LogWarning("Preferred position is outside the room. Using room center instead.");
                spawnPosition = new Vector2(room.x + room.width / 2f, room.y + room.height / 2f);
            }
            else
            {
                spawnPosition = preferredPosition.Value;
            }
        }
        else
        {
            spawnPosition = new Vector2(room.x + room.width / 2f, room.y + room.height / 2f);
        }

        if (IsValidStructurePosition(spawnPosition, structureSize, room, minDistanceFromWalls))
        {
            GameObject structure = Instantiate(structurePrefab, 
                new Vector3(spawnPosition.x, spawnPosition.y, 0), 
                Quaternion.identity);

            GameObject structuresParent = GameObject.Find("Structures");
            if (structuresParent == null)
            {
                structuresParent = new GameObject("Structures");
                structuresParent.transform.parent = transform;
            }
            structure.transform.parent = structuresParent.transform;

            SetupStructureProperties(structure);

            return structure;
        }

        Debug.LogWarning("Could not find valid position for structure in room");
        return null;
    }



    
    [Obsolete("Layer가 적용되지 않은 오브젝트를 제외하고 딱히 사용할 이유가 없습니다. -> CreateObjectInRoom을 사용하세요")]
    public GameObject CreatePlayerInRoom(BSPRoom room, GameObject playerPrefab, RoomPosition position)
    {
        if (!generateDone || room == null || playerPrefab == null)
        {
            Debug.LogError("Cannot create player: Invalid parameters or map not generated");
            return null;
        }

        float minDistanceFromWalls = 2f; // 플레이어는 벽과 더 멀리 떨어져야 함
        Vector2 spawnPosition = Vector2.zero;
        bool positionFound = false;
        int attempts = 0;
        const int MAX_ATTEMPTS = 30;

        while (!positionFound && attempts < MAX_ATTEMPTS)
        {
            spawnPosition = CalculatePositionInRoom(room, position, minDistanceFromWalls);
            
            if (IsValidPlayerPosition(spawnPosition, room, minDistanceFromWalls))
            {
                positionFound = true;
            }
            attempts++;
        }

        if (!positionFound)
        {
            Debug.LogError("Could not find valid player spawn position");
            return null;
        }

        GameObject player = Instantiate(playerPrefab, 
            new Vector3(spawnPosition.x, spawnPosition.y, 0), 
            Quaternion.identity);

        SetupPlayerProperties(player);

        return player;
    }

    [Obsolete("Layer가 적용되지 않은 오브젝트를 제외하고 딱히 사용할 이유가 없습니다. -> CreateObjectInRoom을 사용하세요")]
    public GameObject CreateMonsterInRoom(BSPRoom room, GameObject monsterPrefab, RoomPosition position, 
        float minDistanceFromPlayer = 5f)
    {
        if (!generateDone || room == null || monsterPrefab == null)
        {
            Debug.LogError("Cannot create monster: Invalid parameters or map not generated");
            return null;
        }

        Renderer monsterRenderer = monsterPrefab.GetComponent<Renderer>();
        if (monsterRenderer == null)
        {
            Debug.LogError("Monster prefab must have a Renderer component");
            return null;
        }

        Vector2 monsterSize = monsterRenderer.bounds.size;
        float minDistanceFromWalls = 1f;
        Vector2 spawnPosition = Vector2.zero;
        bool positionFound = false;
        int attempts = 0;
        const int MAX_ATTEMPTS = 30;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2? playerPos = player != null ? (Vector2?)player.transform.position : null;
        
        while (!positionFound && attempts < MAX_ATTEMPTS)
        {
            spawnPosition = CalculatePositionInRoom(room, position, minDistanceFromWalls);
            
            if (IsValidMonsterPosition(spawnPosition, monsterSize, room, minDistanceFromWalls, playerPos, minDistanceFromPlayer))
            {
                positionFound = true;
            }
            attempts++;
        }

        if (!positionFound)
        {
            Debug.LogWarning("Could not find valid monster spawn position");
            return null;
        }

        Transform monstersParent = transform.Find("Monsters");
        if (monstersParent == null)
        {
            monstersParent = new GameObject("Monsters").transform;
            monstersParent.parent = transform;
        }

        GameObject monster = Instantiate(monsterPrefab, 
            new Vector3(spawnPosition.x, spawnPosition.y, 0), 
            Quaternion.identity, 
            monstersParent);

        SetupMonsterProperties(monster);

        return monster;
    }

    [Obsolete("Layer가 적용되지 않은 오브젝트를 제외하고 딱히 사용할 이유가 없습니다. -> CreateObjectInRoom을 사용하세요")]
    public GameObject CreateStructureInRoom(BSPRoom room, GameObject structurePrefab, RoomPosition position, 
        bool requireWallContact = false)
    {
        if (!generateDone || room == null || structurePrefab == null)
        {
            Debug.LogError("Cannot create structure: Invalid parameters or map not generated");
            return null;
        }

        Renderer structureRenderer = structurePrefab.GetComponent<Renderer>();
        if (structureRenderer == null)
        {
            Debug.LogError("Structure prefab must have a Renderer component");
            return null;
        }

        Vector2 structureSize = structureRenderer.bounds.size;
        float minDistanceFromWalls = requireWallContact ? 0f : 1f;
        Vector2 spawnPosition = Vector2.zero;
        bool positionFound = false;
        int attempts = 0;
        const int MAX_ATTEMPTS = 30;

        while (!positionFound && attempts < MAX_ATTEMPTS)
        {
            spawnPosition = CalculatePositionInRoom(room, position, minDistanceFromWalls);
            
            if (IsValidStructurePosition(spawnPosition, structureSize, room, minDistanceFromWalls, requireWallContact))
            {
                positionFound = true;
            }
            attempts++;
        }

        if (!positionFound)
        {
            Debug.LogWarning("Could not find valid structure position");
            return null;
        }

        Transform structuresParent = transform.Find("Structures");
        if (structuresParent == null)
        {
            structuresParent = new GameObject("Structures").transform;
            structuresParent.parent = transform;
        }

        GameObject structure = Instantiate(structurePrefab, 
            new Vector3(spawnPosition.x, spawnPosition.y, 0), 
            Quaternion.identity, 
            structuresParent);

        SetupStructureProperties(structure);

        return structure;
    }

    private void SetupPlayerProperties(GameObject player)
    {
        player.layer = LayerMask.NameToLayer("Player");
        
        var renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Player";
            renderer.sortingOrder = 10;
        }

        foreach (Transform child in player.transform)
        {
            child.gameObject.layer = player.layer;
            var childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.sortingLayerName = "Player";
                childRenderer.sortingOrder = 10;
            }
        }
    }

    private void SetupMonsterProperties(GameObject monster)
    {
        monster.layer = LayerMask.NameToLayer("Monster");
        
        var renderer = monster.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Monster";
            renderer.sortingOrder = 5; 
        }

        foreach (Transform child in monster.transform)
        {
            child.gameObject.layer = monster.layer;
            var childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.sortingLayerName = "Monster";
                childRenderer.sortingOrder = 5;
            }
        }
    }

    private void SetupStructureProperties(GameObject structure)
    {
        structure.layer = LayerMask.NameToLayer("Structure");
        
        var renderer = structure.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Structure";
            renderer.sortingOrder = 2;
        }

        foreach (Transform child in structure.transform)
        {
            child.gameObject.layer = structure.layer;
            var childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.sortingLayerName = "Structure";
                childRenderer.sortingOrder = 2;
            }
        }
    }

    private bool IsValidPlayerPosition(Vector2 position, BSPRoom room, float minDistanceFromWalls)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);

        // 맵 경계 검사
        if (!IsInMapBounds(x, y))
            return false;

        // 벽과의 거리 검사
        for (int dx = -Mathf.CeilToInt(minDistanceFromWalls); dx <= Mathf.CeilToInt(minDistanceFromWalls); dx++)
        {
            for (int dy = -Mathf.CeilToInt(minDistanceFromWalls); dy <= Mathf.CeilToInt(minDistanceFromWalls); dy++)
            {
                int checkX = x + dx;
                int checkY = y + dy;
                if (!IsInMapBounds(checkX, checkY) || map[checkX, checkY] != 0)
                    return false;
            }
        }

        return true;
    }

    private bool IsValidMonsterPosition(Vector2 position, Vector2 monsterSize, BSPRoom room, 
        float minDistanceFromWalls, Vector2? playerPos, float minDistanceFromPlayer)
    {
        if (!IsValidStructurePosition(position, monsterSize, room, minDistanceFromWalls))
            return false;

        if (playerPos.HasValue)
        {
            float distanceToPlayer = Vector2.Distance(position, playerPos.Value);
            if (distanceToPlayer < minDistanceFromPlayer)
                return false;
        }

        return true;
    }

    private bool IsValidStructurePosition(Vector2 position, Vector2 structureSize, BSPRoom room, 
        float minDistanceFromWalls, bool requireWallContact = false)
    {
        Rect structureBounds = new Rect(
            position.x - structureSize.x/2f - minDistanceFromWalls,
            position.y - structureSize.y/2f - minDistanceFromWalls,
            structureSize.x + minDistanceFromWalls * 2,
            structureSize.y + minDistanceFromWalls * 2
        );

        if (!IsRectInRoom(structureBounds, room))
            return false;
        
        bool hasWallContact = false;
        for (int x = Mathf.FloorToInt(structureBounds.xMin); x <= Mathf.CeilToInt(structureBounds.xMax); x++)
        {
            for (int y = Mathf.FloorToInt(structureBounds.yMin); y <= Mathf.CeilToInt(structureBounds.yMax); y++)
            {
                if (!IsInMapBounds(x, y))
                    return false;

                if (map[x, y] == 1) // 벽
                {
                    if (requireWallContact)
                        hasWallContact = true;
                    else
                        return false;
                }
            }
        }

        return !requireWallContact || hasWallContact;
    }
    private List<Vector2> FindSpawnPointsInRoom(BSPRoom room, int count, SpawnSetting spawnSetting)
    {
        List<Vector2> points = new List<Vector2>();
        int attempts = 0;
        const int maxAttempts = 100;

        while (points.Count < count && attempts < maxAttempts)
        {
            Vector2 candidatePoint = new Vector2(
                Random.Range(room.x + 1, room.x + room.width - 1),
                Random.Range(room.y + 1, room.y + room.height - 1)
            );

            if (IsValidSpawnPointInRoom(candidatePoint, points, room, spawnSetting))
            {
                points.Add(candidatePoint);
            }

            attempts++;
        }

        return points;
    }

    private bool IsValidSpawnPointInRoom(Vector2 point, List<Vector2> existingPoints, 
        BSPRoom room, SpawnSetting spawnSetting)
    {
        if (!IsPositionInRoom(point, room)) return false;

        int x = Mathf.RoundToInt(point.x);
        int y = Mathf.RoundToInt(point.y);
        if (!IsInMapBounds(x, y) || map[x, y] != 0) return false;

        if (existingPoints.Any(p => Vector2.Distance(p, point) < spawnSetting.minDistanceFromOtherSpawns))
            return false;

        return true;
    }

    private bool IsValidStructurePosition(Vector2 position, Vector2 structureSize, 
        BSPRoom room, float minDistanceFromWalls)
    {
        Rect structureBounds = new Rect(
            position.x - structureSize.x/2f - minDistanceFromWalls,
            position.y - structureSize.y/2f - minDistanceFromWalls,
            structureSize.x + minDistanceFromWalls * 2,
            structureSize.y + minDistanceFromWalls * 2
        );

        if (!IsRectInRoom(structureBounds, room)) return false;

        for (int x = Mathf.FloorToInt(structureBounds.xMin); x <= Mathf.CeilToInt(structureBounds.xMax); x++)
        {
            for (int y = Mathf.FloorToInt(structureBounds.yMin); y <= Mathf.CeilToInt(structureBounds.yMax); y++)
            {
                if (!IsInMapBounds(x, y) || map[x, y] != 0) return false;
            }
        }

        return true;
    }

    private bool IsPositionInRoom(Vector2 position, BSPRoom room)
    {
        return position.x >= room.x && position.x < room.x + room.width &&
               position.y >= room.y && position.y < room.y + room.height;
    }

    private bool IsRectInRoom(Rect rect, BSPRoom room)
    {
        return rect.xMin >= room.x && rect.xMax <= room.x + room.width &&
               rect.yMin >= room.y && rect.yMax <= room.y + room.height;
    }

    private bool IsInMapBounds(int x, int y)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }
    
    private Vector2 CalculatePositionInRoom(BSPRoom room, RoomPosition position, float padding = 1f)
    {
        switch (position)
        {
            case RoomPosition.Center:
                return new Vector2(
                    room.x + room.width / 2f,
                    room.y + room.height / 2f
                );

            case RoomPosition.NearWall:
                return GetRandomWallPosition(room, padding);

            case RoomPosition.Corner:
                return GetRandomCornerPosition(room, padding);

            case RoomPosition.Random:
            default:
                return new Vector2(
                    Random.Range(room.x + padding, room.x + room.width - padding),
                    Random.Range(room.y + padding, room.y + room.height - padding)
                );
        }
    }


    public GameObject CreateObjectInRoom(BSPRoom room, GameObject prefab, RoomPosition position, 
        float minDistanceFromWalls = 1f) {
        if (!generateDone || room == null || prefab == null)
        {
            Debug.LogError("Cannot create object: Invalid parameters or map not generated");
            return null;
        }

        Renderer objectRenderer = prefab.GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("Prefab must have a Renderer component");
            return null;
        }

        Vector2 objectSize = objectRenderer.bounds.size;
        float padding = Mathf.Max(minDistanceFromWalls, objectSize.x/2f, objectSize.y/2f);

        int maxAttempts = 30;
        int attempts = 0;
        Vector2 spawnPosition;

        do
        {
            spawnPosition = CalculatePositionInRoom(room, position, padding);
            attempts++;

            if (IsValidObjectPosition(spawnPosition, objectSize, room, minDistanceFromWalls))
                break;

        } while (attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"Could not find valid position for {position} placement after {maxAttempts} attempts");
            return null;
        }

        GameObject obj = Instantiate(prefab, 
            new Vector3(spawnPosition.x, spawnPosition.y, 0), 
            Quaternion.identity);

        SetupObjectProperties(obj);

        return obj;
    }


    private Vector2 GetRandomWallPosition(BSPRoom room, float padding)
    {
        // (0: 위, 1: 오른쪽, 2: 아래, 3: 왼쪽)
        int wall = Random.Range(0, 4);
        
        switch (wall)
        {
            case 0: // 위쪽 벽
                return new Vector2(
                    Random.Range(room.x + padding, room.x + room.width - padding),
                    room.y + room.height - padding
                );
            case 1: // 오른쪽 벽
                return new Vector2(
                    room.x + room.width - padding,
                    Random.Range(room.y + padding, room.y + room.height - padding)
                );
            case 2: // 아래쪽 벽
                return new Vector2(
                    Random.Range(room.x + padding, room.x + room.width - padding),
                    room.y + padding
                );
            default: // 왼쪽 벽
                return new Vector2(
                    room.x + padding,
                    Random.Range(room.y + padding, room.y + room.height - padding)
                );
        }
    }


    private Vector2 GetRandomCornerPosition(BSPRoom room, float padding) {
        int corner = Random.Range(0, 4);
        
        switch (corner)
        {
            case 0: // 좌상단
                return new Vector2(room.x + padding, room.y + room.height - padding);
            case 1: // 우상단
                return new Vector2(room.x + room.width - padding, room.y + room.height - padding);
            case 2: // 좌하단
                return new Vector2(room.x + padding, room.y + padding);
            default: // 우하단
                return new Vector2(room.x + room.width - padding, room.y + padding);
        }
    }


    private void SetupObjectProperties(GameObject obj) {
        string objType = obj.tag;
        switch (objType)
        {
            case "Monster":
                obj.layer = LayerMask.NameToLayer("Monster");
                SetSpriteProperties(obj, "Monster", 5);
                break;
            case "Structure":
                obj.layer = LayerMask.NameToLayer("Structure");
                SetSpriteProperties(obj, "Structure", 2);
                break;
            default:
                obj.layer = LayerMask.NameToLayer("Default");
                SetSpriteProperties(obj, "Default", 1);
                break;
        }
    }


    private void SetSpriteProperties(GameObject obj, string sortingLayerName, int sortingOrder)
    {
        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }

        foreach (Transform child in obj.transform)
        {
            var childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.sortingLayerName = sortingLayerName;
                childRenderer.sortingOrder = sortingOrder;
            }
        }
    }


    private bool IsValidObjectPosition(Vector2 position, Vector2 objectSize, BSPRoom room, float minDistanceFromWalls)
    {
        Rect objectBounds = new Rect(
            position.x - objectSize.x/2f - minDistanceFromWalls,
            position.y - objectSize.y/2f - minDistanceFromWalls,
            objectSize.x + minDistanceFromWalls * 2,
            objectSize.y + minDistanceFromWalls * 2
        );

        if (!IsRectInRoom(objectBounds, room))
            return false;

        for (int x = Mathf.FloorToInt(objectBounds.xMin); x <= Mathf.CeilToInt(objectBounds.xMax); x++)
        {
            for (int y = Mathf.FloorToInt(objectBounds.yMin); y <= Mathf.CeilToInt(objectBounds.yMax); y++)
            {
                if (!IsInMapBounds(x, y) || map[x, y] != 0)
                    return false;
            }
        }

        return true;
    }
    private void OnDrawGizmos()
    {
        if (!debug) return;
        if (map == null) return;

        for (int x = 0; x < mapWidth; x++) {
            for (int y = 0; y < mapHeight; y++) {
                if (map[x, y] == 1)
                    Gizmos.color = Color.black;
                else
                    Gizmos.color = Color.white;
                Vector3 pos = new Vector3(x, y, 0);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }
}

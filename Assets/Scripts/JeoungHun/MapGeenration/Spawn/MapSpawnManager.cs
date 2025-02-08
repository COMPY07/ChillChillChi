using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapSpawnManager
{
    private readonly int[,] map;  // 0: 빈 공간, 1: 벽, 2: 구조물
    private readonly List<BSPRoom> rooms;
    private readonly List<Vector2> existingSpawns = new List<Vector2>();
    private Vector2? playerPosition;
    
    private const int MAX_GLOBAL_ATTEMPTS = 1000;
    private const int MIN_ROOM_SIZE_FOR_SPAWN = 4;
    private const float DEFAULT_STEP_SIZE = 0.5f;
    private const float EDGE_THRESHOLD = 0.2f;

    public MapSpawnManager(int[,] map, List<BSPRoom> rooms)
    {
        this.map = map ?? throw new System.ArgumentNullException(nameof(map));
        this.rooms = rooms ?? throw new System.ArgumentNullException(nameof(rooms));
        
        if (!rooms.Any())
            throw new System.ArgumentException("[MapSpawnManager]: rooms not provided");

        ValidateRooms();
    }


    private void ValidateRooms() {
        foreach (BSPRoom room in rooms) {
            bool isValid = true;
            string warnings = "";

            if (room.x < 0 || room.y < 0) {
                isValid = false;
                warnings += "Negative coordinates. ";
            }

            if (room.x + room.width > map.GetLength(0)) {
                isValid = false;
                warnings += "Exceeds X bounds. ";
            }

            if (room.y + room.height > map.GetLength(1)) {
                isValid = false;
                warnings += "Exceeds Y bounds. ";
            }

            if (!isValid)
                Debug.LogWarning($"Invalid room at ({room.x}, {room.y}), size: {room.width}x{room.height}. Issues: {warnings}");
            
        }
    }

    public Vector2? FindPlayerSpawnPoint(SpawnSetting settings)
    {
        if (settings == null)
        {
            Debug.LogError("Spawn settings cannot be null");
            return null;
        }

        Debug.Log($"Finding player spawn point with settings: MinDistanceFromWalls={settings.minDistanceFromWalls}, " +
                 $"PreferredMapPosition={settings.preferredMapPosition}");

        List<BSPRoom> validRooms = GetValidRooms();
        if (!validRooms.Any()) return null;

        BSPRoom targetRoom = FindTargetRoom(settings.preferredMapPosition, validRooms);
        if (targetRoom == null)
        {
            Debug.LogWarning("Using random room as fallback");
            targetRoom = validRooms[Random.Range(0, validRooms.Count)];
        }

        Debug.Log($"Selected room: ({targetRoom.x}, {targetRoom.y}), size: {targetRoom.width}x{targetRoom.height}");

        Vector2? spawnPoint = FindSpawnPointInRoom(targetRoom, settings);
        if (!spawnPoint.HasValue) {
            Debug.LogWarning("Could not find valid spawn point in target room. Trying other rooms...");
            foreach (BSPRoom room in validRooms.Where(r => r != targetRoom)) {
                spawnPoint = FindSpawnPointInRoom(room, settings);
                if (spawnPoint.HasValue) break;
            }
        }
        if (spawnPoint.HasValue) {
            playerPosition = spawnPoint;
            existingSpawns.Add(spawnPoint.Value);
            Debug.Log($"Successfully found player spawn point at: {spawnPoint.Value}");
        }
        else 
            Debug.LogError("Failed to find valid player spawn point in any room");
        

        return spawnPoint;
    }

    public List<Vector2> FindMonsterSpawnPoints(SpawnSetting settings, int count)
    {
        if (settings == null || count <= 0) {
            Debug.LogError($"Invalid spawn request: settings={settings != null}, count={count}");
            return new List<Vector2>();
        }

        List<Vector2> spawnPoints = new List<Vector2>();
        int globalAttempts = 0;
        int successfulSpawns = 0;
        List<BSPRoom> validRooms = GetValidRooms();

        Debug.Log($"Attempting to spawn {count} monsters");

        while (successfulSpawns < count && globalAttempts < MAX_GLOBAL_ATTEMPTS) {
            BSPRoom targetRoom = FindTargetRoom(settings.preferredMapPosition, validRooms);
            if (targetRoom == null) continue;

            Vector2? spawnPoint = FindSpawnPointInRoom(targetRoom, settings);
            if (spawnPoint.HasValue && IsValidSpawnPoint(spawnPoint.Value, settings))
            {
                spawnPoints.Add(spawnPoint.Value);
                existingSpawns.Add(spawnPoint.Value);
                successfulSpawns++;
                Debug.Log($"Found monster spawn point {successfulSpawns}/{count} at: {spawnPoint.Value}");
            }

            globalAttempts++;
        }

        if (successfulSpawns < count)
        {
            Debug.LogWarning($"Only generated {successfulSpawns}/{count} spawn points after {globalAttempts} attempts");
        }

        return spawnPoints;
    }

    private List<BSPRoom> GetValidRooms() {
        List<BSPRoom> validRooms = rooms.Where(r =>
            r.width >= MIN_ROOM_SIZE_FOR_SPAWN &&
            r.height >= MIN_ROOM_SIZE_FOR_SPAWN).ToList();

        Debug.Log($"Found {validRooms.Count} valid rooms out of {rooms.Count} total rooms");
        return validRooms;
    }

    private BSPRoom FindTargetRoom(MapPosition mapPosition, List<BSPRoom> validRooms)
    {
        if (!validRooms.Any()) return null;

        try
        {
            BSPRoom selectedRoom = null;
            Vector2 mapCenter = new Vector2(map.GetLength(0) / 2f, map.GetLength(1) / 2f);

            switch (mapPosition)
            {
                case MapPosition.Center:
                    selectedRoom = validRooms
                        .OrderBy(r => Vector2.Distance(
                            new Vector2(r.x + r.width/2f, r.y + r.height/2f),
                            mapCenter))
                        .FirstOrDefault();
                    Debug.Log("Selecting central room");
                    break;

                case MapPosition.Edge:
                    List<BSPRoom> edgeRooms = validRooms.Where(r =>
                        r.x < map.GetLength(0) * EDGE_THRESHOLD ||
                        r.x > map.GetLength(0) * (1 - EDGE_THRESHOLD) ||
                        r.y < map.GetLength(1) * EDGE_THRESHOLD ||
                        r.y > map.GetLength(1) * (1 - EDGE_THRESHOLD)).ToList();

                    selectedRoom = edgeRooms.Any()
                        ? edgeRooms[Random.Range(0, edgeRooms.Count)]
                        : validRooms[Random.Range(0, validRooms.Count)];
                    Debug.Log($"Selected edge room. Edge rooms available: {edgeRooms.Count}");
                    break;

                case MapPosition.Random:
                default:
                    selectedRoom = validRooms[Random.Range(0, validRooms.Count)];
                    Debug.Log("Selecting random room");
                    break;
            }

            return selectedRoom;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error finding target room: {e.Message}");
            return null;
        }
    }

    private Vector2? FindSpawnPointInRoom(BSPRoom room, SpawnSetting settings)
    {
        if (room == null || settings == null) return null;

        List<Vector2> candidatePoints = new List<Vector2>();
        int attempts = 0;
        int maxLocalAttempts = settings.maxAttempts;

        Debug.Log($"Searching for spawn point in room: ({room.x}, {room.y}), " +
                 $"size: {room.width}x{room.height}, attempt: {attempts + 1}/{maxLocalAttempts}");

        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                Vector2 point = new Vector2(x, y);
                if (IsValidSpawnPoint(point, settings))
                {
                    candidatePoints.Add(point);
                }
            }
        }

        if (candidatePoints.Count == 0)
        {
            Debug.Log($"No valid spawn points found in room after {attempts + 1} attempts");
            return null;
        }

        Debug.Log($"Found {candidatePoints.Count} valid spawn points in room");

        try
        {
            Vector2 selectedPoint;
            Vector2 roomCenter = new Vector2(room.x + room.width/2f, room.y + room.height/2f);

            switch (settings.preferredRoomPosition)
            {
                case RoomPosition.Center:
                    selectedPoint = candidatePoints
                        .OrderBy(p => Vector2.Distance(p, roomCenter))
                        .First();
                    break;

                case RoomPosition.NearWall:
                    selectedPoint = candidatePoints
                        .OrderBy(p => GetDistanceFromNearestWall(p))
                        .First();
                    break;

                case RoomPosition.Corner:
                    Vector2 nearestCorner = GetNearestCorner(room, candidatePoints[0]);
                    selectedPoint = candidatePoints
                        .OrderBy(p => Vector2.Distance(p, nearestCorner))
                        .First();
                    break;

                case RoomPosition.Random:
                default:
                    selectedPoint = candidatePoints[Random.Range(0, candidatePoints.Count)];
                    break;
            }

            Debug.Log($"Selected spawn point at {selectedPoint}");
            return selectedPoint;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error selecting spawn point: {e.Message}");
            return candidatePoints.FirstOrDefault();
        }
    }

    private bool IsValidSpawnPoint(Vector2 point, SpawnSetting settings)
    {
        try
        {
            // 맵 경계 검사
            if (!IsInMapBounds((int)point.x, (int)point.y))
                return false;
            

            // 빈 공간 검사
            if (map[(int)point.x, (int)point.y] != 0)
                return false;
            

            // 벽과의 거리 검사
            float wallDistance = GetDistanceFromNearestWall(point);
            if (wallDistance < settings.minDistanceFromWalls)
                return false;
            

            // 다른 스폰 지점과의 거리 검사
            if (existingSpawns.Any(s => Vector2.Distance(s, point) < settings.minDistanceFromOtherSpawns))
                return false;
            

            // 플레이어와의 거리 검사
            if (playerPosition.HasValue && settings.minDistanceFromPlayer > 0) {
                if (Vector2.Distance(playerPosition.Value, point) < settings.minDistanceFromPlayer)
                    return false;
                
            }

            // 시야 확보 검사
            if (settings.requireLineOfSight && playerPosition.HasValue) {
                if (!HasLineOfSight(point, playerPosition.Value))
                    return false;
                
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error validating spawn point: {e.Message}");
            return false;
        }
    }

    private float GetDistanceFromNearestWall(Vector2 point)
    {
        float minDistance = float.MaxValue;
        int checkRadius = Mathf.Min(5, Mathf.Min(map.GetLength(0), map.GetLength(1)) / 4);

        for (int x = -checkRadius; x <= checkRadius; x++)
        {
            for (int y = -checkRadius; y <= checkRadius; y++)
            {
                int checkX = (int)point.x + x;
                int checkY = (int)point.y + y;

                if (IsInMapBounds(checkX, checkY) && map[checkX, checkY] == 1)
                {
                    float distance = Vector2.Distance(point, new Vector2(checkX, checkY));
                    minDistance = Mathf.Min(minDistance, distance);
                }
            }
        }

        return minDistance;
    }

    private Vector2 GetNearestCorner(BSPRoom room, Vector2 point)
    {
        Vector2[] corners = {
            new Vector2(room.x, room.y),
            new Vector2(room.x + room.width, room.y),
            new Vector2(room.x, room.y + room.height),
            new Vector2(room.x + room.width, room.y + room.height)
        };

        return corners.OrderBy(c => Vector2.Distance(c, point)).First();
    }

    private bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        float distance = direction.magnitude;
        direction.Normalize();

        for (float i = 0; i < distance; i += DEFAULT_STEP_SIZE)
        {
            Vector2 checkPoint = from + direction * i;
            int x = Mathf.RoundToInt(checkPoint.x);
            int y = Mathf.RoundToInt(checkPoint.y);

            if (!IsInMapBounds(x, y) || map[x, y] == 1)
                return false;
            
        }

        return true;
    }

    private bool IsInMapBounds(int x, int y)
    {
        return x >= 0 && x < map.GetLength(0) && 
               y >= 0 && y < map.GetLength(1);
    }
}
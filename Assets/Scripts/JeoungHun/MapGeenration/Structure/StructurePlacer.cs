using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StructurePlacer
{
    private int[,] map;  // 0: 빈 공간, 1: 벽, 2: 구조물
    private List<Vector2Int> placedStructurePositions = new List<Vector2Int>();
    private GeneratorSetting settings;
    private int layerId;
    public StructurePlacer(int[,] map, GeneratorSetting settings) {
        this.map = map;
        this.settings = settings;
        layerId = LayerMask.NameToLayer("Structure");
        if (layerId == -1) {
            Debug.LogError($"Layer '{layerId}' not found! 이거 추가좀 이거 왜 안고쳐짐?");
            layerId = 0;
        }
    }

    public void PlaceStructures(Transform parent)
    {
        var sortedStructures = settings.structures
            .OrderBy(s => s.priority)
            .ThenByDescending(s => s.width * s.height)
            .ToList();

        foreach (var structure in sortedStructures)
        {
            int structuresToPlace = Random.Range(structure.minCount, structure.maxCount + 1);
            int attemptCount = 0;

            for (int i = 0; i < structuresToPlace; i++)
            {
                bool placed = false;
                while (!placed && attemptCount < settings.maxPlacementAttempts)
                {
                    placed = TryPlaceStructure(structure, parent);
                    attemptCount++;
                }

                if (!placed && structure.priority == StructurePriority.Mandatory)
                {
                    Debug.LogWarning($"Could not place mandatory structure: {structure.structureName}");
                }
            }
        }
    }

    private bool TryPlaceStructure(CompyStructure structure, Transform parent) {
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);

        List<Vector2Int> validPositions = new List<Vector2Int>();

        for (int x = structure.padding; x < mapWidth - structure.width - structure.padding; x++)
        {
            for (int y = structure.padding; y < mapHeight - structure.height - structure.padding; y++)
            {
                if (IsValidPosition(new Vector2Int(x, y), structure))
                {
                    validPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (validPositions.Count == 0)
            return false;

        Vector2Int selectedPosition = validPositions[Random.Range(0, validPositions.Count)];
        
        PlaceStructureAt(selectedPosition, structure, parent);
        
        return true;
    }

    private bool IsValidPosition(Vector2Int position, CompyStructure structure)
    {
        for (int x = -structure.padding; x < structure.width + structure.padding; x++)
        {
            for (int y = -structure.padding; y < structure.height + structure.padding; y++)
            {
                Vector2Int checkPos = new Vector2Int(position.x + x, position.y + y);
                
                if (!IsInMapBounds(checkPos))
                    return false;

                if (x >= 0 && x < structure.width && y >= 0 && y < structure.height) {
                    if (map[checkPos.x, checkPos.y] != 0) // 비어있지 않으면 invalid
                        return false;
                }
            }
        }

        if (structure.requireWallConnection)
        {
            bool hasWallConnection = false;
            for (int x = -1; x <= structure.width; x++) {
                for (int y = -1; y <= structure.height; y++) {
                    if (x == -1 || x == structure.width || y == -1 || y == structure.height) {
                        Vector2Int checkPos = new Vector2Int(position.x + x, position.y + y);
                        if (IsInMapBounds(checkPos) && map[checkPos.x, checkPos.y] == 1) {
                            hasWallConnection = true;
                            break;
                        }
                    }
                }
                if (hasWallConnection) break;
            }
            if (!hasWallConnection) return false;
        }

        foreach (var placedPos in placedStructurePositions) {
            float distance = Vector2Int.Distance(position, placedPos);
            if (distance < structure.minDistanceFromOthers)
                return false;
        }

        return true;
    }

    private void PlaceStructureAt(Vector2Int position, CompyStructure structure, Transform parent)
    {
        for (int x = 0; x < structure.width; x++) {
            for (int y = 0; y < structure.height; y++)
                map[position.x + x, position.y + y] = 2;
        }


        Transform structuresParent = parent.Find("Structures");
        if (structuresParent == null) {
            structuresParent = new GameObject("Structures").transform;
            structuresParent.parent = parent;
            structuresParent.gameObject.layer = layerId;
        }

        Transform structureTypeParent = structuresParent.Find(structure.structureName);
        if (structureTypeParent == null) {
            structureTypeParent = new GameObject(structure.structureName).transform;
            structureTypeParent.parent = structuresParent;
            structuresParent.gameObject.layer = layerId;
        }

        Vector3 worldPosition = new Vector3(position.x + structure.width / 2f, 
                                          position.y + structure.height / 2f, 
                                          0);
        GameObject structureObj = GameObject.Instantiate(structure.structurePrefab, 
                                                       worldPosition, 
                                                       Quaternion.identity, 
                                                       structureTypeParent);
        
        
        structureObj.name = $"{structure.structureName}_{placedStructurePositions.Count:D4}";
        structureObj.layer = layerId;
        SpriteRenderer childRenderer = structureObj.GetComponent<SpriteRenderer>();
        if (childRenderer != null) {
            childRenderer.sortingLayerName = "Structure";
            childRenderer.sortingOrder = 5;
        }
        placedStructurePositions.Add(position);
    }

    private bool IsInMapBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < map.GetLength(0) &&
               position.y >= 0 && position.y < map.GetLength(1);
    }
}
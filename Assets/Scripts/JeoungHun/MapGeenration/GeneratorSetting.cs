using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "MapSettings", menuName = "Compy/Generator Settings")]
public class GeneratorSetting : ScriptableObject
{
    [Header("Generation Type")]
    [Tooltip("맵 생성에 사용할 알고리즘")]
    [SerializeField] private GenerationType generationType = GenerationType.BSP;
    public GenerationType GenerationType => generationType;

    [Header("Map Size")]
    [Tooltip("전체 너비")]
    public int mapWidth = 100;
    
    [Tooltip("전체 높이")]
    public int mapHeight = 100;

    [Header("Room Generation")]
    [Tooltip("방의 최소 크기")]
    [Min(5)]
    public int minRoomSize = 10;
    
    [Tooltip("BSP 분할 최대 횟수")]
    [Range(1, 10)]
    public int maxIterations = 4;

    [Header("Room Constraints")]
    [Tooltip("방 사이의 최소 거리")]
    [Min(1)]
    public int minRoomDistance = 2;
    
    [Tooltip("방의 최대 크기 (전체 공간 대비 비율)")]
    [Range(0.3f, 0.8f)]
    public float maxRoomSizeRatio = 0.6f;

    [Header("Tile Settings")]
    [Tooltip("벽 타일 설정")]
    public WallTileSettings wallTileSettings;

    [Tooltip("기본 타일 프리팹 (스프라이트 렌더러와 콜라이더가 있는 빈 게임오브젝트)")]
    public GameObject tilePrefab;

    [Header("Prefabs")]
    [Tooltip("바닥 타일 프리팹")]
    public GameObject floorPrefab;

    [Header("Layer Settings")]
    [Tooltip("벽 레이어의 이름")]
    public string wallLayerName = "Wall";

    [Header("Advanced Settings")]
    [Tooltip("통로 너비")]
    [Range(1, 5)]
    public int corridorWidth = 3;
    
    [Tooltip("방 생성 시도 최대 횟수")]
    [Min(1)]
    public int maxRoomPlacementAttempts = 10;

    [Tooltip("추가 통로 생성 확률 (0-1)")]
    [Range(0f, 1f)]
    public float extraCorridorChance = 0.2f;
    
    [Header("Structures")]
    [Tooltip("맵에 배치할 구조물들의 정의")]
    public List<CompyStructure> structures = new List<CompyStructure>();

    [Tooltip("구조물 배치 최대 시도 횟수")]
    [Min(1)]
    public int maxPlacementAttempts = 100;

    
    
    
    public void ValidateSettings()
    {
        mapWidth = Mathf.Max(mapWidth, minRoomSize * 2);
        mapHeight = Mathf.Max(mapHeight, minRoomSize * 2);

        minRoomSize = Mathf.Min(minRoomSize, Mathf.Min(mapWidth, mapHeight) / 2);
        
        corridorWidth = Mathf.Min(corridorWidth, minRoomSize - 2);
        minRoomDistance = Mathf.Max(minRoomDistance, corridorWidth);

        ValidateStructures();
    }


    private void ValidateStructures()
    {
        if (structures == null) return;

        foreach (var structure in structures)
        {
            if (structure == null) continue;
            structure.width = Mathf.Min(structure.width, mapWidth - 4);
            structure.height = Mathf.Min(structure.height, mapHeight - 4);

            structure.minCount = Mathf.Min(structure.minCount, structure.maxCount);

            if (structure.padding > 0) 
                structure.padding = Mathf.Min(structure.padding,
                    Mathf.Min(structure.width, structure.height) / 2);
            
        }
    }
    
    public GeneratorSetting Clone()
    {
        var clone = CreateInstance<GeneratorSetting>();
        
        clone.generationType = this.generationType;
        clone.mapWidth = this.mapWidth;
        clone.mapHeight = this.mapHeight;
        clone.minRoomSize = this.minRoomSize;
        clone.maxIterations = this.maxIterations;
        clone.minRoomDistance = this.minRoomDistance;
        clone.maxRoomSizeRatio = this.maxRoomSizeRatio;
        clone.wallTileSettings = this.wallTileSettings;
        clone.tilePrefab = this.tilePrefab;
        clone.floorPrefab = this.floorPrefab;
        clone.wallLayerName = this.wallLayerName;
        clone.corridorWidth = this.corridorWidth;
        clone.maxRoomPlacementAttempts = this.maxRoomPlacementAttempts;
        clone.extraCorridorChance = this.extraCorridorChance;

        clone.structures = new List<CompyStructure>(this.structures);
        clone.maxPlacementAttempts = this.maxPlacementAttempts;

        return clone;
    }

#if UNITY_EDITOR
    // 내가 한번 당하지 두번은 안 당할거임 ㄹㅇ

    private void OnValidate()
    {
        ValidateSettings();
    }
#endif
}

using UnityEngine;

[System.Serializable]
public class CompyStructure
{
    [Tooltip("구조물의 이름")]
    public string structureName;

    [Tooltip("구조물 프리팹")]
    public GameObject structurePrefab;

    [Tooltip("구조물의 가로 크기 (타일 단위)")]
    [Min(1)]
    public int width = 1;

    [Tooltip("구조물의 세로 크기 (타일 단위)")]
    [Min(1)]
    public int height = 1;

    [Tooltip("이 구조물의 최소 배치 개수")]
    [Min(0)]
    public int minCount;

    [Tooltip("이 구조물의 최대 배치 개수")]
    [Min(0)]
    public int maxCount = 1;

    [Tooltip("구조물 주변에 필요한 여유 공간 (타일 단위)")]
    [Min(0)]
    public int padding = 1;

    [Tooltip("구조물의 배치 우선순위")]
    public StructurePriority priority = StructurePriority.Normal;

    [Tooltip("구조물이 벽에 붙어야 하는지 여부")]
    public bool requireWallConnection = false;

    [Tooltip("다른 구조물과의 최소 거리")]
    [Min(0)]
    public int minDistanceFromOthers = 2;
}
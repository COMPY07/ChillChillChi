public enum StructurePriority
{
    Mandatory,  // 반드시 배치해야 하는 구조물
    High,       // 가능한 배치하려고 시도
    Normal,     // 공간이 있으면 배치
    Low         // 여유 공간이 충분할 때만 배치
}
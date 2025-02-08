using UnityEngine;

[CreateAssetMenu(fileName = "WallTileSettings", menuName = "Compy/Wall Tile Settings")]
public class WallTileSettings : ScriptableObject
{
    
    [System.Serializable]
    public class WallSprites
    {
        [Header("기본 벽 스프라이트")]
        [Tooltip("주변에 아무 연결이 없는 기본적인 벽 스프라이트")]
        public Sprite defaultWall;

        [Header("벽 테두리")]
        [Tooltip("아래쪽으로 연결된 상단 벽면")] // │ 의 윗부분
        public Sprite wallTop;
        [Tooltip("위쪽으로 연결된 하단 벽면")] // │ 의 아랫부분
        public Sprite wallBottom;
        [Tooltip("오른쪽으로 연결된 왼쪽 벽면")] // ─ 의 왼쪽
        public Sprite wallLeft;
        [Tooltip("왼쪽으로 연결된 오른쪽 벽면")] // ─ 의 오른쪽
        public Sprite wallRight;

        [Header("벽 모서리 - 외부")]
        [Tooltip("오른쪽과 아래로 연결된 외부 모서리")] // ┌ 모양
        public Sprite cornerOuterTopLeft;
        [Tooltip("왼쪽과 아래로 연결된 외부 모서리")] // ┐ 모양
        public Sprite cornerOuterTopRight;
        [Tooltip("오른쪽과 위로 연결된 외부 모서리")] // └ 모양
        public Sprite cornerOuterBottomLeft;
        [Tooltip("왼쪽과 위로 연결된 외부 모서리")] // ┘ 모양
        public Sprite cornerOuterBottomRight;

        [Header("벽 모서리 - 내부")]
        [Tooltip("위쪽과 왼쪽이 벽인 내부 모서리")] // ┘ 모양이지만 안쪽으로
        public Sprite cornerInnerTopLeft;
        [Tooltip("위쪽과 오른쪽이 벽인 내부 모서리")] // └ 모양이지만 안쪽으로
        public Sprite cornerInnerTopRight;
        [Tooltip("아래쪽과 왼쪽이 벽인 내부 모서리")] // ┐ 모양이지만 안쪽으로
        public Sprite cornerInnerBottomLeft;
        [Tooltip("아래쪽과 오른쪽이 벽인 내부 모서리")] // ┌ 모양이지만 안쪽으로
        public Sprite cornerInnerBottomRight;

        [Header("단독 벽")]
        [Tooltip("주변에 어떤 연결도 없는 단독으로 서있는 벽")]
        public Sprite singleWall;

        [Header("끝부분")]
        [Tooltip("위쪽 방향으로 돌출된 벽의 끝부분 (아래쪽만 연결)")] // ╽ 모양
        public Sprite endTop;
        [Tooltip("아래쪽 방향으로 돌출된 벽의 끝부분 (위쪽만 연결)")] // ╿ 모양
        public Sprite endBottom;
        [Tooltip("왼쪽 방향으로 돌출된 벽의 끝부분 (오른쪽만 연결)")] // ╾ 모양
        public Sprite endLeft;
        [Tooltip("오른쪽 방향으로 돌출된 벽의 끝부분 (왼쪽만 연결)")] // ╼ 모양
        public Sprite endRight;
    }

    [Header("벽 스프라이트 설정")]
    public WallSprites sprites;

    [Header("벽 렌더링 설정")]
    public string sortingLayerName = "Wall";
    public int sortingOrder = 0;
}
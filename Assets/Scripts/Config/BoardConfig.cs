using UnityEngine;
public static class BoardConfig
{
    public const float TileSize = 0.66f;
    public static readonly Vector2 Offset = new Vector2(-2.3f, -2.3f);
    public const int BoardWidth = 8;
    public const int BoardHeight = 8;

    /* Converts a world position (e.g., mouse click) to grid coordinates */
    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float x = (worldPos.x - Offset.x) / TileSize;
        float y = (worldPos.y - Offset.y) / TileSize;
        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    /* Converts a grid coordinate (0–7, 0–7) to world position */
    public static Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            Offset.x + gridPos.x * TileSize,
            Offset.y + gridPos.y * TileSize,
            0f
        );
    }

    public static bool IsInsideBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < BoardWidth && pos.y >= 0 && pos.y < BoardHeight;
    }

    public static Vector2Int ClampToBoard(Vector2Int pos)
    {
        return new Vector2Int(
            Mathf.Clamp(pos.x, 0, BoardWidth - 1),
            Mathf.Clamp(pos.y, 0, BoardHeight - 1)
        );
    }
}


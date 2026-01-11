using UnityEngine;

public class ColorFlipSwitch : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float zOffset = -1f;

    private void Start()
    {
        PlaceOnRandomBlackSquare();
    }

    private void PlaceOnRandomBlackSquare()
    {
        int attempts = 0;
        int maxAttempts = 500;

        while (attempts < maxAttempts)
        {
            attempts++;

            int x = Random.Range(0, BoardConfig.BoardWidth);
            int y = Random.Range(0, BoardConfig.BoardHeight);

            if ((x + y) % 2 != 0)
                continue;


            if (ChessGame.Instance.GetPosition(x, y) != null)
                continue;

            Vector3 worldPos = BoardConfig.GridToWorld(new Vector2Int(x, y));
            worldPos.z = zOffset;

            transform.position = worldPos;
            return;
        }

        Debug.LogWarning("ColorFlipSwitch: Failed to find valid black square");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ChessPiece bishop = other.GetComponent<ChessPiece>();

        if (bishop == null || !bishop.name.Contains("bishop"))
            return;

        FlipColor(bishop);
        Destroy(gameObject);
    }

    private void FlipColor(ChessPiece bishop)
    {
        int x = bishop.xBoard;
        int y = bishop.yBoard;

        Vector2Int target = new Vector2Int(x + 1, y);

        if (!ChessGame.Instance.IsPositionOnBoard(target.x, target.y))
            target = new Vector2Int(x - 1, y);

        if (!ChessGame.Instance.IsPositionOnBoard(target.x, target.y))
            target = new Vector2Int(x, y + 1);

        if (!ChessGame.Instance.IsPositionOnBoard(target.x, target.y))
            target = new Vector2Int(x, y - 1);

        if (!ChessGame.Instance.IsPositionOnBoard(target.x, target.y))
        {
            Debug.LogWarning("No valid square to flip bishop color!");
            return;
        }

        ChessGame.Instance.SetPositionEmpty(x, y);

        bishop.SetXBoard(target.x);
        bishop.SetYBoard(target.y);
        bishop.SetCoords();

        ChessGame.Instance.SetPosition(bishop.gameObject);

        Debug.Log($"[ColorFlip] Bishop moved from ({x},{y}) to ({target.x},{target.y})");
    }
}

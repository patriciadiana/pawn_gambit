using System.Collections;
using UnityEngine;

public class KnightAI : MonoBehaviour
{
    private ChessPiece piece;
    private ChessGame game;

    public float moveInterval = 5f;
    public GameObject targetPawn;

    private void Start()
    {
        piece = GetComponent<ChessPiece>();
        game = ChessGame.Instance;

        StartCoroutine(AutoMoveRoutine());
    }

    IEnumerator AutoMoveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveInterval);
            MakeAIMove();
        }
    }

    void MakeAIMove()
    {
        if (targetPawn == null)
            return;

        ChessPiece pawnScript = targetPawn.GetComponent<ChessPiece>();
        Vector2Int pawnPos = new Vector2Int(pawnScript.xBoard, pawnScript.yBoard);

        Vector2Int[] offsets =
        {
            new Vector2Int( 1,  2), new Vector2Int(-1,  2),
            new Vector2Int( 2,  1), new Vector2Int( 2, -1),
            new Vector2Int( 1, -2), new Vector2Int(-1, -2),
            new Vector2Int(-2,  1), new Vector2Int(-2, -1),
        };

        Vector2Int current = new Vector2Int(piece.xBoard, piece.yBoard);

        float bestDistance = Mathf.Infinity;
        Vector2Int bestMove = current;

        foreach (Vector2Int off in offsets)
        {
            Vector2Int newPos = current + off;

            if (!game.IsPositionOnBoard(newPos.x, newPos.y))
                continue;

            GameObject occupant = game.GetPosition(newPos.x, newPos.y);

            if (occupant != null && occupant != targetPawn)
                continue;

            float dist = Vector2Int.Distance(newPos, pawnPos);

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestMove = newPos;
            }
        }

        MoveTo(bestMove);
    }

    void MoveTo(Vector2Int pos)
    {
        ChessPiece p = GetComponent<ChessPiece>();

        GameObject targetAtPos = game.GetPosition(pos.x, pos.y);
        if (targetAtPos != null)
        {
            if (targetAtPos == targetPawn)
            {
                Destroy(targetPawn);
            }
            else
            {
                return;
            }
        }

        game.SetPositionEmpty(p.xBoard, p.yBoard);

        p.SetXBoard(pos.x);
        p.SetYBoard(pos.y);
        p.SetCoords();

        game.SetPosition(gameObject);
    }
}

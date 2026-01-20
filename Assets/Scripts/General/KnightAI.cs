using System.Collections;
using UnityEngine;

public class KnightAI : MonoBehaviour
{
    private ChessPiece piece;
    private ChessGame game;

    public float moveInterval = 5f;
    public GameObject targetPawn;

    public GameObject knightAlert;
    public float preMoveDisplayTime = 1.5f;

    private Coroutine aiRoutine;

    private void Start()
    {
        piece = GetComponent<ChessPiece>();
        game = ChessGame.Instance;

        if (knightAlert != null)
            knightAlert.SetActive(false);

        aiRoutine = StartCoroutine(AutoMoveRoutine());
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (aiRoutine != null)
        {
            StopCoroutine(aiRoutine);
            aiRoutine = null;
        }

        StopAllCoroutines();

        if (knightAlert != null)
            knightAlert.SetActive(false);
    }

    IEnumerator AutoMoveRoutine()
    {
        while (targetPawn != null)
        {
            yield return new WaitForSeconds(moveInterval);

            yield return StartCoroutine(DisplayAlert(preMoveDisplayTime));

            MakeAIMove();
        }
    }

    IEnumerator DisplayAlert(float duration)
    {
        if (knightAlert == null)
            yield break;

        knightAlert.SetActive(true);

        yield return new WaitForSeconds(duration);

        knightAlert.SetActive(false);
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

        ChessPiece pawnScript = targetPawn != null
            ? targetPawn.GetComponent<ChessPiece>()
            : null;

        if (pawnScript != null &&
            pawnScript.xBoard == pos.x &&
            pawnScript.yBoard == pos.y)
        {
            SoundManager.PlaySound(SoundType.ATTACKPIECE);
            Destroy(targetPawn);
            ChessGame.Instance.HandleGameOver();
        }

        game.SetPositionEmpty(p.xBoard, p.yBoard);

        p.SetXBoard(pos.x);
        p.SetYBoard(pos.y);
        p.SetCoords();

        game.SetPosition(gameObject);
    }
}

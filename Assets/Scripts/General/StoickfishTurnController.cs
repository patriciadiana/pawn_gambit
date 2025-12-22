using System.Collections;
using UnityEngine;

public class StockfishTurnController : MonoBehaviour
{
    private ChessGame chessGame;

    void Start()
    {
        chessGame = ChessGame.Instance;
        chessGame.OnMoveCompleted += OnMoveCompleted;
    }

    void OnDestroy()
    {
        chessGame.OnMoveCompleted -= OnMoveCompleted;
    }

    private void OnMoveCompleted(ChessPiece piece, Vector2Int from, Vector2Int to)
    {
        if (chessGame.isMidgameMode == true)
        {
            chessGame.SwitchTurn();
        }

        /* If the current player is black -> Stockfish moves*/
        if (chessGame.GetCurrentPlayer() == "black")
        {
            StartCoroutine(HandleStockfishTurn());
        }
    }

    private IEnumerator HandleStockfishTurn()
    {
        string fen = ChessGame.GenerateFEN();
        Debug.Log("FEN: " + fen);

        yield return StockfishManager.Instance.GetBestMove(
            fen,
            12,
            ExecuteStockfishMove
        );
    }

    private void ExecuteStockfishMove(string uci)
    {
        if (string.IsNullOrEmpty(uci) || uci == "none")
        {
            Debug.Log("Game over");
            return;
        }

        Vector2Int from = UciToBoard(uci.Substring(0, 2));
        Vector2Int to = UciToBoard(uci.Substring(2, 2));

        GameObject pieceObj = chessGame.GetPosition(from.x, from.y);
        if (pieceObj == null)
        {
            Debug.LogError("No piece at " + from);
            return;
        }

        GameObject target = chessGame.GetPosition(to.x, to.y);
        if (target != null)
            Destroy(target);

        ChessPiece piece = pieceObj.GetComponent<ChessPiece>();

        chessGame.SetPositionEmpty(from.x, from.y);

        piece.SetXBoard(to.x);
        piece.SetYBoard(to.y);
        piece.SetCoords();

        chessGame.SetPosition(pieceObj);
    }

    private Vector2Int UciToBoard(string uci)
    {
        return new Vector2Int(uci[0] - 'a', uci[1] - '1');
    }
}

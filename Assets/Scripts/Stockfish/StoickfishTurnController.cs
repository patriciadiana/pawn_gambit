using System.Collections;
using TMPro;
using UnityEngine;

public class StockfishTurnController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI checkText;
    private ChessGame chessGame;
    void Start()
    {
        chessGame = ChessGame.Instance;
        chessGame.OnMoveCompleted += OnMoveCompleted;
        checkText.gameObject.SetActive(false);
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
            Debug.Log($"did white put black in check ? {ChessGame.IsKingInCheck("black")}");
            UpdateCheckText();
            StartCoroutine(HandleStockfishTurn("black"));
        }
    }

    private IEnumerator HandleStockfishTurn(string turn)
    {
        string fen = ChessGame.GenerateFEN();

        yield return StockfishManager.Instance.GetBestMove(
            fen,
            12,
            ExecuteStockfishMove,
            turn
        );
    }

    private void UpdateCheckText()
    {
        string currentPlayer = chessGame.GetCurrentPlayer();
        bool isInCheck = ChessGame.IsKingInCheck(currentPlayer);

        if (!isInCheck)
        {
            checkText.gameObject.SetActive(false);
            return;
        }

        checkText.gameObject.SetActive(true);

        if (currentPlayer == "white")
        {
            checkText.text = "<color=#FF0000>You're in check! Move the king!</color>";
        }
        else
        {
            checkText.text = "Opponent is in check!";
        }
    }


    private void ExecuteStockfishMove(string uci, string turn)
    {
        if (string.IsNullOrEmpty(uci) || uci == "(none)")
        {
            if(turn=="white")
            {
                if (ChessGame.IsKingInCheck("white") == false)
                {
                    Debug.Log("stalemate");
                    ChessGame.Instance.HandleGameOver();
                }
                else
                {
                    Debug.Log("black wins");
                    ChessGame.Instance.HandleGameOver();
                }
            }
            else
            {
                if (ChessGame.IsKingInCheck("black") == false)
                {
                    Debug.Log("stalemate");
                    ChessGame.Instance.HandleGameWon();
                }
                else
                {
                    Debug.Log("white wins");
                    ChessGame.Instance.HandleGameWon();
                }
            }
            return;
        }
        if (turn == "white")
            return;

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

        Debug.Log($"did black put white in check ? {ChessGame.IsKingInCheck("white")}");

        UpdateCheckText();

        StartCoroutine(HandleStockfishTurn("white"));
    }

    private Vector2Int UciToBoard(string uci)
    {
        return new Vector2Int(uci[0] - 'a', uci[1] - '1');
    }
}

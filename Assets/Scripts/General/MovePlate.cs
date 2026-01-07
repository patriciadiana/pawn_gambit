using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MovePlate : MonoBehaviour
{
    public GameObject gameController;

    GameObject referencePiece = null;

    private int targetX;
    private int targetY;

    public bool isAttackMove = false;

    public void Start()
    {
        if(isAttackMove)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public void OnMouseDown()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController");
        ChessGame chessGame = gameController.GetComponent<ChessGame>();
        ChessPiece pieceScript = referencePiece.GetComponent<ChessPiece>();

        if (isAttackMove)
        {
            SoundManager.PlaySound(SoundType.ATTACKPIECE);
        }
        else
        {
            SoundManager.PlaySound(SoundType.MOVEPIECE);
        }

        if (isAttackMove)
        {
            GameObject targetPiece = gameController.GetComponent<ChessGame>().GetPosition(targetX, targetY);
            if(targetPiece != null)
            {
                ChessPiece targetPieceScript = targetPiece.GetComponent<ChessPiece>();

                if(chessGame.puzzleMode)
                {
                    if(targetPieceScript.name.Contains("white_queen"))
                    {
                        chessGame.SavePawnPosition(referencePiece);
                        //SceneManager.LoadScene("KingPuzzle");
                        SceneManager.LoadScene("QueenPuzzle");
                    }
                    else if (targetPieceScript.name.Contains("white_rook"))
                    {
                        chessGame.SavePawnPosition(referencePiece);
                        SceneManager.LoadScene("RookPuzzle");
                    }
                    else if (targetPieceScript.name.Contains("white_bishop"))
                    {
                        chessGame.SavePawnPosition(referencePiece);
                        SceneManager.LoadScene("BishopPuzzle");
                    }
                    else if (targetPieceScript.name.Contains("white_knight") &&
                             !targetPiece.CompareTag("EnemyKnight"))
                    {
                        chessGame.SavePawnPosition(referencePiece);
                        SceneManager.LoadScene("KnightPuzzle");
                    }
                    else if (targetPieceScript.name.Contains("white_king"))
                    {
                        chessGame.SavePawnPosition(referencePiece);
                        SceneManager.LoadScene("KingPuzzle");
                    }
                }
            }
            Destroy(targetPiece);
        }
        chessGame.SetPositionEmpty(pieceScript.xBoard, pieceScript.yBoard);

        pieceScript.SetXBoard(targetX);
        pieceScript.SetYBoard(targetY);
        pieceScript.SetCoords();

        chessGame.SetPosition(referencePiece);
        pieceScript.DestroyMovePlates();

        ChessPiece movedPiece = referencePiece.GetComponent<ChessPiece>();
        if (movedPiece.name.Equals("white_pawn") && chessGame.isMidgameMode == false)
        {
            chessGame.TransitionToPuzzleMode(movedPiece.gameObject);
        }
        else if(chessGame.isMidgameMode == false)
        {
            chessGame.SwitchTurn();
        }
    }

    public void SetCoords(int x, int y)
    {
        targetX = x;
        targetY = y;
    }

    public void SetReference(GameObject obj)
    {
        referencePiece = obj;
    }

    public GameObject GetReference()
    {
        return referencePiece;
    }
}

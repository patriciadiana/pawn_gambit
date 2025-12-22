using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ChessPiece : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] public GameObject movePlatePrefab;
    public bool ignorePuzzleRestriction = false;

    public GameObject controller;

    [Header("Sprites")]
    [SerializeField] public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    [SerializeField] public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    public int xBoard { get; private set; }
    public int yBoard { get; private set; }
    public string player { get; private set; }

    public Vector2Int lastPosition;

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetCoords();
        AssignSprite(this.name);
    }

    private void AssignSprite(string name)
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        switch (name)
        {
            case "black_queen": renderer.sprite = black_queen; player = "black"; break;
            case "black_knight": renderer.sprite = black_knight; player = "black"; break;
            case "black_bishop": renderer.sprite = black_bishop; player = "black"; break;
            case "black_king": renderer.sprite = black_king; player = "black"; break;
            case "black_rook": renderer.sprite = black_rook; player = "black"; break;
            case "black_pawn": renderer.sprite = black_pawn; player = "black"; break;

            case "white_queen": renderer.sprite = white_queen; player = "white"; break;
            case "white_knight": renderer.sprite = white_knight; player = "white"; break;
            case "white_bishop": renderer.sprite = white_bishop; player = "white"; break;
            case "white_king": renderer.sprite = white_king; player = "white"; break;
            case "white_rook": renderer.sprite = white_rook; player = "white"; break;
            case "white_pawn": renderer.sprite = white_pawn; player = "white"; break;
        }
    }

    public void SetCoords()
    {
        Vector2Int gridPos = new Vector2Int(xBoard, yBoard);
        Vector3 worldPos = BoardConfig.GridToWorld(gridPos);
        transform.position = new Vector3(worldPos.x, worldPos.y, -1.0f);
    }

    private void OnMouseDown()
    {
        GameObject controller = GameObject.FindGameObjectWithTag("GameController");
        ChessGame chessGameScript = controller.GetComponent<ChessGame>();

        if (player != chessGameScript.GetCurrentPlayer() && !ignorePuzzleRestriction)
            return;

        DestroyMovePlates();
        GenerateMovePlates();
    }

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]);
        }
    }

    public void GenerateMovePlates()
    {
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;

            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;

            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;

            case "black_king":
            case "white_king":
                SurroundMovePlate();
                break;

            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                break;

            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;

            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
                break;
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        ChessGame chessGameScript = controller.GetComponent<ChessGame>();
        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (chessGameScript.IsPositionOnBoard(x, y) && chessGameScript.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (chessGameScript.IsPositionOnBoard(x, y) &&
            chessGameScript.GetPosition(x, y).GetComponent<ChessPiece>().player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 0);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard + 0);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    public void PointMovePlate(int x, int y)
    {
        ChessGame chessGameScript = controller.GetComponent<ChessGame>();
        if (chessGameScript.IsPositionOnBoard(x, y))
        {
            GameObject chessPiecePosition = chessGameScript.GetPosition(x, y);

            if (chessPiecePosition == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (chessPiecePosition.GetComponent<ChessPiece>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    public void PawnMovePlate(int x, int y)
    {
        ChessGame chessGameScript = controller.GetComponent<ChessGame>();
        bool puzzleMode = chessGameScript.puzzleMode;

        if (puzzleMode)
        {
            // Left movement
            if (chessGameScript.IsPositionOnBoard(xBoard - 1, yBoard) &&
                chessGameScript.GetPosition(xBoard - 1, yBoard) == null)
                MovePlateSpawn(xBoard - 1, yBoard);

            // Right movement
            if (chessGameScript.IsPositionOnBoard(xBoard + 1, yBoard) &&
                chessGameScript.GetPosition(xBoard + 1, yBoard) == null)
                MovePlateSpawn(xBoard + 1, yBoard);

            // Upward movement
            if (chessGameScript.IsPositionOnBoard(xBoard, yBoard + 1) &&
                chessGameScript.GetPosition(xBoard, yBoard + 1) == null)
                MovePlateSpawn(xBoard, yBoard + 1);

            // Downward movement
            if (chessGameScript.IsPositionOnBoard(xBoard, yBoard - 1) &&
                chessGameScript.GetPosition(xBoard, yBoard - 1) == null)
                MovePlateSpawn(xBoard, yBoard - 1);

            // Attack diagonals
            TryPawnAttack(xBoard + 1, yBoard + 1, chessGameScript);
            TryPawnAttack(xBoard - 1, yBoard + 1, chessGameScript);
            TryPawnAttack(xBoard + 1, yBoard - 1, chessGameScript);
            TryPawnAttack(xBoard - 1, yBoard - 1, chessGameScript);
        }
        else
        {
            if (chessGameScript.IsPositionOnBoard(x, y))
            {
                if (chessGameScript.GetPosition(x, y) == null)
                {
                    MovePlateSpawn(x, y);

                    if (player == "white" && yBoard == 1 &&
                        chessGameScript.GetPosition(x, y + 1) == null)
                        MovePlateSpawn(x, y + 1);
                    else if (player == "black" && yBoard == 6 &&
                             chessGameScript.GetPosition(x, y - 1) == null)
                        MovePlateSpawn(x, y - 1);
                }

                TryPawnAttack(x + 1, y, chessGameScript);
                TryPawnAttack(x - 1, y, chessGameScript);
            }
        }
    }

    private void TryPawnAttack(int x, int y, ChessGame game)
    {
        if (!game.IsPositionOnBoard(x, y)) return;

        GameObject target = game.GetPosition(x, y);
        if (target == null) return;

        ChessPiece targetPiece = target.GetComponent<ChessPiece>();

        // Pawn attack rule
        if (!game.isMidgameMode || targetPiece.player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    public void UpdateLastPosition()
    {
        lastPosition = new Vector2Int(xBoard, yBoard);
    }

    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        Vector3 worldPos = BoardConfig.GridToWorld(new Vector2Int(matrixX, matrixY));
        GameObject movePlate = Instantiate(movePlatePrefab, new Vector3(worldPos.x, worldPos.y, -3.0f), Quaternion.identity);

        MovePlate movePlateScript = movePlate.GetComponent<MovePlate>();
        movePlateScript.SetReference(gameObject);
        movePlateScript.SetCoords(matrixX, matrixY);
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        Vector3 worldPos = BoardConfig.GridToWorld(new Vector2Int(matrixX, matrixY));
        GameObject movePlate = Instantiate(movePlatePrefab, new Vector3(worldPos.x, worldPos.y, -3.0f), Quaternion.identity);

        MovePlate movePlateScript = movePlate.GetComponent<MovePlate>();
        movePlateScript.isAttackMove = true;
        movePlateScript.SetReference(gameObject);
        movePlateScript.SetCoords(matrixX, matrixY);
    }

    public string GetPlayer()
    {
        return player;
    }

    public int GetPieceType()
    {
        string pieceName = this.name.ToLower();
        if (pieceName.Contains("pawn")) return 0;
        if (pieceName.Contains("knight")) return 1;
        if (pieceName.Contains("bishop")) return 2;
        if (pieceName.Contains("rook")) return 3;
        if (pieceName.Contains("queen")) return 4;
        if (pieceName.Contains("king")) return 5;
        return -1;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }

    public void SetPlayer(string newPlayer)
    {
        player = newPlayer;
    }
}

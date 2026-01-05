using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChessGame : MonoBehaviour
{
    public delegate void MoveCompleted(ChessPiece piece, Vector2Int from, Vector2Int to);
    public event MoveCompleted OnMoveCompleted;
    public bool suppressCallback = false;

    [Header("Prefabs")]
    [SerializeField] public GameObject chessPiecePrefab;

    [Header("Game Over")]
    [SerializeField] private GameOverScreen gameOver;

    private GameObject[,] boardPositions = new GameObject[8, 8];
    private GameObject[] blackPieces = new GameObject[16];
    private GameObject[] whitePieces = new GameObject[16];
    private static List<GameObject> white_pieces = new List<GameObject>();
    private static List<GameObject> black_pieces = new List<GameObject>();

    public static ChessGame Instance;

    private bool hasTransitionedToPuzzle = false;
    public bool puzzleMode = false;
    public bool isMidgameMode = false;

    private string currentPlayer = "white";

    private bool shouldRestorePawnAfterLoad = false;
    private string nextPieceToSpawn = null;
    private Vector2Int nextPiecePosition;
    private Vector2Int savedPawnPosition;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "ChessGame")
            return;

        SoundManager.Instance.ResumeMusic();

        if (PuzzleManager.Instance != null &&
        PuzzleManager.Instance.kingPuzzleCompleted)
        {
            PuzzleManager.Instance.kingPuzzleCompleted = false;

            ChessGame.Instance.ResetForMidgame();
            MidgameGenerator.Instance.GenerateMidgame();

            string stockfishPath = Application.dataPath + "/StreamingAssets/stockfish/stockfish-windows-x86-64-avx2.exe";
            StockfishManager.Instance.StartEngine(stockfishPath);
        }


        if (shouldRestorePawnAfterLoad)
        {
            RestorePawn();

            if (!string.IsNullOrEmpty(nextPieceToSpawn))
            {
                StartCoroutine(
                    SpawnPieceAfterTime(
                        nextPieceToSpawn,
                        nextPiecePosition.x,
                        nextPiecePosition.y));
            }

            shouldRestorePawnAfterLoad = false;
            nextPieceToSpawn = null;
        }
    }

    public void QueuePawnRestore(string nextPieceName = null, Vector2Int? nextPos = null)
    {
        shouldRestorePawnAfterLoad = true;

        if (!string.IsNullOrEmpty(nextPieceName) && nextPos.HasValue)
        {
            nextPieceToSpawn = nextPieceName;
            nextPiecePosition = nextPos.Value;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SpawnPieces();
    }
    public void SwitchTurn()
    {
        if (puzzleMode)
        {
            return;
        }

        string oldTurn = currentPlayer;
        currentPlayer = (currentPlayer == "white") ? "black" : "white";
    }
    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void ResetForMidgame()
    {
        puzzleMode = false;
        isMidgameMode = true;
        suppressCallback = true;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                boardPositions[x, y] = null;

        suppressCallback = false;
    }

    public void SpawnPieces()
    {
        whitePieces = new GameObject[]
        {
            CreatePiece("white_rook",0,0), CreatePiece("white_knight",1,0), CreatePiece("white_bishop",2,0), CreatePiece("white_queen",3,0),
            CreatePiece("white_king",4,0), CreatePiece("white_bishop",5,0), CreatePiece("white_knight",6,0), CreatePiece("white_rook",7,0),
            CreatePiece("white_pawn",0,1), CreatePiece("white_pawn",1,1), CreatePiece("white_pawn",2,1), CreatePiece("white_pawn",3,1),
            CreatePiece("white_pawn",4,1), CreatePiece("white_pawn",5,1), CreatePiece("white_pawn",6,1), CreatePiece("white_pawn",7,1),
        };

        blackPieces = new GameObject[]
        {
            CreatePiece("black_rook",0,7), CreatePiece("black_knight",1,7), CreatePiece("black_bishop",2,7), CreatePiece("black_queen",3,7),
            CreatePiece("black_king",4,7), CreatePiece("black_bishop",5,7), CreatePiece("black_knight",6,7), CreatePiece("black_rook",7,7),
            CreatePiece("black_pawn",0,6), CreatePiece("black_pawn",1,6), CreatePiece("black_pawn",2,6), CreatePiece("black_pawn",3,6),
            CreatePiece("black_pawn",4,6), CreatePiece("black_pawn",5,6), CreatePiece("black_pawn",6,6), CreatePiece("black_pawn",7,6),
        };

        for (int i = 0; i < blackPieces.Length; i++)
        {
            SetPosition(blackPieces[i]);
            SetPosition(whitePieces[i]);
        }
    }

    public static string GenerateFEN()
    {
        StringBuilder sb = new StringBuilder();

        bool whiteKing = false;
        bool blackKing = false;
        bool whiteRookA = false, whiteRookH = false;
        bool blackRookA = false, blackRookH = false;

        // 1. Piece placement
        for (int y = 7; y >= 0; y--)
        {
            int emptyCount = 0;

            for (int x = 0; x < 8; x++)
            {
                GameObject piece = ChessGame.Instance.GetPosition(x, y);

                if (piece == null)
                {
                    emptyCount++;
                    continue;
                }

                if (emptyCount > 0)
                {
                    sb.Append(emptyCount);
                    emptyCount = 0;
                }

                string name = piece.name.ToLower();
                char fenChar;

                if (name.Contains("pawn")) fenChar = 'p';
                else if (name.Contains("rook")) fenChar = 'r';
                else if (name.Contains("knight")) fenChar = 'n';
                else if (name.Contains("bishop")) fenChar = 'b';
                else if (name.Contains("queen")) fenChar = 'q';
                else if (name.Contains("king")) fenChar = 'k';
                else continue;

                bool isWhite = name.StartsWith("white");
                if (isWhite) fenChar = char.ToUpper(fenChar);

                // Track kings
                if (fenChar == 'K') whiteKing = true;
                if (fenChar == 'k') blackKing = true;

                // Track rooks for castling
                if (fenChar == 'R' && y == 0 && x == 0) whiteRookA = true;
                if (fenChar == 'R' && y == 0 && x == 7) whiteRookH = true;
                if (fenChar == 'r' && y == 7 && x == 0) blackRookA = true;
                if (fenChar == 'r' && y == 7 && x == 7) blackRookH = true;

                sb.Append(fenChar);
            }

            if (emptyCount > 0)
                sb.Append(emptyCount);

            if (y > 0)
                sb.Append('/');
        }

        // Sanity check (VERY IMPORTANT)
        if (!whiteKing || !blackKing)
        {
            Debug.LogError("Invalid FEN: missing king!");
        }

        // 2. Active color
        sb.Append(' ');
        sb.Append(ChessGame.Instance.GetCurrentPlayer() == "white" ? 'w' : 'b');

        // 3. Castling rights
        sb.Append(' ');
        string castling = "";

        if (whiteRookH && whiteKing) castling += "K";
        if (whiteRookA && whiteKing) castling += "Q";
        if (blackRookH && blackKing) castling += "k";
        if (blackRookA && blackKing) castling += "q";

        sb.Append(castling == "" ? "-" : castling);

        // 4. En passant
        sb.Append(" -");

        // 5. Halfmove clock
        sb.Append(" 0");

        // 6. Fullmove number
        sb.Append(" 1");

        return sb.ToString();
    }


    public GameObject CreatePiece(string pieceName, int x, int y)
    {
        Vector3 spawnPosition = BoardConfig.GridToWorld(new Vector2Int(x, y));
        spawnPosition.z = -1;

        GameObject newPiece = Instantiate(chessPiecePrefab, spawnPosition, Quaternion.identity);
        ChessPiece pieceScript = newPiece.GetComponent<ChessPiece>();
        pieceScript.name = pieceName;
        pieceScript.SetXBoard(x);
        pieceScript.SetYBoard(y);
        pieceScript.Activate();
        return newPiece;
    }

    public void SetPosition(GameObject piece)
    {
        ChessPiece pieceScript = piece.GetComponent<ChessPiece>();

        Vector2Int from = pieceScript.lastPosition;
        Vector2Int to = new Vector2Int(pieceScript.xBoard, pieceScript.yBoard);

        boardPositions[pieceScript.xBoard, pieceScript.yBoard] = piece;

        // Don't trigger callback during spawn
        if (suppressCallback)
            return;

        // Only trigger callback if actual movement happened
        if (from != to && OnMoveCompleted != null)
        {
            OnMoveCompleted(pieceScript, from, to);
        }

        // Update lastPosition AFTER placing
        pieceScript.lastPosition = to;
    }


    public void SetPosition(GameObject piece, int x, int y)
    {
        boardPositions[x, y] = piece;
    }

    public void SetPositionEmpty(int x, int y)
    {
        GameObject pieceObj = GetPosition(x, y);

        if (pieceObj != null)
        {
            ChessPiece cp = pieceObj.GetComponent<ChessPiece>();
            if (cp != null)
            {
                cp.lastPosition = new Vector2Int(x, y);
            }
        }

        boardPositions[x, y] = null;
    }
    public GameObject GetPosition(int x, int y)
    {
        return boardPositions[x, y];
    }

    public bool IsPositionOnBoard(int x, int y)
    {
        return BoardConfig.IsInsideBoard(new Vector2Int(x, y));
    }

    public void TransitionToPuzzleMode(GameObject pawn)
    {
        if (hasTransitionedToPuzzle) return;

        hasTransitionedToPuzzle = true;
        puzzleMode = true;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                boardPositions[x, y] = null;
            }
        }

        foreach (GameObject piece in whitePieces)
        {
            if (piece != pawn && piece != null)
            {
                Destroy(piece);
            }
        }

        foreach (GameObject piece in blackPieces)
        {
            if (piece != null)
            {
                Destroy(piece);
            }
        }

        SpawnRandomAIKnights(3, pawn);

        StartCoroutine(SpawnPieceAfterTime("white_queen", 0, 7));
    }

    public void RestorePawn()
    {
        GameObject restoredPawn = CreatePiece("white_pawn", savedPawnPosition.x, savedPawnPosition.y);

        SpawnRandomAIKnights(3, restoredPawn);

        SetPosition(restoredPawn);
    }

    public void SavePawnPosition(GameObject pawn)
    {
        ChessPiece pawnScript = pawn.GetComponent<ChessPiece>();
        savedPawnPosition = new Vector2Int(pawnScript.xBoard, pawnScript.yBoard);
    }

    public GameObject SpawnChessPiece(string pieceName, int x, int y)
    {
        Vector3 spawnPosition = BoardConfig.GridToWorld(new Vector2Int(x, y));
        spawnPosition.z = -1;

        GameObject newPiece = Instantiate(chessPiecePrefab, spawnPosition, Quaternion.identity);
        ChessPiece pieceScript = newPiece.GetComponent<ChessPiece>();
        pieceScript.name = pieceName;
        pieceScript.SetXBoard(x);
        pieceScript.SetYBoard(y);
        pieceScript.Activate();

        SetPosition(newPiece);

        if(pieceName.StartsWith("white"))
            white_pieces.Add(newPiece);
        else
            black_pieces.Add(newPiece);

        return newPiece;
    }

    public GameObject SpawnAIKnight(int x, int y, GameObject pawnTarget)
    {
        GameObject knight = SpawnChessPiece("black_knight", x, y);
        KnightAI ai = knight.AddComponent<KnightAI>();
        ai.targetPawn = pawnTarget;
        knight.tag = "EnemyKnight";
        return knight;
    }

    public void HandleGameOver()
    {
        gameOver.Setup();
    }

    public void SpawnRandomAIKnights(int count, GameObject pawnTarget)
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = 300;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            int x = Random.Range(0, BoardConfig.BoardWidth);
            int y = Random.Range(0, BoardConfig.BoardHeight);

            if (ChessGame.Instance.GetPosition(x, y) == null)
            {
                SpawnAIKnight(x, y, pawnTarget);
                spawned++;
            }
        }
    }

    private IEnumerator SpawnPieceAfterTime(string name, int x, int y)
    {
        yield return new WaitForSeconds(2f);

        SpawnChessPiece(name, x, y);
    }

    public static bool IsKingInCheck(string piece_color)
    {
        GameObject king;
        ChessPiece kingPiece = null;
        List<GameObject> pieces = new List<GameObject>();
        List<GameObject> enemy_pieces = new List<GameObject>();
        List<GameObject> all_pieces = white_pieces;
        all_pieces.AddRange(black_pieces);

        string enemy_color;

        if (piece_color == "white")
        {
            pieces = white_pieces;
            enemy_pieces = black_pieces;
            enemy_color = "black";
        }
        else
        {
            pieces = black_pieces;
            enemy_pieces = white_pieces;
            enemy_color = "white";
        }

        foreach (GameObject piece in pieces)
        {
            if(piece!=null)
            {
                ChessPiece pieceScript = piece.GetComponent<ChessPiece>();
                if (pieceScript.name == piece_color + "_king")
                {
                    king = piece;
                    kingPiece = king.GetComponent<ChessPiece>();
                    break;
                }

            }
        }

        foreach (GameObject piece in enemy_pieces)
        {
            if(piece!=null)
            {
                ChessPiece pieceScript = piece.GetComponent<ChessPiece>();
                // is there a check on same row/column
                if (pieceScript.name == enemy_color + "_rook" || pieceScript.name == enemy_color + "_queen")
                {
                    if (pieceScript.xBoard == kingPiece.xBoard) //possible check on same row
                    {
                        if (IsThereAPieceBetweenTwoPieces(kingPiece, pieceScript, all_pieces, "row") == false)
                            return true;
                    }
                    if (pieceScript.yBoard == kingPiece.yBoard) //possible check on same column
                    {
                        if (IsThereAPieceBetweenTwoPieces(kingPiece, pieceScript, all_pieces, "column") == false)
                            return true;
                    }
                }
                // is there a check on same diagonal
                if (pieceScript.name == enemy_color + "_bishop" || pieceScript.name == enemy_color + "_queen")
                {
                    //if (pos[0] - pos[1] == queen[0] - queen[1] || pos[0] + pos[1] == queen[0] + queen[1])
                    if (pieceScript.xBoard - pieceScript.yBoard == kingPiece.xBoard - kingPiece.yBoard) //possible check on left leaning diagonal
                    {
                        if (IsThereAPieceBetweenTwoPieces(kingPiece, pieceScript, all_pieces, "right_diagonal") == false)
                            return true;
                    }
                    if (pieceScript.xBoard + pieceScript.yBoard == kingPiece.xBoard + kingPiece.yBoard) //possible check on right leaning diagonal
                    {
                        if (IsThereAPieceBetweenTwoPieces(kingPiece, pieceScript, all_pieces, "left_diagonal") == false)
                            return true;
                    }
                }
                if (pieceScript.name == enemy_color + "_knight")
                {
                    int[] x = { 1, 2, 2, 1, -1, -2, -2, -1 };
                    int[] y = { -2, -1, 1, 2, 2, 1, -1, -2 };

                    for (int i = 0; i < 8; i++)
                    {
                        int newx = kingPiece.xBoard + x[i];
                        int newy = kingPiece.yBoard + y[i];
                        if (0 <= newx && newx < 8 && 0 <= newy && newy < 8)
                        {
                            if (pieceScript.xBoard == newx && pieceScript.yBoard == newy)
                                return true;
                        }
                    }
                }
                if (pieceScript.name == enemy_color + "_pawn")
                {
                    if(piece_color == "white")
                    {
                        if(kingPiece.yBoard <7)
                        {
                            if ((kingPiece.yBoard + 1 == pieceScript.yBoard) &&
                               (kingPiece.xBoard == pieceScript.xBoard + 1 || kingPiece.xBoard == pieceScript.xBoard - 1))
                                return true;
                        }
                    }
                    else
                    {
                        if (kingPiece.yBoard > 2)
                        {
                            if ((kingPiece.yBoard - 1 == pieceScript.yBoard) &&
                               (kingPiece.xBoard == pieceScript.xBoard + 1 || kingPiece.xBoard == pieceScript.xBoard - 1))
                                return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    public static bool IsThereAPieceBetweenTwoPieces(ChessPiece piece1, ChessPiece piece2, List<GameObject> allPieces, string comparison)
    {
        foreach (GameObject piece in allPieces)
        {
            if (piece != null)
            {
                ChessPiece pieceScript = piece.GetComponent<ChessPiece>();
                if (comparison == "row")
                {
                    if (pieceScript.xBoard == piece1.xBoard &&
                        ((piece1.yBoard < pieceScript.yBoard && pieceScript.yBoard < piece2.yBoard) ||
                        (piece2.yBoard < pieceScript.yBoard && pieceScript.yBoard < piece1.yBoard)))
                        return true;
                }
                if (comparison == "column")
                {
                    if (pieceScript.yBoard == piece1.yBoard &&
                        ((piece1.xBoard < pieceScript.xBoard && pieceScript.xBoard < piece2.xBoard) ||
                        (piece2.xBoard < pieceScript.xBoard && pieceScript.xBoard < piece1.xBoard)))
                        return true;
                }
                if(comparison == "right_diagonal")
                {
                    if(pieceScript.xBoard - pieceScript.yBoard == piece1.xBoard - piece1.yBoard)
                    {
                        if((piece1.xBoard < pieceScript.xBoard && pieceScript.xBoard < piece2.xBoard &&
                            piece1.yBoard < pieceScript.yBoard && pieceScript.yBoard < piece2.yBoard) ||
                        (piece2.xBoard < pieceScript.xBoard && pieceScript.xBoard < piece1.xBoard &&
                            piece2.yBoard < pieceScript.yBoard && pieceScript.yBoard < piece1.yBoard))
                            return true;
                    }
                }
                if (comparison == "left_diagonal")
                {
                    if (pieceScript.xBoard + pieceScript.yBoard == piece1.xBoard + piece1.yBoard)
                    {
                        if ((piece1.xBoard < pieceScript.xBoard && pieceScript.xBoard < piece2.xBoard &&
                            piece1.yBoard > pieceScript.yBoard && pieceScript.yBoard > piece2.yBoard) ||
                        (piece2.xBoard < pieceScript.xBoard && pieceScript.xBoard < piece1.xBoard &&
                            piece2.yBoard > pieceScript.yBoard && pieceScript.yBoard > piece1.yBoard))
                            return true;
                    }
                }
            }
        }
        return false;
    }
}

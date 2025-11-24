using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChessGame : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] public GameObject chessPiecePrefab;

    private GameObject[,] boardPositions = new GameObject[8, 8];
    private GameObject[] blackPieces = new GameObject[16];
    private GameObject[] whitePieces = new GameObject[16];

    public static ChessGame Instance;

    private bool hasTransitionedToPuzzle = false;

    private string currentPlayer = "white";
    public bool puzzleMode = false;

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
        if (shouldRestorePawnAfterLoad && scene.name == "ChessGame")
        {
            RestorePawn();

            if (!string.IsNullOrEmpty(nextPieceToSpawn))
            {
                StartCoroutine(SpawnPieceAfterTime(nextPieceToSpawn, nextPiecePosition.x, nextPiecePosition.y));
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
        currentPlayer = (currentPlayer == "white") ? "black" : "white";
    }
    public string GetCurrentPlayer()
    {
        return currentPlayer;
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

        boardPositions[pieceScript.xBoard, pieceScript.yBoard] = piece;
    }

    public void SetPosition(GameObject piece, int x, int y)
    {
        boardPositions[x, y] = piece;
    }

    public void SetPositionEmpty(int x, int y)
    {
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

        StartCoroutine(SpawnPieceAfterTime("black_queen", 0, 7));
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
}

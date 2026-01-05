using UnityEngine;
using System.Collections.Generic;

public class KingPuzzles : MonoBehaviour
{
    public PuzzleConfig puzzleConfig;
    public bool puzzleWon = false;
    private bool puzzleCompleted = false;

    private float timer;
    private bool firstPuzzleFinished = false;
    private List<GameObject> spawnedPieces = new List<GameObject>();

    private Vector2Int firstPuzzleTarget = new Vector2Int(7, 5);
    private Vector2Int secondPuzzleTarget = new Vector2Int(5, 7);

    public void MarkWin()
    {
        puzzleWon = true;
    }

    void Start()
    {
        SoundManager.Instance.PauseMusic();
        SoundManager.PlayMusic(MusicType.KINGTHEME, 0.2f);

        ChessGame.Instance.OnMoveCompleted += OnPieceMoved;
        ChessGame.Instance.puzzleMode = true;

        if (puzzleConfig.hasTimeLimit)
        {
            timer = puzzleConfig.timeLimitinSeconds;
        }

        if (firstPuzzleFinished == false)
        {
            SpawnFirstPuzzle();
        }
    }

    void Update()
    {
        if (puzzleWon && !puzzleCompleted)
        {
            puzzleCompleted = true;

            PuzzleManager.Instance.kingPuzzleCompleted = true;

            PuzzlePiecesDisplay.Instance.UnlockPiece("king");

            PuzzleManager.Instance.CompletePuzzle(
                puzzleConfig.initialSceneName,
                puzzleConfig.nextPieceType,
                puzzleConfig.nextPiecePosition,
                puzzleConfig.rewardType
            );

            return;
        }

        if (puzzleConfig.hasTimeLimit && !puzzleWon)
        {
            timer -= Time.deltaTime;
            TimerUI.Instance.DisplayTime(timer);

            if (timer <= 0f)
            {
                TimerUI.Instance.Hide();

                PuzzleManager.Instance.FailPuzzle(
                    puzzleConfig.puzzleName,
                    puzzleConfig.nextPieceType,
                    puzzleConfig.nextPiecePosition
                );

                ClearSpawnedPieces();
                if (ChessGame.Instance != null)
                    ChessGame.Instance.OnMoveCompleted -= OnPieceMoved;
            }
        }
    }

    void SpawnFirstPuzzle()
    {
        spawnedPieces.Clear();

        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_rook", 3, 7));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_bishop", 4, 2));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_bishop", 4, 3));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_pawn", 0, 2));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_pawn", 1, 1));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_pawn", 5, 1));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_pawn", 6, 2));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_king", 6, 1));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_rook", 7, 0));

        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_pawn", 7, 6));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_pawn", 5, 6));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_pawn", 6, 5));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_king", 6, 6));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_queen", 4, 4));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_rook", 5, 5));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_rook", 2, 2));
    }

    void SpawnSecondPuzzle()
    {
        ClearSpawnedPieces();

        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_queen", 1, 7));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_pawn", 2, 3));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_pawn", 5, 3));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_pawn", 6, 2));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_king", 7, 2));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("white_knight", 4, 4));

        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_pawn", 0, 4));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_pawn", 1, 5));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_pawn", 4, 5));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_pawn", 5, 4));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_queen", 0, 3));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_bishop", 3, 3));
        spawnedPieces.Add(ChessGame.Instance.SpawnChessPiece("black_king", 2, 4));
    }

    void ClearSpawnedPieces()
    {
        foreach (GameObject piece in spawnedPieces)
        {
            if (piece != null)
            {
                ChessPiece cp = piece.GetComponent<ChessPiece>();
                if (cp != null)
                {
                    ChessGame.Instance.SetPositionEmpty(cp.xBoard, cp.yBoard);
                }

                Destroy(piece);
            }
        }

        spawnedPieces.Clear();
    }

    private void OnPieceMoved(ChessPiece piece, Vector2Int from, Vector2Int to)
    {
        if (from == Vector2Int.zero) return;

        if (piece.name.Contains("white_bishop") && to == firstPuzzleTarget)
        {
            firstPuzzleFinished = true;
            SpawnSecondPuzzle();
        }

        else if (piece.name.Contains("white_queen") && to == secondPuzzleTarget)
        {
            MarkWin();
        }
        else
        {
            ChessGame.Instance.suppressCallback = true;
            ChessGame.Instance.SetPositionEmpty(to.x, to.y);
            piece.SetXBoard(from.x);
            piece.SetYBoard(from.y);
            piece.SetCoords();
            ChessGame.Instance.SetPosition(piece.gameObject);
            ChessGame.Instance.suppressCallback = false;
        }
    }

    void OnDestroy()
    {
        if (ChessGame.Instance != null)
            ChessGame.Instance.OnMoveCompleted -= OnPieceMoved;
    }
}
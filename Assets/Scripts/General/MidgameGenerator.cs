using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MidgameGenerator : MonoBehaviour
{
    public static MidgameGenerator Instance;
    private List<Vector2Int> white_pieces = new List<Vector2Int>();
    private List<Vector2Int> black_pieces = new List<Vector2Int>();

    private Coroutine timerRoutine;
    [SerializeField] private MidgameUIDisplay imageDisplay;
    [SerializeField] private float timeLimit = 300f;

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

    public void GenerateMidgame()
    {
        if (imageDisplay == null)
        {
            GameObject midgameUI = GameObject.FindWithTag("MidgameUIGenerator");
            imageDisplay = midgameUI.GetComponent<MidgameUIDisplay>();
        }
        imageDisplay.Play();

        ClearBoard();

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        Vector2Int whiteKing = SpawnWhiteSide(occupied);
        SpawnBlackSide(occupied, whiteKing);

        ChessGame.Instance.puzzleMode = false;
        ChessGame.Instance.suppressCallback = false;

        CheckKingAndStartTimer();
    }

    void CheckKingAndStartTimer()
    {
        if (PuzzleManager.Instance == null) return;

        bool hasKing = PuzzleManager.Instance.rewardPieces
            .Exists(r => r.Equals("king", StringComparison.OrdinalIgnoreCase));

        if (!hasKing)
        {
            if (timerRoutine != null)
                StopCoroutine(timerRoutine);

            timerRoutine = StartCoroutine(NoKingTimer());
        }
        else
        {
            TimerUI.Instance?.Hide();
        }
    }

    IEnumerator NoKingTimer()
    {
        float timeLeft = timeLimit;

        while (timeLeft > 0f)
        {
            TimerUI.Instance?.DisplayTime(timeLeft);
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        TimerUI.Instance?.DisplayTime(0f);

        ChessGame.Instance.HandleGameOver();
    }


    void ClearBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = ChessGame.Instance.GetPosition(x, y);
                if (piece != null)
                    Destroy(piece);

                ChessGame.Instance.SetPositionEmpty(x, y);
            }
        }
    }

    Vector2Int SpawnWhiteSide(HashSet<Vector2Int> occupied)
    {
        Vector2Int kingPos = GetRandomEmpty(occupied, (x, y) => y == 0);
        SpawnPiece("white_king", kingPos, occupied);

        int pawnCount = Random.Range(3, 6);
        int count = Random.Range(1, 3);

        for (int i = 0; i < pawnCount; i++)
        {
            Vector2Int pos = GetRandomEmpty(occupied, (x, y) => IsValidPawnSquare(y));
            SpawnPiece("white_pawn", pos, occupied);
        }

        if (PuzzleManager.Instance != null && PuzzleManager.Instance.rewardPieces != null)
        {
            foreach (string reward in PuzzleManager.Instance.rewardPieces)
            {
                if (string.IsNullOrEmpty(reward)) continue;

                if (reward.Equals("queen", StringComparison.OrdinalIgnoreCase))
                {
                    Vector2Int pos = GetRandomEmpty(
                        occupied,
                        (x, y) => y <= 3 && !IsPawnRank(y)
                    );

                    SpawnPiece("white_queen", pos, occupied);
                    continue;
                }
                else if (!reward.Equals("king", StringComparison.OrdinalIgnoreCase))
                {
                    for (int i = 0; i < count; i++)
                    {
                        Vector2Int pos = GetRandomEmpty(
                            occupied,
                            (x, y) => y <= 3 && !IsPawnRank(y)
                        );

                        SpawnPiece($"white_{reward}", pos, occupied);
                    }
                }
            }
        }

        return kingPos;
    }


    Vector2Int SpawnBlackSide(HashSet<Vector2Int> occupied, Vector2Int whiteKing)
    {
        Vector2Int kingPos;
        int attempts = 0;
        do
        {
            kingPos = GetRandomEmpty(occupied, (x, y) => y == 7);
            attempts++;
            if (attempts > 100)
            {
                Debug.Log("Failed to spawn black king in a safe position!");
                return kingPos;
            }
        }
        while (IsAdjacentToWhiteKing(kingPos, whiteKing) || IsSquareAttackedByWhite(kingPos));

        SpawnPiece("black_king", kingPos, occupied);

        int pawnCount = Random.Range(3, 6);
        for (int i = 0; i < pawnCount; i++)
        {
            Vector2Int pos = GetRandomEmpty(occupied, (x, y) => IsValidPawnSquare(y));
        }

        Dictionary<string, int> pieceLimits = new()
        {
            { "queen", 1 },
            { "rook", 2 },
            { "bishop", 2 },
            { "knight", 2 }
        };

        foreach (var pieceType in pieceLimits.Keys)
        {
            int maxCount = pieceLimits[pieceType];
            int spawnCount = Random.Range(0, maxCount + 1);
            for (int i = 0; i < spawnCount; i++)
            {
                Vector2Int pos = GetRandomEmpty(occupied, (x, y) => y >= 4);
                SpawnPiece($"black_{pieceType}", pos, occupied);
            }
        }
        return kingPos;
    }

    bool IsValidPawnSquare(int y) => y >= 1 && y <= 6;
    bool IsPawnRank(int y) => y == 1 || y == 6;

    bool IsAdjacentToWhiteKing(Vector2Int pos, Vector2Int whiteKing)
    {
        int dx = Mathf.Abs(pos.x - whiteKing.x);
        int dy = Mathf.Abs(pos.y - whiteKing.y);
        return dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0);
    }
    bool IsSquareAttackedByWhite(Vector2Int pos)
    {
        foreach (Vector2Int pawn in FindPieces("white_pawn"))
        {
            if (pos == pawn + new Vector2Int(1, 1) || pos == pawn + new Vector2Int(-1, 1))
                return true;
        }
        foreach (Vector2Int rook in FindPieces("white_rook"))
        {
            //check if on same row / column
            if (pos[0] == rook[0] || pos[1] == rook[1])
                return true;
        }
        foreach (Vector2Int queen in FindPieces("white_queen"))
        {
            //check if on same row / column
            if (pos[0] == queen[0] || pos[1] == queen[1])
                return true;
            //check if on same diagonal
            if (pos[0] - pos[1] == queen[0] - queen[1] || pos[0] + pos[1] == queen[0] + queen[1])
                return true;
        }
        return false;
    }

    void SpawnPiece(string pieceName, Vector2Int pos, HashSet<Vector2Int> occupied)
    {
        if (pos == Vector2Int.zero) return;

        GameObject piece = ChessGame.Instance.SpawnChessPiece(pieceName, pos.x, pos.y);
        if (piece != null)
            occupied.Add(pos);
        else
            Debug.Log($"Failed to spawn {pieceName} at {pos}");
    }

    Vector2Int GetRandomEmpty(HashSet<Vector2Int> occupied, Func<int, int, bool> validator = null)
    {
        List<Vector2Int> available = new();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector2Int pos = new(x, y);
                if (occupied.Contains(pos)) continue;
                if (validator != null && !validator(x, y)) continue;
                available.Add(pos);
            }
        }

        if (available.Count == 0) return Vector2Int.zero;
        return available[Random.Range(0, available.Count)];
    }

    List<Vector2Int> FindPieces(string pieceName)
    {
        List<Vector2Int> positions = new();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = ChessGame.Instance.GetPosition(x, y);
                if (piece != null && piece.name.ToLower().Contains(pieceName.ToLower()))
                    positions.Add(new Vector2Int(x, y));
            }
        }
        return positions;
    }
}

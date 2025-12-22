using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MidgameGenerator : MonoBehaviour
{
    public static MidgameGenerator Instance;

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
        ClearBoard();

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        Vector2Int whiteKing = SpawnWhiteSide(occupied);
        SpawnBlackSide(occupied, whiteKing);

        ChessGame.Instance.puzzleMode = false;
        ChessGame.Instance.suppressCallback = false;
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
    //Vector2Int SpawnWhiteSide(HashSet<Vector2Int> occupied)
    //{
    //    Vector2Int kingPos = GetRandomEmpty(occupied, (x, y) => y == 0);
    //    SpawnPiece("white_king", kingPos, occupied);

    //    int pawnCount = Random.Range(3, 6);
    //    int count = Random.Range(0, 3);

    //    for (int i = 0; i < pawnCount; i++)
    //    {
    //        Vector2Int pos = GetRandomEmpty(occupied, (x, y) => IsValidPawnSquare(y));
    //        SpawnPiece("white_pawn", pos, occupied);
    //    }

    //    if (PuzzleManager.Instance != null && PuzzleManager.Instance.rewardPieces != null)
    //    {
    //        foreach (string reward in PuzzleManager.Instance.rewardPieces)
    //        {
    //            if (string.IsNullOrEmpty(reward)) continue;

    //            if (reward.Equals("queen", StringComparison.OrdinalIgnoreCase))
    //            {
    //                Vector2Int pos = GetRandomEmpty(
    //                    occupied,
    //                    (x, y) => y <= 3 && !IsPawnRank(y)
    //                );

    //                SpawnPiece("white_queen", pos, occupied);
    //                continue;
    //            }
    //            // 0-2 copies
    //            else if (!reward.Equals("king", StringComparison.OrdinalIgnoreCase))
    //            {
    //                for (int i = 0; i < count; i++)
    //                {
    //                    Vector2Int pos = GetRandomEmpty(
    //                        occupied,
    //                        (x, y) => y <= 3 && !IsPawnRank(y)
    //                    );

    //                    SpawnPiece($"white_{reward}", pos, occupied);
    //                }
    //            }  
    //        }
    //    }

    //    return kingPos;
    //}

    Vector2Int SpawnWhiteSide(HashSet<Vector2Int> occupied)
    {
        Vector2Int kingPos = GetRandomEmpty(occupied, (x, y) => y == 0);
        SpawnPiece("white_king", kingPos, occupied);

        Vector2Int queenPos = GetRandomEmpty(occupied, (x, y) => y <= 3 && !IsPawnRank(y));
        SpawnPiece("white_queen", queenPos, occupied);

        for (int i = 0; i < 2; i++)
        {
            Vector2Int rookPos = GetRandomEmpty(occupied, (x, y) => y <= 3 && !IsPawnRank(y));
            SpawnPiece("white_rook", rookPos, occupied);
        }

        int pawnCount = Random.Range(3, 6);
        for (int i = 0; i < pawnCount; i++)
        {
            Vector2Int pos = GetRandomEmpty(occupied, (x, y) => IsValidPawnSquare(y));
            SpawnPiece("white_pawn", pos, occupied);
        }

        return kingPos;
    }


    void SpawnBlackSide(HashSet<Vector2Int> occupied, Vector2Int whiteKing)
    {
        Vector2Int kingPos;
        int attempts = 0;
        do
        {
            kingPos = GetRandomEmpty(occupied, (x, y) => y == 7);
            attempts++;
            if (attempts > 100)
            {
                Debug.LogError("Failed to spawn black king in a safe position!");
                return;
            }
        }
        while (IsAdjacentToWhiteKing(kingPos, whiteKing) || IsSquareAttackedByWhite(kingPos));

        SpawnPiece("black_king", kingPos, occupied);

        int pawnCount = Random.Range(3, 6);
        for (int i = 0; i < pawnCount; i++)
        {
            Vector2Int pos = GetRandomEmpty(occupied, (x, y) => IsValidPawnSquare(y));
            SpawnPiece("black_pawn", pos, occupied);
        }

        //Dictionary<string, int> pieceLimits = new()
        //{
        //    { "queen", 1 },
        //    { "rook", 2 },
        //    { "bishop", 2 },
        //    { "knight", 2 }
        //};

        Dictionary<string, int> pieceLimits = new()
        {
            { "queen", 1 },
            { "rook", 1 },
            { "bishop", 1 },
            { "knight", 1 }
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
        return false;
    }

    void SpawnPiece(string pieceName, Vector2Int pos, HashSet<Vector2Int> occupied)
    {
        if (pos == Vector2Int.zero) return;

        GameObject piece = ChessGame.Instance.SpawnChessPiece(pieceName, pos.x, pos.y);
        if (piece != null)
            occupied.Add(pos);
        else
            Debug.LogError($"Failed to spawn {pieceName} at {pos}");
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

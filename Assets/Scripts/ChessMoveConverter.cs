using System.Collections.Generic;
using UnityEngine;

public static class ChessMoveConverter
{
    // Convert algebraic notation (e.g., "e2e4") to board coordinates
    public static (Vector2Int from, Vector2Int to) ParseAlgebraicMove(string move)
    {
        if (string.IsNullOrEmpty(move) || move.Length < 4)
            return (Vector2Int.zero, Vector2Int.zero);

        // Algebraic notation: e2e4 = from file 'e', rank '2' to file 'e', rank '4'
        Dictionary<char, int> fileMap = new Dictionary<char, int>
        {
            {'a', 0}, {'b', 1}, {'c', 2}, {'d', 3},
            {'e', 4}, {'f', 5}, {'g', 6}, {'h', 7}
        };

        try
        {
            char fromFileChar = move[0];
            char fromRankChar = move[1];
            char toFileChar = move[2];
            char toRankChar = move[3];

            int fromX = fileMap[fromFileChar];
            int fromY = int.Parse(fromRankChar.ToString()) - 1; // Convert rank to 0-based

            int toX = fileMap[toFileChar];
            int toY = int.Parse(toRankChar.ToString()) - 1;

            return (new Vector2Int(fromX, fromY), new Vector2Int(toX, toY));
        }
        catch
        {
            return (Vector2Int.zero, Vector2Int.zero);
        }
    }

    // Convert board coordinates to algebraic notation
    public static string ConvertToAlgebraic(Vector2Int from, Vector2Int to)
    {
        Dictionary<int, char> reverseFileMap = new Dictionary<int, char>
        {
            {0, 'a'}, {1, 'b'}, {2, 'c'}, {3, 'd'},
            {4, 'e'}, {5, 'f'}, {6, 'g'}, {7, 'h'}
        };

        if (reverseFileMap.ContainsKey(from.x) && reverseFileMap.ContainsKey(to.x))
        {
            return $"{reverseFileMap[from.x]}{from.y + 1}{reverseFileMap[to.x]}{to.y + 1}";
        }

        return null;
    }
}
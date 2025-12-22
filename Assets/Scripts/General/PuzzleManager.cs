using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    public HashSet<string> solvedPuzzles = new();

    public List<string> rewardPieces = new();

    public bool kingPuzzleCompleted = false;

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
    public void CompletePuzzle(string name, string nextPiece, Vector2Int pos, string reward)
    {
        solvedPuzzles.Add(name);
        if(!string.IsNullOrEmpty(reward))
        {
            rewardPieces.Add(reward);
        }

        if(!string.IsNullOrEmpty(nextPiece) && kingPuzzleCompleted == false)
        {
            ChessGame.Instance.QueuePawnRestore(nextPiece, pos);
        }

        string rewardsList = string.Join(", ", rewardPieces);
        Debug.Log($"Puzzle Completed! Rewards: {rewardsList}</color>");

        SceneManager.LoadScene("ChessGame");
    }

    public void FailPuzzle(string name, string nextPiece, Vector2Int pos)
    {
        string rewardsList = string.Join(", ", rewardPieces);
        Debug.Log($"Puzzle Completed! Rewards: {rewardsList}</color>");

        if (!string.IsNullOrEmpty(nextPiece) && kingPuzzleCompleted == false)
        {
            ChessGame.Instance.QueuePawnRestore(nextPiece, pos);
        }

        SceneManager.LoadScene("ChessGame");
    }
}


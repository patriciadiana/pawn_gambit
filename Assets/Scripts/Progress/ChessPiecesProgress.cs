using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzlePiecesDisplay : MonoBehaviour
{
    public static PuzzlePiecesDisplay Instance; // Singleton

    [System.Serializable]
    public class PuzzlePiece
    {
        public string pieceName;
        public Image imageUI;
    }

    public PuzzlePiece[] piecesArray;
    private Dictionary<string, Image> piecesDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        piecesDict = new Dictionary<string, Image>();
        foreach (var piece in piecesArray)
        {
            if (!piecesDict.ContainsKey(piece.pieceName))
                piecesDict.Add(piece.pieceName, piece.imageUI);
        }
    }

    public void UnlockPiece(string name)
    {
        if (piecesDict.TryGetValue(name, out Image img))
        {
            img.color = Color.white;
        }
        else
        {
            Debug.LogWarning("Puzzle piece not found: " + name);
        }
    }

    public void ResetPieces()
    {
        foreach (var img in piecesDict.Values)
            img.color = Color.black;
    }
}

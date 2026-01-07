using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RookPuzzle : MonoBehaviour
{
    public Transform player;
    public Transform[] crates;
    public Transform[] goals;

    public TextMeshProUGUI cratesLeftText;

    private bool isMoving = false;

    public PuzzleConfig puzzleConfig;
    private float timer;

    private Vector3 playerStartPos;
    private Vector3[] crateStartPositions;

    void Start()
    {
        SoundManager.Instance.PauseMusic();
        SoundManager.PlayMusic(MusicType.ROOKTHEME);

        // Hide UI until HowTo is displayed
        cratesLeftText.gameObject.SetActive(false);

        playerStartPos = player.position;
        crateStartPositions = new Vector3[crates.Length];
        for (int i = 0; i < crates.Length; i++)
        {
            crateStartPositions[i] = crates[i].position;
        }

        if (puzzleConfig.hasTimeLimit)
            timer = puzzleConfig.timeLimitinSeconds;
    }

    void Update()
    {
        // Show UI only after HowToDisplay is done
        if (HowToDisplay.hasBeenDisplayed && !cratesLeftText.gameObject.activeSelf)
        {
            cratesLeftText.gameObject.SetActive(true);
            UpdateUIText();
        }

        if (isMoving) return;

        if (puzzleConfig.hasTimeLimit)
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
            }
        }

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) dir = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S)) dir = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) dir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D)) dir = Vector2Int.right;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
            return;
        }

        if (dir != Vector2Int.zero)
            TryMove(dir);
    }

    void TryMove(Vector2Int dir)
    {
        isMoving = true;

        Vector2Int playerGrid = BoardConfig.WorldToGrid(player.position);
        Vector2Int nextGrid = playerGrid + dir;

        if (!BoardConfig.IsInsideBoard(nextGrid))
        {
            isMoving = false;
            return;
        }

        Transform crateAtNext = GetCrateAtGrid(nextGrid);

        if (crateAtNext == null)
        {
            player.position = BoardConfig.GridToWorld(nextGrid);
        }
        else
        {
            Vector2Int beyondGrid = nextGrid + dir;
            if (!BoardConfig.IsInsideBoard(beyondGrid))
            {
                isMoving = false;
                return;
            }

            if (GetCrateAtGrid(beyondGrid) == null)
            {
                crateAtNext.position = BoardConfig.GridToWorld(beyondGrid);
                player.position = BoardConfig.GridToWorld(nextGrid);
            }
        }

        isMoving = false;
        CheckWin();
        UpdateUIText();
    }

    Transform GetCrateAtGrid(Vector2Int g)
    {
        foreach (var c in crates)
        {
            if (BoardConfig.WorldToGrid(c.position) == g)
                return c;
        }
        return null;
    }

    void CheckWin()
    {
        HashSet<Vector2Int> cratePositions = new HashSet<Vector2Int>();
        int correct = 0;

        foreach (var c in crates)
            cratePositions.Add(BoardConfig.WorldToGrid(c.position));

        foreach (var g in goals)
        {
            if (cratePositions.Contains(BoardConfig.WorldToGrid(g.position)))
                correct++;
        }

        if (correct == goals.Length)
        {
            PuzzlePiecesDisplay.Instance.UnlockPiece("rook");

            PuzzleManager.Instance.CompletePuzzle(
                puzzleConfig.initialSceneName,
                puzzleConfig.nextPieceType,
                puzzleConfig.nextPiecePosition,
                puzzleConfig.rewardType
            );
        }
    }

    void ResetBoard()
    {
        player.position = playerStartPos;

        for (int i = 0; i < crates.Length; i++)
        {
            crates[i].position = crateStartPositions[i];
        }

        UpdateUIText();
    }

    void UpdateUIText()
    {
        if (!HowToDisplay.hasBeenDisplayed)
            return;

        int placed = 0;
        HashSet<Vector2Int> cratePositions = new HashSet<Vector2Int>();

        foreach (var c in crates)
            cratePositions.Add(BoardConfig.WorldToGrid(c.position));

        foreach (var g in goals)
        {
            if (cratePositions.Contains(BoardConfig.WorldToGrid(g.position)))
                placed++;
        }

        int remaining = goals.Length - placed;
        cratesLeftText.text = $"Crates left: {remaining}";
    }
}

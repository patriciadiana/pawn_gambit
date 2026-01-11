using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private Vector3[] goalStartPositions;

    public Sprite goalEmptySprite;
    public Sprite goalFilledSprite;

    void Start()
    {
        SoundManager.Instance.PauseMusic();
        SoundManager.PlayMusic(MusicType.ROOKTHEME);

        cratesLeftText.gameObject.SetActive(false);

        playerStartPos = player.position;

        crateStartPositions = new Vector3[crates.Length];
        for (int i = 0; i < crates.Length; i++)
            crateStartPositions[i] = crates[i].position;

        goalStartPositions = new Vector3[goals.Length];
        for (int i = 0; i < goals.Length; i++)
            goalStartPositions[i] = goals[i].position;

        if (puzzleConfig.hasTimeLimit)
            timer = puzzleConfig.timeLimitinSeconds;
    }

    void Update()
    {
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

    Transform GetCrateOnGoal(Vector2Int goalGrid)
    {
        foreach (var c in crates)
        {
            if (BoardConfig.WorldToGrid(c.position) == goalGrid)
                return c;
        }
        return null;
    }

    void CheckWin()
    {
        int correct = 0;

        foreach (var g in goals)
        {
            if (GetCrateOnGoal(BoardConfig.WorldToGrid(g.position)) != null)
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

            SpriteRenderer crateSR = crates[i].GetComponent<SpriteRenderer>();
            if (crateSR != null)
                crateSR.enabled = true;
        }

        for (int i = 0; i < goals.Length; i++)
        {
            goals[i].position = goalStartPositions[i];

            SpriteRenderer sr = goals[i].GetComponent<SpriteRenderer>();
            if (sr != null && goalEmptySprite != null)
                sr.sprite = goalEmptySprite;
        }

        UpdateUIText();
    }

    void UpdateUIText()
    {
        if (!HowToDisplay.hasBeenDisplayed)
            return;

        // 1️⃣ Always re-enable all crates first
        foreach (var c in crates)
        {
            SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = true;
        }

        int placed = 0;

        // 2️⃣ Handle goals
        for (int i = 0; i < goals.Length; i++)
        {
            Transform g = goals[i];
            Vector2Int goalGrid = BoardConfig.WorldToGrid(g.position);
            SpriteRenderer goalSR = g.GetComponent<SpriteRenderer>();
            Transform crate = GetCrateOnGoal(goalGrid);

            if (crate != null)
            {
                placed++;

                // Hide crate while on goal
                SpriteRenderer crateSR = crate.GetComponent<SpriteRenderer>();
                if (crateSR != null)
                    crateSR.enabled = false;

                // Swap goal sprite
                if (goalSR != null && goalFilledSprite != null)
                    goalSR.sprite = goalFilledSprite;

                // Apply Y offset ONCE
                g.position = goalStartPositions[i] + Vector3.up * 0.2f;
            }
            else
            {
                // Restore goal
                if (goalSR != null && goalEmptySprite != null)
                    goalSR.sprite = goalEmptySprite;

                g.position = goalStartPositions[i];
            }
        }

        int remaining = goals.Length - placed;
        cratesLeftText.text = $"Crates left: {remaining}";
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QueensProblem : MonoBehaviour
{
    private Camera mainCamera;
    private Collider2D boardCollider;
    private int queensCounter;

    public PuzzleConfig puzzleConfig;
    private float timer;

    void Start()
    {
        mainCamera = Camera.main;
        boardCollider = GetComponent<Collider2D>();

        if (puzzleConfig.hasTimeLimit)
        {
            timer = puzzleConfig.timeLimitinSeconds;
        }
    }
    void Update()
    {
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
    }
    private void OnMouseDown()
    {
        Vector2Int boardPos = GetClickedBoardPosition();

        if (boardPos.x >= 0 && boardPos.y >= 0)
        {
            GameObject existingPiece = ChessGame.Instance.GetPosition(boardPos.x, boardPos.y);

            if (existingPiece == null)
            {
                ChessGame.Instance.SpawnChessPiece("white_queen", boardPos.x, boardPos.y);
            }
            else if (existingPiece != null && existingPiece.name.Contains("queen"))
            {
                Destroy(existingPiece);
            }
            queensCounter = GetAllQueenPositions().Count;
            CheckWinCondition();
        }
    }
    public List<Vector2Int> GetAllQueenPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = ChessGame.Instance.GetPosition(x, y);
                if (piece != null && piece.name.Contains("queen"))
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }
        return positions;
    }
    private void CheckWinCondition()
    {
        if (queensCounter == 8)
        {
            List<Vector2Int> queens = GetAllQueenPositions();
            bool attacking = false;

            for (int i = 0; i < queens.Count; i++)
            {
                for (int j = i + 1; j < queens.Count; j++)
                {
                    if (AreQueensAttacking(queens[i], queens[j]))
                    {
                        attacking = true;
                        break;
                    }
                }
                if (attacking) break;
            }

            if (!attacking)
            {
                PuzzlePiecesDisplay.Instance.UnlockPiece("queen");

                PuzzleManager.Instance.CompletePuzzle(
                    puzzleConfig.initialSceneName,
                    puzzleConfig.nextPieceType,
                    puzzleConfig.nextPiecePosition,
                    puzzleConfig.rewardType
                );
            }
        }
    }
    private bool AreQueensAttacking(Vector2Int a, Vector2Int b)
    {
        return (a.x == b.x || a.y == b.y || Mathf.Abs(a.x - b.x) == Mathf.Abs(a.y - b.y));
    }
    private Vector2Int GetClickedBoardPosition()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        if (boardCollider.OverlapPoint(mouseWorldPos))
        {
            Vector2Int gridPos = BoardConfig.WorldToGrid(mouseWorldPos);

            return BoardConfig.ClampToBoard(gridPos);
        }
        
        return new Vector2Int(-1, -1);
    }
}

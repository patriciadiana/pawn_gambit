using UnityEngine;
using System.Collections.Generic;

public class KnightTour : MonoBehaviour
{
    public GameObject squarePrefab;
    public PuzzleConfig puzzleConfig;
    public bool puzzleWon = false;
    private bool puzzleCompleted = false;

    public float timer;

    private GameObject[,] squares = new GameObject[8, 8];
    private ChessPiece knight;
    private Vector2Int lastPos = new Vector2Int(-1, -1);

    private readonly HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

    private bool hasShownCompletion = false;

    void Start()
    {
        SoundManager.Instance.PauseMusic();
        SoundManager.PlayMusic(MusicType.KNIGHTTHEME);

        var knightObj = ChessGame.Instance.SpawnChessPiece("black_knight", 0, 0);
        knight = knightObj.GetComponent<ChessPiece>();
        knight.ignorePuzzleRestriction = true;

        if (puzzleConfig.hasTimeLimit)
        {
            timer = puzzleConfig.timeLimitinSeconds;
        }

        CreateBoardSquares();

        lastPos = new Vector2Int(knight.xBoard, knight.yBoard);
        visited.Add(lastPos);

        UpdateColors();
    }

    void Update()
    {
        if (puzzleWon && !puzzleCompleted)
        {
            puzzleCompleted = true;

            PuzzlePiecesDisplay.Instance.UnlockPiece("knight");

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
            }
        }

        Vector2Int currentPos = new Vector2Int(knight.xBoard, knight.yBoard);

        if (currentPos == lastPos)
            return; 

        foreach (var p in GetLShapePath(lastPos, currentPos))
            visited.Add(p);

        visited.Add(currentPos);

        lastPos = currentPos;

        UpdateColors();
    }

    List<Vector2Int> GetLShapePath(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        if (Mathf.Abs(dx) == 2 && Mathf.Abs(dy) == 1)
        {
            path.Add(new Vector2Int(from.x + stepX, from.y));
            path.Add(new Vector2Int(from.x + 2 * stepX, from.y));
            path.Add(new Vector2Int(from.x + 2 * stepX, from.y + stepY));
        }

        else if (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 2)
        {
            path.Add(new Vector2Int(from.x, from.y + stepY));
            path.Add(new Vector2Int(from.x, from.y + 2 * stepY));
            path.Add(new Vector2Int(from.x + stepX, from.y + 2 * stepY));
        }

        return path;
    }
    void CreateBoardSquares()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector3 pos = BoardConfig.GridToWorld(new Vector2Int(x, y));
                pos.z = -0.5f;

                var sq = Instantiate(squarePrefab, pos, Quaternion.identity);
                sq.transform.localScale = Vector3.one * BoardConfig.TileSize;

                squares[x, y] = sq;
                squares[x, y].GetComponent<SpriteRenderer>().color = DefaultColor;
            }
        }
    }

    void UpdateColors()
    {
        foreach (var sq in squares)
            sq.GetComponent<SpriteRenderer>().color = DefaultColor;

        foreach (Vector2Int pos in visited)
            squares[pos.x, pos.y].GetComponent<SpriteRenderer>().color = Dark;

        squares[lastPos.x, lastPos.y].GetComponent<SpriteRenderer>().color = Dark;

        if (!hasShownCompletion && visited.Count >= 64)
        {
            hasShownCompletion = true;
            OnBoardCompleted();
        }
    }

    void OnBoardCompleted()
    {
        puzzleWon = true;
    }

    Color DefaultColor => new Color(1, 1, 1, 0.15f);
    Color Dark = new Color(0.1216f, 0.1373f, 0.1725f, 1f);
}

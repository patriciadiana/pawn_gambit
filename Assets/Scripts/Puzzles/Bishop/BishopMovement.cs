using UnityEngine;

public class BishopMovement : MonoBehaviour
{
    public PuzzleConfig puzzleConfig;
    public bool puzzleWon = false;
    private bool puzzleCompleted = false;

    private float timer;
    private GameObject bishop;
    private ChessPiece bishopScript;

    public void MarkWin()
    {
        puzzleWon = true;
    }

    private void Start()
    {
        SoundManager.Instance.PauseMusic();
        SoundManager.PlayMusic(MusicType.BISHOPTHEME, 0.2f);

        bishop = ChessGame.Instance.SpawnChessPiece("white_bishop", 2, 0);
        bishopScript = bishop.GetComponent<ChessPiece>();
        bishopScript.ignorePuzzleRestriction = true;

        if (puzzleConfig.hasTimeLimit)
        {
            timer = puzzleConfig.timeLimitinSeconds;
        }
    }

    void Update()
    {
        if (puzzleWon && !puzzleCompleted)
        {
            puzzleCompleted = true;

            PuzzlePiecesDisplay.Instance.UnlockPiece("bishop");

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
    }
}

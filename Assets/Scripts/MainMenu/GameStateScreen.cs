using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateScreen : MonoBehaviour
{
    public GameObject gameOver;
    public GameObject gameWon;
    public bool gameOverClicked = false;

    public void SetupGameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        gameOver.SetActive(true);

        gameOverClicked = false;
    }

    public void SetupGameWon()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        gameWon.SetActive(true);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartButton()
    {
        Debug.Log("Restart clicked");

        gameOverClicked = true;

        PuzzleManager.Instance.ClearRewards();
        PuzzlePiecesDisplay.Instance.ResetPieces();

        // Destroy singleton to allow new instance
        if (ChessGame.Instance != null)
        {
            Destroy(ChessGame.Instance.gameObject);
            ChessGame.Instance = null;
        }

        Time.timeScale = 1f;
        gameOver.SetActive(false);

        SceneManager.LoadScene("ChessGame");
    }

}

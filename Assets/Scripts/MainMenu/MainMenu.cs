using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    public static bool MenuLoadedFromGame = false;

    public GameObject optionsMenu;
    public void PlayGame()
    {
        if (MenuLoadedFromGame)
        {
            PuzzleManager.Instance.ClearRewards();
            PuzzlePiecesDisplay.Instance.ResetPieces();

            if (ChessGame.Instance != null)
            {
                Destroy(ChessGame.Instance.gameObject);
                ChessGame.Instance = null;
            }

            MenuLoadedFromGame = false;
        }

        SceneManager.LoadScene("ChessGame");
    }
    public void QuitGame()
    {
        Debug.Log("Game is quitted");
        Application.Quit();
    }

    public void OnOptionsButtonClicked()
    {
        optionsMenu.SetActive(true);
        gameObject.SetActive(false);

        OptionsMenu.Instance.SetLastMenu("MainMenu");
    }
}

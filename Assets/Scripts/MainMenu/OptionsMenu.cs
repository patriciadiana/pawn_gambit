using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance;

    private string lastMenu;

    private void Awake()
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

    public void SetLastMenu(string menu)
    {
        lastMenu = menu;
    }

    public void OnBackButtonClicked()
    {
        if (lastMenu == "MainMenu")
        {
            MainMenu.Instance.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
        else if (lastMenu == "PauseMenu")
        {
            PauseMenu.Instance.pauseMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
using System.Collections;
using UnityEngine;
using TMPro;

public class HowToDisplay : MonoBehaviour
{
    [Header("UI")]
    public GameObject howToPanel;
    public TextMeshProUGUI howToText;

    [Header("Text")]
    [TextArea(3, 6)]
    public string puzzleInstructions;

    [Header("Timing")]
    public float closeTime = 3f;

    [Header("Input (Optional)")]
    public KeyCode skipKey = KeyCode.Space;

    private Coroutine hideRoutine;
    public bool displayed = false;
    public static bool hasBeenDisplayed = false;

    void Start()
    {
        ShowPanel();
    }

    void Update()
    {
        displayed = hasBeenDisplayed;

        if (howToPanel.activeSelf && Input.GetKeyDown(skipKey))
        {
            HidePanel();
        }
    }

    public void ShowPanel()
    {
        howToText.text = puzzleInstructions;

        hasBeenDisplayed = false;

        howToPanel.SetActive(true);
        Time.timeScale = 0f;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(closeTime);
        hasBeenDisplayed = true;
        HidePanel();
    }

    public void HidePanel()
    {
        howToPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}

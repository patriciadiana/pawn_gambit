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

    void Start()
    {
        ShowPanel();
    }

    void Update()
    {
        if (howToPanel.activeSelf && Input.GetKeyDown(skipKey))
        {
            HidePanel();
        }
    }

    public void ShowPanel()
    {
        howToText.text = puzzleInstructions;

        howToPanel.SetActive(true);
        Time.timeScale = 0f;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(closeTime);
        HidePanel();
    }

    public void HidePanel()
    {
        howToPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}

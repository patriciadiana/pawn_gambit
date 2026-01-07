using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public Image image;
    public TMP_Text text;

    [Header("Content")]
    public Sprite[] sprites;
    [TextArea(2, 5)]
    public string[] texts;

    [Header("Timing")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float displayDuration = 2f;

    private static bool hasPlayed = false;

    void Start()
    {
        if (hasPlayed)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return;
        }

        hasPlayed = true; 

        Time.timeScale = 0f;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        int count = Mathf.Min(sprites.Length, texts.Length);
        if (count == 0) yield break;

        image.sprite = sprites[0];
        text.text = texts[0];
        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(displayDuration);

        for (int i = 1; i < count; i++)
        {
            yield return Fade(1f, 0.5f, fadeInDuration / 3f);
            image.sprite = sprites[i];
            text.text = texts[i];
            yield return Fade(0.5f, 1f, fadeInDuration / 3f);
            yield return new WaitForSecondsRealtime(displayDuration);
        }

        yield return Fade(1f, 0f, fadeOutDuration);
        Time.timeScale = 1f;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}

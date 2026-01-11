using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MidgameUIDisplay : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image image1;
    [SerializeField] private Image image2;

    [Header("Timing")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float showTime = 3f;

    private Coroutine sequenceRoutine;
    [SerializeField] private DynamicRewardDisplay rewardDisplay;

    void Awake()
    {
        Hide(image1);
        Hide(image2);
    }

    public void Play()
    {
        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return null;

        rewardDisplay.ShowRewards();

        yield return FadeIn(image1);
        yield return new WaitForSeconds(showTime);
        yield return FadeOut(image1);

        yield return FadeIn(image2);
        yield return new WaitForSeconds(showTime);
        yield return FadeOut(image2);
    }

    IEnumerator FadeIn(Image img)
    {
        img.gameObject.SetActive(true);
        yield return Fade(img, 0f, 1f);
    }

    IEnumerator FadeOut(Image img)
    {
        yield return Fade(img, 1f, 0f);
        img.gameObject.SetActive(false);
    }

    IEnumerator Fade(Image img, float from, float to)
    {
        float t = 0f;
        Color c = img.color;
        c.a = from;
        img.color = c;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeTime);
            img.color = c;
            yield return null;
        }

        c.a = to;
        img.color = c;
    }

    void Hide(Image img)
    {
        if (!img) return;
        Color c = img.color;
        c.a = 0f;
        img.color = c;
        img.gameObject.SetActive(false);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;

    [Header("Startup")]
    public bool startBlack = false;

    private Coroutine _currentRoutine;

    private void Awake()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = startBlack ? 1f : 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }
    }

    public IEnumerator FadeToBlack(float duration)
    {
        if (fadeImage == null) yield break;

        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        yield return Fade(0f, 1f, duration);
    }

    public IEnumerator FadeFromBlack(float duration)
    {
        if (fadeImage == null) yield break;

        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        yield return Fade(1f, 0f, duration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;

        Color c = fadeImage.color;
        c.a = from;
        fadeImage.color = c;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            c.a = Mathf.Lerp(from, to, a);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}
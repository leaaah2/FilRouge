using System.Collections;
using UnityEngine;

public class SceneFadeInOnStart : MonoBehaviour
{
    public ScreenFader screenFader;
    public float fadeInDuration = 1f;

    private IEnumerator Start()
    {
        if (screenFader != null)
            yield return screenFader.FadeFromBlack(fadeInDuration);
    }
}
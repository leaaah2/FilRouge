using System.Collections;
using UnityEngine;

public class ForestSceneStartupFade : MonoBehaviour
{
    [Header("References")]
    public ScreenFader screenFader;
    public WeatherSkyController weatherSkyController;

    [Header("Timing")]
    public float extraDelayAfterReady = 0.2f;
    public float fadeInDuration = 1f;

    private IEnumerator Start()
    {
        // Stay black until the weather system is ready
        while (weatherSkyController == null || !weatherSkyController.IsReady())
            yield return null;

        // Let one or two frames pass so dependent systems update too
        yield return null;
        yield return null;

        if (extraDelayAfterReady > 0f)
            yield return new WaitForSeconds(extraDelayAfterReady);

        if (screenFader != null)
            yield return screenFader.FadeFromBlack(fadeInDuration);
    }
}
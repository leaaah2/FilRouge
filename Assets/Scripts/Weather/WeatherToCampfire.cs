using UnityEngine;

public class WeatherToCampfire : MonoBehaviour
{
    public WeatherSkyController weatherSkyController;
    public CampfireController[] campfires;

    private void Awake()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnCampfireWeatherChanged += HandleCampfireWeatherChanged;
    }

    private void OnDestroy()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnCampfireWeatherChanged -= HandleCampfireWeatherChanged;
    }

    private void HandleCampfireWeatherChanged(float sunAltitude, float precipitation)
    {
        if (campfires == null) return;

        foreach (CampfireController campfire in campfires)
        {
            if (campfire == null) continue;
            campfire.ApplyWeather(sunAltitude, precipitation);
        }
    }
}
using UnityEngine;

public class WeatherToPrecipitation : MonoBehaviour
{
    public WeatherSkyController weatherSkyController;
    public WeatherPrecipitationController precipitationController;

    private void Awake()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnPrecipitationChanged += HandlePrecipitationChanged;
    }

    private void OnDestroy()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnPrecipitationChanged -= HandlePrecipitationChanged;
    }

    private void HandlePrecipitationChanged(float temperature, float precipitation, float windSpeed, float windDirection)
    {
        precipitationController.ApplyClimate(temperature, precipitation, windSpeed, windDirection);
    }
}
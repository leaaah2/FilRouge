using UnityEngine;

public class WeatherToBreath : MonoBehaviour
{
    public WeatherSkyController weatherSkyController;
    public ColdBreathController coldBreathController;

    private void Awake()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnPrecipitationChanged += HandleWeatherChanged;
    }

    private void OnDestroy()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnPrecipitationChanged -= HandleWeatherChanged;
    }

    private void HandleWeatherChanged(float temperature, float precipitation, float windSpeed, float windDirection)
    {
        if (coldBreathController == null) return;
        coldBreathController.ApplyTemperature(temperature);
    }
}
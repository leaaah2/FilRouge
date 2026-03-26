using UnityEngine;

public class WeatherToClimateScatter : MonoBehaviour
{
    [Header("References")]
    public WeatherSkyController weatherSkyController;
    public ClimateScatter[] scatterTargets;

    private void Awake()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnClimateSampleChanged += HandleClimateSampleChanged;
    }

    private void OnDestroy()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnClimateSampleChanged -= HandleClimateSampleChanged;
    }

    private void HandleClimateSampleChanged(int hourIndex, float temperature, float humidity, float precipitation, float windSpeed, float windDirection)
    {
        foreach (ClimateScatter scatter in scatterTargets)
        {
            if (scatter == null)
                continue;

            scatter.ApplyClimateIfRuleSetChanged(temperature, humidity);
        }
    }
}
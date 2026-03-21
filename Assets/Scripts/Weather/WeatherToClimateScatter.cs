using UnityEngine;

public class WeatherToClimateScatter : MonoBehaviour
{
    [Header("References")]
    public WeatherSkyController weatherSkyController;
    public ClimateScatter[] climateScatterTargets;

    [Header("Behavior")]
    public bool applyOnceOnStart = true;
    public bool clearBeforeGenerate = true;

    private int _lastAppliedHour = -1;

    private void Awake()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnClimateHourChanged += HandleClimateHourChanged;
    }

    private void OnDestroy()
    {
        if (weatherSkyController != null)
            weatherSkyController.OnClimateHourChanged -= HandleClimateHourChanged;
    }

    private void Start()
    {
        if (!applyOnceOnStart || weatherSkyController == null)
            return;

        if (weatherSkyController.TryGetClimateAtCurrentTime(out float temp, out float humidity, out int hourIndex))
        {
            ApplyClimateToTargets(temp, humidity, hourIndex);
        }
    }

    private void HandleClimateHourChanged(int hourIndex, float temperature, float humidity)
    {
        ApplyClimateToTargets(temperature, humidity, hourIndex);
    }

    private void ApplyClimateToTargets(float temperature, float humidity, int hourIndex)
    {
        if (hourIndex == _lastAppliedHour)
            return;

        _lastAppliedHour = hourIndex;

        if (climateScatterTargets == null || climateScatterTargets.Length == 0)
            return;

        Debug.Log($"WeatherToClimateScatter: updating fauna at hour {hourIndex} | T={temperature:0.0}°C H={humidity:0.0}%");

        foreach (ClimateScatter scatter in climateScatterTargets)
        {
            if (scatter == null)
                continue;

            scatter.SetClimate(temperature, humidity);

            if (clearBeforeGenerate)
                scatter.Clear();

            scatter.Generate();
        }
    }
}
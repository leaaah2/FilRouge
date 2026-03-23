using UnityEngine;

public class WeatherToSnowAccumulation : MonoBehaviour
{
    [Header("References")]
    public WeatherSkyController weatherSkyController;

    [Header("Snow State")]
    [Range(0f, 1f)] public float snowAmount = 0f;

    [Header("Behavior")]
    public float accumulateSpeed = 0.05f;
    public float meltSpeed = 0.03f;
    public float rainMeltMultiplier = 2f;
    public float freezeTemperature = 0f;

    [Header("Debug")]
    public bool logSnowChanges = false;

    private float _currentTemperature;
    private float _currentPrecipitation;
    private bool _hasWeather;

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

    private void Update()
    {
        if (!_hasWeather)
            return;

        bool snowing = _currentTemperature <= freezeTemperature && _currentPrecipitation > 0.05f;
        bool melting = _currentTemperature > freezeTemperature;

        if (snowing)
        {
            snowAmount += accumulateSpeed * Time.deltaTime * Mathf.Clamp01(_currentPrecipitation);
        }
        else if (melting)
        {
            float melt = meltSpeed * Time.deltaTime;

            if (_currentPrecipitation > 0.05f)
                melt *= rainMeltMultiplier;

            snowAmount -= melt;
        }

        snowAmount = Mathf.Clamp01(snowAmount);

        Shader.SetGlobalFloat("_GlobalSnowAmount", snowAmount);
    }

    private void HandlePrecipitationChanged(float temperature, float precipitation, float windSpeed, float windDirection)
    {
        _currentTemperature = temperature;
        _currentPrecipitation = precipitation;
        _hasWeather = true;

        if (logSnowChanges)
        {
            Debug.Log(
                $"Snow system weather update | Temp={temperature:0.0}°C | Precip={precipitation:0.00} | SnowAmount={snowAmount:0.00}"
            );
        }
    }
}
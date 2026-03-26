using UnityEngine.InputSystem;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherDebugPanelController : MonoBehaviour
{
    [Header("Target")]
    public WeatherSkyController weatherSkyController;

    [Header("Root")]
    public GameObject panelRoot;

    [Header("Controller Toggle")]
    public InputActionReference toggleAction;

    [Header("Toggles")]
    public Toggle enableDebugWeatherToggle;
    public Toggle enableDebugTimeToggle;

    [Header("Sliders")]
    public Slider temperatureSlider;
    public Slider humiditySlider;
    public Slider precipitationSlider;
    public Slider cloudCoverSlider;
    public Slider windSpeedSlider;
    public Slider windDirectionSlider;
    public Slider hourOfDaySlider;

    [Header("Labels")]
    public TextMeshProUGUI temperatureValueText;
    public TextMeshProUGUI humidityValueText;
    public TextMeshProUGUI precipitationValueText;
    public TextMeshProUGUI cloudCoverValueText;
    public TextMeshProUGUI windSpeedValueText;
    public TextMeshProUGUI windDirectionValueText;
    public TextMeshProUGUI hourOfDayValueText;

    [Header("Optional Toggle Key")]
    public bool allowKeyboardToggleInEditor = true;
    public KeyCode togglePanelKey = KeyCode.F1;

    private void Start()
    {
        if (weatherSkyController == null)
        {
            Debug.LogWarning("WeatherDebugPanelController: WeatherSkyController not assigned.");
            return;
        }

        PullFromWeatherController();
        BindUI();
        RefreshLabels();
    }


    private void OnEnable()
    {
        if (toggleAction != null)
            toggleAction.action.performed += OnTogglePerformed;
    }

    private void OnDisable()
    {
        if (toggleAction != null)
            toggleAction.action.performed -= OnTogglePerformed;
    }

    private void OnTogglePerformed(InputAction.CallbackContext ctx)
    {
        TogglePanel();
    }



    public void TogglePanel()
    {
        if (panelRoot == null) return;
        panelRoot.SetActive(!panelRoot.activeSelf);
    }

    public void ShowPanel(bool show)
    {
        if (panelRoot == null) return;
        panelRoot.SetActive(show);
    }

    private void PullFromWeatherController()
    {
        enableDebugWeatherToggle.isOn = weatherSkyController.useDebugWeather;
        enableDebugTimeToggle.isOn = weatherSkyController.useDebugTime;

        temperatureSlider.value = weatherSkyController.debugTemperature;
        humiditySlider.value = weatherSkyController.debugHumidity;
        precipitationSlider.value = weatherSkyController.debugPrecipitation;
        cloudCoverSlider.value = weatherSkyController.debugCloudCover;
        windSpeedSlider.value = weatherSkyController.debugWindSpeed;
        windDirectionSlider.value = weatherSkyController.debugWindDirection;
        hourOfDaySlider.value = weatherSkyController.debugHourOfDay;
    }

    private void BindUI()
    {
        if (enableDebugWeatherToggle != null)
            enableDebugWeatherToggle.onValueChanged.AddListener(OnDebugWeatherToggleChanged);

        if (enableDebugTimeToggle != null)
            enableDebugTimeToggle.onValueChanged.AddListener(OnDebugTimeToggleChanged);

        if (temperatureSlider != null)
            temperatureSlider.onValueChanged.AddListener(OnTemperatureChanged);

        if (humiditySlider != null)
            humiditySlider.onValueChanged.AddListener(OnHumidityChanged);

        if (precipitationSlider != null)
            precipitationSlider.onValueChanged.AddListener(OnPrecipitationChanged);

        if (cloudCoverSlider != null)
            cloudCoverSlider.onValueChanged.AddListener(OnCloudCoverChanged);

        if (windSpeedSlider != null)
            windSpeedSlider.onValueChanged.AddListener(OnWindSpeedChanged);

        if (windDirectionSlider != null)
            windDirectionSlider.onValueChanged.AddListener(OnWindDirectionChanged);

        if (hourOfDaySlider != null)
            hourOfDaySlider.onValueChanged.AddListener(OnHourOfDayChanged);
    }

    private void RefreshLabels()
    {
        if (temperatureValueText != null)
            temperatureValueText.text = $"{weatherSkyController.debugTemperature:0.0} °C";

        if (humidityValueText != null)
            humidityValueText.text = $"{weatherSkyController.debugHumidity:0}%";

        if (precipitationValueText != null)
            precipitationValueText.text = $"{weatherSkyController.debugPrecipitation:0.00} mm";

        if (cloudCoverValueText != null)
            cloudCoverValueText.text = $"{weatherSkyController.debugCloudCover:0}%";

        if (windSpeedValueText != null)
            windSpeedValueText.text = $"{weatherSkyController.debugWindSpeed:0.0} km/h";

        if (windDirectionValueText != null)
            windDirectionValueText.text = $"{weatherSkyController.debugWindDirection:0}°";

        if (hourOfDayValueText != null)
            hourOfDayValueText.text = $"{weatherSkyController.debugHourOfDay:0.00} h";
    }

    private void OnDebugWeatherToggleChanged(bool value)
    {
        weatherSkyController.useDebugWeather = value;
    }

    private void OnDebugTimeToggleChanged(bool value)
    {
        weatherSkyController.useDebugTime = value;
    }

    private void OnTemperatureChanged(float value)
    {
        weatherSkyController.useDebugWeather = true;
        if (enableDebugWeatherToggle != null) enableDebugWeatherToggle.isOn = true;

        weatherSkyController.debugTemperature = value;
        RefreshLabels();
    }

    private void OnHumidityChanged(float value)
    {
        weatherSkyController.useDebugWeather = true;
        if (enableDebugWeatherToggle != null) enableDebugWeatherToggle.isOn = true;

        weatherSkyController.debugHumidity = value;
        RefreshLabels();
    }

    private void OnPrecipitationChanged(float value)
    {
        weatherSkyController.useDebugWeather = true;
        if (enableDebugWeatherToggle != null) enableDebugWeatherToggle.isOn = true;

        weatherSkyController.debugPrecipitation = value;
        RefreshLabels();
    }

    private void OnCloudCoverChanged(float value)
    {
        weatherSkyController.useDebugWeather = true;
        if (enableDebugWeatherToggle != null) enableDebugWeatherToggle.isOn = true;

        weatherSkyController.debugCloudCover = value;
        RefreshLabels();
    }

    private void OnWindSpeedChanged(float value)
    {
        weatherSkyController.useDebugWeather = true;
        if (enableDebugWeatherToggle != null) enableDebugWeatherToggle.isOn = true;

        weatherSkyController.debugWindSpeed = value;
        RefreshLabels();
    }

    private void OnWindDirectionChanged(float value)
    {
        weatherSkyController.useDebugWeather = true;
        if (enableDebugWeatherToggle != null) enableDebugWeatherToggle.isOn = true;

        weatherSkyController.debugWindDirection = value;
        RefreshLabels();
    }

    private void OnHourOfDayChanged(float value)
    {
        weatherSkyController.useDebugTime = true;
        if (enableDebugTimeToggle != null) enableDebugTimeToggle.isOn = true;

        weatherSkyController.debugHourOfDay = value;
        RefreshLabels();
    }
}
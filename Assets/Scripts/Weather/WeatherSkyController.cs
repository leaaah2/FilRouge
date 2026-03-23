using System;
using System.Globalization;
using UnityEngine;

public class WeatherSkyController : MonoBehaviour
{
    [Header("Data")]
    public WeatherApiClient apiClient;
    public SelectedLocationSO selectedLocation;

    [Header("Sun / Moon")]
    public Light sunLight;
    public Light moonLight;
    public Transform moonVisual;

    [Header("Sky Visuals")]
    public Material proceduralSkybox;
    public Renderer starsRenderer;
    public Renderer cloudsRenderer;

    [Header("Cloud Motion")]
    public float minCloudSpeed = 0.002f;
    public float maxCloudSpeed = 0.02f;
    public float cloudCoverageAlphaMultiplier = 0.85f;

    [Header("Lighting")]
    public float sunMaxIntensity = 1.15f;
    public float moonMaxIntensity = 0.2f;
    public float ambientDay = 1.0f;
    public float ambientNight = 0.15f;

    [Header("Fog")]
    public bool controlFog = true;
    public float fogMinDensity = 0.0005f;
    public float fogMaxDensity = 0.01f;

    [Header("Audio")]
    public WeatherAudioController weatherAudioController;

    [Header("Debug")]
    public bool applyOnStartClosestToNow = true;
    public bool useDebugWeather = false;
    public bool useDebugTime = false;

    [Range(-30f, 45f)] public float debugTemperature = 12f;
    [Range(0f, 100f)] public float debugHumidity = 75f;
    [Range(0f, 8f)] public float debugPrecipitation = 0f;
    [Range(0f, 100f)] public float debugCloudCover = 40f;
    [Range(0f, 100f)] public float debugWindSpeed = 0f;
    [Range(0f, 360f)] public float debugWindDirection = 0f;

    [Range(0f, 23.9f)] public float debugHourOfDay = 14f;


    [Header("Logging")]
    public bool enableLogs = true;

    private int _lastLoggedHourIndex = -1;
    private float _lastLoggedTemperature = float.MinValue;
    private float _lastLoggedPrecipitation = float.MinValue;

    public DateTime currentLocalTime;

    private WeatherResponse _data;
    private int _utcOffsetSeconds;
    private Material _starsMat;
    private Material _cloudsMat;
    private float _cloudOffsetX;
    private float _cloudOffsetY;
    private bool _ready;

    public event System.Action<float, float, float, float> OnPrecipitationChanged;

    private void Awake()
    {
        if (apiClient != null)
            apiClient.OnWeatherLoaded += HandleWeatherLoaded;

        if (starsRenderer != null)
            _starsMat = starsRenderer.material;

        if (cloudsRenderer != null)
            _cloudsMat = cloudsRenderer.material;
    }

    private void OnDestroy()
    {
        if (apiClient != null)
            apiClient.OnWeatherLoaded -= HandleWeatherLoaded;
    }

    private void Update()
    {
        if (!_ready) return;

        ApplyAtTime(currentLocalTime, Time.deltaTime);
    }

    private void HandleWeatherLoaded(WeatherResponse data)
    {
        _data = data;
        _utcOffsetSeconds = data.utc_offset_seconds;
        _ready = true;

        if (applyOnStartClosestToNow)
            currentLocalTime = GetClosestLocationNow();
        else if (!TryParseOpenMeteoTime(_data.hourly.time[_data.hourly.time.Length / 2], out currentLocalTime))
            currentLocalTime = DateTime.Now;

        ApplyAtTime(currentLocalTime, 0f);

        Debug.Log($"Sky controller ready. Start time = {currentLocalTime:yyyy-MM-dd HH:mm} | timezone={data.timezone}");
    }

    public void SetCurrentLocalTime(DateTime newLocalTime)
    {
        currentLocalTime = newLocalTime;
    }

    public void NudgeMinutes(float minutes)
    {
        currentLocalTime = currentLocalTime.AddMinutes(minutes);
    }

    public bool IsReady()
    {
        return _ready;
    }

    public bool TryGetTimeBounds(out DateTime minTime, out DateTime maxTime)
    {
        minTime = default;
        maxTime = default;

        if (!_ready) return false;
        if (!TryParseOpenMeteoTime(_data.hourly.time[0], out minTime)) return false;
        if (!TryParseOpenMeteoTime(_data.hourly.time[_data.hourly.time.Length - 1], out maxTime)) return false;

        return true;
    }

    private DateTime GetClosestLocationNow()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime locationNow = utcNow.AddSeconds(_utcOffsetSeconds);

        int bestIndex = 0;
        double bestMinutes = double.MaxValue;

        for (int i = 0; i < _data.hourly.time.Length; i++)
        {
            if (!TryParseOpenMeteoTime(_data.hourly.time[i], out DateTime sampleTime))
                continue;

            double diff = Math.Abs((sampleTime - locationNow).TotalMinutes);
            if (diff < bestMinutes)
            {
                bestMinutes = diff;
                bestIndex = i;
            }
        }

        if (!TryParseOpenMeteoTime(_data.hourly.time[bestIndex], out DateTime result))
            result = locationNow;

        return result;
    }

    private void ApplyAtTime(DateTime targetLocalTime, float deltaTime)
    {
        if (!_ready) return;
    

        float temperature;
        float humidity;
        float cloudCover;
        float windSpeed;
        float windDirection;
        float precipitation;
        int weatherCode;

        DateTime appliedLocalTime = targetLocalTime;

        if (useDebugTime)
        {
            appliedLocalTime = new DateTime(
                targetLocalTime.Year,
                targetLocalTime.Month,
                targetLocalTime.Day,
                Mathf.Clamp(Mathf.FloorToInt(debugHourOfDay), 0, 23),
                Mathf.Clamp(Mathf.FloorToInt(debugHourOfDay % 1f * 60f), 0, 59),
                0
            );
        }

        if (!TryGetInterpolation(appliedLocalTime, out int i0, out int i1, out float t))
            return;

        if (useDebugWeather)
        {
            temperature = debugTemperature;
            //humidity = debugHumidity;
            cloudCover = debugCloudCover;
            windSpeed = debugWindSpeed;
            windDirection = debugWindDirection;

            precipitation = debugPrecipitation;
            weatherCode = 0;
        }
        else
        {
            temperature = Mathf.Lerp(_data.hourly.temperature_2m[i0], _data.hourly.temperature_2m[i1], t);
            //humidity = Mathf.Lerp(_data.hourly.relative_humidity_2m[i0], _data.hourly.relative_humidity_2m[i1], t);
            cloudCover = Mathf.Lerp(_data.hourly.cloud_cover[i0], _data.hourly.cloud_cover[i1], t);
            windSpeed = Mathf.Lerp(_data.hourly.wind_speed_10m[i0], _data.hourly.wind_speed_10m[i1], t);
            windDirection = Mathf.LerpAngle(_data.hourly.wind_direction_10m[i0], _data.hourly.wind_direction_10m[i1], t);
            precipitation = Mathf.Lerp(_data.hourly.precipitation[i0], _data.hourly.precipitation[i1], t);
            weatherCode = t < 0.5f ? _data.hourly.weather_code[i0] : _data.hourly.weather_code[i1];
        }


        float cloud01 = Mathf.Clamp01(cloudCover / 100f);
        float rain01 = Mathf.Clamp01(precipitation / 2f);

        TimeSpan offset = TimeSpan.FromSeconds(_utcOffsetSeconds);
        DateTimeOffset dto = new DateTimeOffset(appliedLocalTime, offset);

        Vector3 sunDir = SolarPosition.SunDirectionENU(selectedLocation.latitude, selectedLocation.longitude, dto);
        SolarPosition.ApplyToDirectionalLight(sunLight, sunDir);

        float sunAltitude = Mathf.Asin(Mathf.Clamp(sunDir.y, -1f, 1f)) * Mathf.Rad2Deg;
        float dayFactor = Mathf.InverseLerp(-6f, 10f, sunAltitude);
        float nightFactor = 1f - dayFactor;
        float starFactor = Mathf.InverseLerp(0f, -12f, sunAltitude);

        UpdateSun(dayFactor, cloud01);
        UpdateMoon(sunDir, nightFactor);
        UpdateStars(starFactor);
        UpdateClouds(cloud01, windSpeed, windDirection, rain01, deltaTime);
        UpdateSkybox(dayFactor, cloud01, rain01);
        UpdateFog(cloud01, rain01, dayFactor);

        OnPrecipitationChanged?.Invoke(temperature, precipitation, windSpeed, windDirection);

        if (weatherAudioController != null)
        {
            weatherAudioController.ApplyWeatherAudio(temperature, precipitation, windSpeed, sunAltitude);
        }

        if (enableLogs)
        {
            bool shouldLog =
                i0 != _lastLoggedHourIndex ||
                Mathf.Abs(temperature - _lastLoggedTemperature) > 0.5f ||
                Mathf.Abs(precipitation - _lastLoggedPrecipitation) > 0.2f;

            if (shouldLog)
            {
                _lastLoggedHourIndex = i0;
                _lastLoggedTemperature = temperature;
                _lastLoggedPrecipitation = precipitation;

                Debug.Log(
                    $"Time={appliedLocalTime:yyyy-MM-dd HH:mm} | Temp={temperature:0.0}°C | Clouds={cloudCover:0}% | " +
                    $"Precipitation={precipitation:0.00} | Wind={windSpeed:0.0}km/h dir={windDirection:0} | Code={weatherCode} | SunAlt={sunAltitude:0.0}"
                );

                //Debug.Log(
                //    $"Time={appliedLocalTime:yyyy-MM-dd HH:mm} | Temp={temperature:0.0}°C | " +
                //    $"Humidity={humidity:0}% | Rain={precipitation:0.00} | Clouds={cloudCover:0}%"
                //);
            }
        }
    }

    private void UpdateSun(float dayFactor, float cloud01)
    {
        if (sunLight == null) return;

        float dimFromClouds = Mathf.Lerp(1f, 0.55f, cloud01);
        sunLight.intensity = Mathf.Lerp(0f, sunMaxIntensity, dayFactor) * dimFromClouds;

        if (dayFactor > 0.2f)
            sunLight.color = Color.Lerp(new Color(1f, 0.65f, 0.45f), Color.white, Mathf.InverseLerp(0.2f, 1f, dayFactor));
        else
            sunLight.color = new Color(1f, 0.55f, 0.4f);
    }

    private void UpdateMoon(Vector3 sunDir, float nightFactor)
    {
        Vector3 moonDir = -sunDir;

        if (moonLight != null)
        {
            SolarPosition.ApplyToDirectionalLight(moonLight, moonDir);
            moonLight.intensity = moonMaxIntensity * nightFactor;
            moonLight.color = new Color(0.72f, 0.8f, 1f);
        }

        if (moonVisual != null)
        {
            float moonDistance = 300f;
            moonVisual.position = transform.position + moonDir * moonDistance;
            moonVisual.LookAt(transform.position);

            Renderer r = moonVisual.GetComponent<Renderer>();
            if (r != null)
            {
                Material m = r.material;
                SetMaterialAlpha(m, nightFactor);
            }
        }
    }

    private void UpdateStars(float starFactor)
    {
        if (_starsMat == null) return;
        SetMaterialAlpha(_starsMat, starFactor);
    }

    private void UpdateClouds(float cloud01, float windSpeed, float windDirectionDeg, float rain01, float deltaTime)
    {
        if (_cloudsMat == null) return;

        float speed01 = Mathf.InverseLerp(0f, 50f, windSpeed);
        float cloudSpeed = Mathf.Lerp(minCloudSpeed, maxCloudSpeed, speed01);

        float dirRad = windDirectionDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Sin(dirRad), Mathf.Cos(dirRad)).normalized;

        _cloudOffsetX += dir.x * cloudSpeed * deltaTime;
        _cloudOffsetY += dir.y * cloudSpeed * deltaTime;

        if (_cloudsMat.HasProperty("_Offset"))
            _cloudsMat.SetVector("_Offset", new Vector4(_cloudOffsetX, _cloudOffsetY, 0f, 0f));

        float alpha = Mathf.Clamp01(cloud01 * cloudCoverageAlphaMultiplier + rain01 * 0.15f);
        SetMaterialAlpha(_cloudsMat, alpha);

        Color baseColor = Color.Lerp(new Color(1f, 1f, 1f, alpha), new Color(0.45f, 0.45f, 0.5f, alpha), rain01);
        SetMaterialColor(_cloudsMat, baseColor);
    }

    private void UpdateSkybox(float dayFactor, float cloud01, float rain01)
    {
        if (proceduralSkybox == null) return;

        RenderSettings.skybox = proceduralSkybox;

        float atmosphere = Mathf.Lerp(0.9f, 1.45f, cloud01);
        float exposureDay = Mathf.Lerp(1.2f, 0.65f, cloud01);
        float exposureNight = Mathf.Lerp(0.45f, 0.2f, cloud01);
        float exposure = Mathf.Lerp(exposureNight, exposureDay, dayFactor);

        exposure = Mathf.Lerp(exposure, exposure * 0.8f, rain01);

        if (proceduralSkybox.HasProperty("_AtmosphereThickness"))
            proceduralSkybox.SetFloat("_AtmosphereThickness", atmosphere);

        if (proceduralSkybox.HasProperty("_Exposure"))
            proceduralSkybox.SetFloat("_Exposure", exposure);

        RenderSettings.ambientIntensity = Mathf.Lerp(ambientNight, ambientDay, dayFactor);
    }

    private void UpdateFog(float cloud01, float rain01, float dayFactor)
    {
        if (!controlFog || !RenderSettings.fog) return;

        float fog01 = Mathf.Clamp01(cloud01 * 0.7f + rain01 * 0.4f + (1f - dayFactor) * 0.2f);
        RenderSettings.fogDensity = Mathf.Lerp(fogMinDensity, fogMaxDensity, fog01);

        Color dayFog = new Color(0.7f, 0.78f, 0.86f);
        Color nightFog = new Color(0.08f, 0.1f, 0.16f);
        RenderSettings.fogColor = Color.Lerp(nightFog, dayFog, dayFactor);
    }

    private bool TryGetInterpolation(DateTime targetLocal, out int i0, out int i1, out float t)
    {
        i0 = 0;
        i1 = 0;
        t = 0f;

        if (!TryParseOpenMeteoTime(_data.hourly.time[0], out DateTime start)) return false;
        if (!TryParseOpenMeteoTime(_data.hourly.time[_data.hourly.time.Length - 1], out DateTime end)) return false;

        if (targetLocal <= start)
        {
            i0 = i1 = 0;
            return true;
        }

        if (targetLocal >= end)
        {
            i0 = i1 = _data.hourly.time.Length - 1;
            return true;
        }

        double totalMinutes = (targetLocal - start).TotalMinutes;
        double exactHour = totalMinutes / 60.0;

        i0 = Mathf.Clamp((int)Math.Floor(exactHour), 0, _data.hourly.time.Length - 1);
        i1 = Mathf.Clamp(i0 + 1, 0, _data.hourly.time.Length - 1);
        t = (float)(exactHour - Math.Floor(exactHour));

        return true;
    }

    public static bool TryParseOpenMeteoTime(string s, out DateTime dt)
    {
        return DateTime.TryParseExact(
            s,
            "yyyy-MM-dd'T'HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out dt
        );
    }

    private void SetMaterialAlpha(Material mat, float alpha)
    {
        if (mat == null) return;

        if (mat.HasProperty("_Color"))
        {
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }
        else if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        }
    }

    private void SetMaterialColor(Material mat, Color color)
    {
        if (mat == null) return;

        if (mat.HasProperty("_Color"))
            mat.color = color;
        else if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
    }

    private void SetMainTextureOffset(Material mat, Vector2 offset)
    {
        if (mat == null) return;

        if (mat.HasProperty("_MainTex"))
            mat.SetTextureOffset("_MainTex", offset);

        if (mat.HasProperty("_BaseMap"))
            mat.SetTextureOffset("_BaseMap", offset);
    }


}
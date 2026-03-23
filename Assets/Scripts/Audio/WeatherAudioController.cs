using UnityEngine;

public class WeatherAudioController : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource rainSoftSource;
    public AudioSource rainHeavySource;
    public AudioSource windSource;
    public AudioSource nightAmbienceSource;
    public AudioSource dayAmbienceSource;
    public AudioSource snowfallSource;
    public AudioSource blizzardSource;

    [Header("Ranges")]
    public float maxConsideredRain = 8f;
    public float maxConsideredWind = 40f;
    public float snowTemperatureThreshold = 1.5f;
    public float blizzardWindThreshold = 18f;

    [Header("Volumes")]
    public float rainSoftMaxVolume = 0.45f;
    public float rainHeavyMaxVolume = 0.7f;
    public float windMaxVolume = 0.5f;
    public float nightMaxVolume = 0.20f;
    public float dayMaxVolume = 0.15f;
    public float snowMaxVolume = 0.20f;
    public float blizzardMaxVolume = 0.15f;

    [Header("Smoothing")]
    public float volumeLerpSpeed = 3f;
    public float pitchLerpSpeed = 2f;

    [Header("Debug")]
    public bool useDebugInput = false;
    public float debugTemperature = 10f;
    public float debugPrecipitation = 0f;
    public float debugWindSpeed = 5f;
    public float debugSunAltitude = 20f;

    [Header("Runtime Debug")]
    public float debugRainSoftVolume;
    public float debugRainHeavyVolume;
    public float debugWindVolume;
    public float debugNightVolume;
    public float debugDayVolume;
    public float debugSnowVolume;
    public float debugBlizzardVolume;

    private float _targetRainSoft;
    private float _targetRainHeavy;
    private float _targetWind;
    private float _targetNight;
    private float _targetDay;
    private float _targetSnow;
    private float _targetBlizzard;

    private float _targetRainSoftPitch = 1f;
    private float _targetRainHeavyPitch = 1f;
    private float _targetWindPitch = 1f;
    private float _targetSnowPitch = 1f;
    private float _targetBlizzardPitch = 1f;

    private void Start()
    {
        EnsureLooping(rainSoftSource);
        EnsureLooping(rainHeavySource);
        EnsureLooping(windSource);
        EnsureLooping(nightAmbienceSource);
        EnsureLooping(dayAmbienceSource);
        EnsureLooping(snowfallSource);
        EnsureLooping(blizzardSource);

        StartIfValid(rainSoftSource);
        StartIfValid(rainHeavySource);
        StartIfValid(windSource);
        StartIfValid(nightAmbienceSource);
        StartIfValid(dayAmbienceSource);
        StartIfValid(snowfallSource);
        StartIfValid(blizzardSource);

        ForceSilentAtStart();
    }

    private void Update()
    {
        if (useDebugInput)
        {
            ApplyWeatherAudio(
                debugTemperature,
                debugPrecipitation,
                debugWindSpeed,
                debugSunAltitude
            );
        }

        UpdateSource(rainSoftSource, _targetRainSoft, _targetRainSoftPitch);
        UpdateSource(rainHeavySource, _targetRainHeavy, _targetRainHeavyPitch);
        UpdateSource(windSource, _targetWind, _targetWindPitch);
        UpdateSource(nightAmbienceSource, _targetNight, 1f);
        UpdateSource(dayAmbienceSource, _targetDay, 1f);
        UpdateSource(snowfallSource, _targetSnow, _targetSnowPitch);
        UpdateSource(blizzardSource, _targetBlizzard, _targetBlizzardPitch);

        debugRainSoftVolume = rainSoftSource != null ? rainSoftSource.volume : 0f;
        debugRainHeavyVolume = rainHeavySource != null ? rainHeavySource.volume : 0f;
        debugWindVolume = windSource != null ? windSource.volume : 0f;
        debugNightVolume = nightAmbienceSource != null ? nightAmbienceSource.volume : 0f;
        debugDayVolume = dayAmbienceSource != null ? dayAmbienceSource.volume : 0f;
        debugSnowVolume = snowfallSource != null ? snowfallSource.volume : 0f;
        debugBlizzardVolume = blizzardSource != null ? blizzardSource.volume : 0f;
    }

    public void ApplyWeatherAudio(float temperature, float precipitationMm, float windSpeed, float sunAltitudeDeg)
    {
        float precip01 = Mathf.Clamp01(precipitationMm / maxConsideredRain);
        float wind01 = Mathf.Clamp01(windSpeed / maxConsideredWind);

        bool isSnowing = precipitationMm > 0.05f && temperature <= snowTemperatureThreshold;
        bool isRaining = precipitationMm > 0.05f && temperature > snowTemperatureThreshold;
        bool isBlizzard = isSnowing && windSpeed >= blizzardWindThreshold;

        // Rain
        _targetRainSoft = isRaining
            ? Mathf.Lerp(0f, rainSoftMaxVolume, Mathf.SmoothStep(0f, 1f, precip01))
            : 0f;

        _targetRainHeavy = isRaining
            ? Mathf.Lerp(0f, rainHeavyMaxVolume, Mathf.SmoothStep(0.25f, 1f, precip01))
            : 0f;

        _targetRainSoftPitch = Mathf.Lerp(0.95f, 1.05f, precip01);
        _targetRainHeavyPitch = Mathf.Lerp(0.95f, 1.08f, precip01);

        // Wind
        _targetWind = Mathf.Lerp(0f, windMaxVolume, wind01);
        _targetWindPitch = Mathf.Lerp(0.9f, 1.1f, wind01);

        // Day / Night ambience
        float dayFactor = Mathf.InverseLerp(-6f, 10f, sunAltitudeDeg);
        float nightFactor = 1f - dayFactor;

        _targetNight = nightFactor * nightMaxVolume;
        _targetDay = dayFactor * dayMaxVolume;

        // Lower ambience under strong precipitation
        _targetDay *= Mathf.Lerp(1f, 0.2f, precip01);
        _targetNight *= Mathf.Lerp(1f, 0.5f, precip01);

        // Snow
        _targetSnow = isSnowing
            ? Mathf.Lerp(0f, snowMaxVolume, Mathf.SmoothStep(0f, 1f, precip01))
            : 0f;

        _targetSnowPitch = Mathf.Lerp(0.95f, 1.03f, precip01);

        // Blizzard
        float blizzardFactor = isBlizzard ? precip01 * wind01 : 0f;
        _targetBlizzard = Mathf.Lerp(0f, blizzardMaxVolume, blizzardFactor);
        _targetBlizzardPitch = Mathf.Lerp(0.95f, 1.05f, wind01);
    }

    private void UpdateSource(AudioSource source, float targetVolume, float targetPitch)
    {
        if (source == null) return;
                source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * volumeLerpSpeed);
        source.pitch = Mathf.Lerp(source.pitch, targetPitch, Time.deltaTime * pitchLerpSpeed);

        if (targetVolume > 0.001f)
        {
            if (!source.isPlaying)
                source.Play();
        }
        else
        {
            if (source.isPlaying && source.volume < 0.01f)
                source.Pause();
        }
    }

    private void EnsureLooping(AudioSource source)
    {
        if (source == null) return;
        source.loop = true;
        source.playOnAwake = false;
    }

    private void StartIfValid(AudioSource source)
    {
        if (source == null || source.clip == null) return;
        source.Play();
        source.Pause();
    }

    private void ForceSilentAtStart()
    {
        SetSilent(rainSoftSource);
        SetSilent(rainHeavySource);
        SetSilent(windSource);
        SetSilent(nightAmbienceSource);
        SetSilent(dayAmbienceSource);
        SetSilent(snowfallSource);
        SetSilent(blizzardSource);
    }

    private void SetSilent(AudioSource source)
    {
        if (source == null) return;
        source.volume = 0f;
        source.pitch = 1f;
    }
}
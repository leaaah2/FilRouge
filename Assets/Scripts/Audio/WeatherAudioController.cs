using UnityEngine;

public class WeatherAudioController : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource rainSoftSource;
    public AudioSource rainHeavySource;
    public AudioSource windSource;
    public AudioSource nightAmbienceSource;
    public AudioSource dayAmbienceSource;

    [Header("Ranges")]
    public float maxConsideredRain = 8f;
    public float maxConsideredWind = 40f;

    [Header("Volumes")]
    public float rainSoftMaxVolume = 0.45f;
    public float rainHeavyMaxVolume = 0.7f;
    public float windMaxVolume = 0.5f;
    public float nightMaxVolume = 0.20f;
    public float dayMaxVolume = 0.15f;

    [Header("Smoothing")]
    public float volumeLerpSpeed = 3f;
    public float pitchLerpSpeed = 2f;

    [Header("Debug")]
    public float debugRainSoftVolume;
    public float debugRainHeavyVolume;
    public float debugWindVolume;
    public float debugNightVolume;
    public float debugDayVolume;

    private float _targetRainSoft;
    private float _targetRainHeavy;
    private float _targetWind;
    private float _targetNight;
    private float _targetDay;

    private float _targetRainSoftPitch = 1f;
    private float _targetRainHeavyPitch = 1f;
    private float _targetWindPitch = 1f;

    private void Start()
    {
        EnsureLooping(rainSoftSource);
        EnsureLooping(rainHeavySource);
        EnsureLooping(windSource);
        EnsureLooping(nightAmbienceSource);
        EnsureLooping(dayAmbienceSource);

        StartIfValid(rainSoftSource);
        StartIfValid(rainHeavySource);
        StartIfValid(windSource);
        StartIfValid(nightAmbienceSource);
        StartIfValid(dayAmbienceSource);

        ForceSilentAtStart();
    }

    private void Update()
    {
        UpdateSource(rainSoftSource, _targetRainSoft, _targetRainSoftPitch);
        UpdateSource(rainHeavySource, _targetRainHeavy, _targetRainHeavyPitch);
        UpdateSource(windSource, _targetWind, _targetWindPitch);
        UpdateSource(nightAmbienceSource, _targetNight, 1f);
        UpdateSource(dayAmbienceSource, _targetDay, 1f);

        debugRainSoftVolume = rainSoftSource != null ? rainSoftSource.volume : 0f;
        debugRainHeavyVolume = rainHeavySource != null ? rainHeavySource.volume : 0f;
        debugWindVolume = windSource != null ? windSource.volume : 0f;
        debugNightVolume = nightAmbienceSource != null ? nightAmbienceSource.volume : 0f;
        debugDayVolume = dayAmbienceSource != null ? dayAmbienceSource.volume : 0f;
    }

    public void ApplyWeatherAudio(float precipitationMm, float windSpeed, float sunAltitudeDeg)
    {
        float rain01 = Mathf.Clamp01(precipitationMm / maxConsideredRain);
        float wind01 = Mathf.Clamp01(windSpeed / maxConsideredWind);

        _targetRainSoft = Mathf.Lerp(0f, rainSoftMaxVolume, Mathf.SmoothStep(0f, 1f, rain01));
        _targetRainHeavy = Mathf.Lerp(0f, rainHeavyMaxVolume, Mathf.SmoothStep(0f, 1f, rain01));

        _targetRainSoftPitch = Mathf.Lerp(0.95f, 1.05f, rain01);
        _targetRainHeavyPitch = Mathf.Lerp(0.95f, 1.08f, rain01);

        _targetWind = Mathf.Lerp(0f, windMaxVolume, wind01);
        _targetWindPitch = Mathf.Lerp(0.9f, 1.1f, wind01);

        float dayFactor = Mathf.InverseLerp(-6f, 10f, sunAltitudeDeg);
        float nightFactor = 1f - dayFactor;

        _targetNight = nightFactor * nightMaxVolume;
        _targetDay = dayFactor * dayMaxVolume;

        _targetDay *= Mathf.Lerp(1f, 0.2f, rain01);

        _targetNight *= Mathf.Lerp(1f, 0.5f, rain01);
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
    }

    private void SetSilent(AudioSource source)
    {
        if (source == null) return;
        source.volume = 0f;
        source.pitch = 1f;
    }
}
using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("References")]
    public Light fireLight;
    public ParticleSystem fireParticles;
    public ParticleSystem sparksParticles;
    public ParticleSystem smokeParticles;
    public AudioSource fireAudioSource;

    [Header("Conditions")]
    public float nightSunAltitudeThreshold = -2f;
    public float lightRainThreshold = 0.05f;
    public float heavyRainThreshold = 1.0f;

    [Header("Light")]
    public float lightOnIntensity = 2f;
    public float lightOffIntensity = 0f;
    public float lightLerpSpeed = 4f;

    [Header("Audio")]
    public float audioOnVolume = 0.6f;
    public float audioOffVolume = 0f;
    public float audioLerpSpeed = 4f;

    [Header("Transition")]
    public float effectBlendSpeed = 4f;

    [Header("Smoke / Smolder Multipliers")]
    public float smolderFireMultiplier = 0.08f;
    public float smolderSmokeMultiplier = 0.35f;
    public float heavyRainSmokeMultiplier = 0.12f;

    [Header("Debug")]
    public bool isActiveNow;

    private bool _targetActive;
    private float _fireEmissionCurrent;
    private float _fireEmissionTarget;

    private float _sparksEmissionCurrent;
    private float _sparksEmissionTarget;

    private float _smokeEmissionCurrent;
    private float _smokeEmissionTarget;

    private float _fireBaseRate;
    private float _sparksBaseRate;
    private float _smokeBaseRate;

    private void Awake()
    {
        _fireBaseRate = GetRate(fireParticles);
        _sparksBaseRate = GetRate(sparksParticles);
        _smokeBaseRate = GetRate(smokeParticles);
    }

    private void Start()
    {
        if (fireAudioSource != null)
        {
            fireAudioSource.loop = true;
            if (!fireAudioSource.isPlaying && fireAudioSource.clip != null)
                fireAudioSource.Play();
            fireAudioSource.volume = 0f;
        }

        if (fireLight != null)
            fireLight.intensity = lightOffIntensity;

        ForceImmediateState(false);
    }

    private void Update()
    {
        isActiveNow = _targetActive;

        _fireEmissionCurrent = Mathf.Lerp(_fireEmissionCurrent, _fireEmissionTarget, Time.deltaTime * effectBlendSpeed);
        _sparksEmissionCurrent = Mathf.Lerp(_sparksEmissionCurrent, _sparksEmissionTarget, Time.deltaTime * effectBlendSpeed);
        _smokeEmissionCurrent = Mathf.Lerp(_smokeEmissionCurrent, _smokeEmissionTarget, Time.deltaTime * effectBlendSpeed);

        SetRate(fireParticles, _fireEmissionCurrent);
        SetRate(sparksParticles, _sparksEmissionCurrent);
        SetRate(smokeParticles, _smokeEmissionCurrent);

        SetPlaying(fireParticles, _fireEmissionCurrent > 0.05f);
        SetPlaying(sparksParticles, _sparksEmissionCurrent > 0.05f);
        SetPlaying(smokeParticles, _smokeEmissionCurrent > 0.05f);

        if (fireLight != null)
        {
            float targetLight = _fireEmissionTarget > (_fireBaseRate * 0.15f) ? lightOnIntensity : lightOffIntensity;
            fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetLight, Time.deltaTime * lightLerpSpeed);
            fireLight.enabled = fireLight.intensity > 0.01f;
        }

        if (fireAudioSource != null)
        {
            float fireNormalized = _fireBaseRate > 0.001f ? (_fireEmissionTarget / _fireBaseRate) : 0f;
            float targetVol = Mathf.Lerp(audioOffVolume, audioOnVolume, fireNormalized);

            fireAudioSource.volume = Mathf.Lerp(fireAudioSource.volume, targetVol, Time.deltaTime * audioLerpSpeed);

            if (targetVol > 0.01f)
            {
                if (!fireAudioSource.isPlaying && fireAudioSource.clip != null)
                    fireAudioSource.Play();
            }
            else
            {
                if (fireAudioSource.isPlaying && fireAudioSource.volume < 0.01f)
                    fireAudioSource.Pause();
            }
        }
    }

    public void ApplyWeather(float sunAltitudeDeg, float precipitationMm)
    {
        bool isNight = sunAltitudeDeg <= nightSunAltitudeThreshold;
        bool lightRain = precipitationMm > lightRainThreshold && precipitationMm <= heavyRainThreshold;
        bool heavyRain = precipitationMm > heavyRainThreshold;

        if (isNight && !lightRain && !heavyRain)
        {
            // Burning
            _targetActive = true;

            _fireEmissionTarget = _fireBaseRate;
            _sparksEmissionTarget = _sparksBaseRate;
            _smokeEmissionTarget = _smokeBaseRate;
        }
        else if ((isNight && lightRain) || (!isNight && !heavyRain))
        {
            // Smoldering
            _targetActive = false;

            _fireEmissionTarget = _fireBaseRate * smolderFireMultiplier;
            _sparksEmissionTarget = 0f;
            _smokeEmissionTarget = _smokeBaseRate * smolderSmokeMultiplier;
        }
        else
        {
            // Heavy rain / mostly extinguished
            _targetActive = false;

            _fireEmissionTarget = 0f;
            _sparksEmissionTarget = 0f;
            _smokeEmissionTarget = _smokeBaseRate * heavyRainSmokeMultiplier;
        }
    }

    private void ForceImmediateState(bool active)
    {
        _targetActive = active;

        _fireEmissionCurrent = active ? _fireBaseRate : 0f;
        _sparksEmissionCurrent = active ? _sparksBaseRate : 0f;
        _smokeEmissionCurrent = active ? _smokeBaseRate : 0f;

        _fireEmissionTarget = _fireEmissionCurrent;
        _sparksEmissionTarget = _sparksEmissionCurrent;
        _smokeEmissionTarget = _smokeEmissionCurrent;

        SetRate(fireParticles, _fireEmissionCurrent);
        SetRate(sparksParticles, _sparksEmissionCurrent);
        SetRate(smokeParticles, _smokeEmissionCurrent);

        SetPlaying(fireParticles, active);
        SetPlaying(sparksParticles, active);
        SetPlaying(smokeParticles, active);
    }

    private float GetRate(ParticleSystem ps)
    {
        if (ps == null) return 0f;
        var emission = ps.emission;
        return emission.rateOverTime.constant;
    }

    private void SetRate(ParticleSystem ps, float rate)
    {
        if (ps == null) return;
        var emission = ps.emission;
        emission.rateOverTime = rate;
    }

    private void SetPlaying(ParticleSystem ps, bool shouldPlay)
    {
        if (ps == null) return;

        if (shouldPlay)
        {
            if (!ps.isPlaying)
                ps.Play();
        }
        else
        {
            if (ps.isPlaying)
                ps.Stop();
        }
    }
}
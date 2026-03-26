using UnityEngine;

public class WeatherPrecipitationController : MonoBehaviour
{
    [Header("References")]
    public Transform followTarget;
    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;

    [Header("Emission")]
    public float rainMaxRate = 800f;
    public float snowMaxRate = 1200f;
    public float precipitationToMaxRate = 8f;

    [Header("Rain / Snow Blend")]
    public float fullSnowTemperature = -1.5f;
    public float fullRainTemperature = 2.5f;

    [Header("Follow")]
    public Vector3 positionOffset = new Vector3(0f, 8f, 0f);
    public bool followY = true;

    [Header("Wind")]
    public float windInfluence = 0.5f;

    [Header("Smoothing")]
    public float emissionBlendSpeed = 5f;

    public RainWaterLevelController rainWaterLevelController;

    private float _currentRainRate;
    private float _currentSnowRate;
    private float _targetRainRate;
    private float _targetSnowRate;

    private float _currentRainWindSpeed;
    private float _currentSnowWindSpeed;
    private float _targetRainWindSpeed;
    private float _targetSnowWindSpeed;
    private float _currentWindDirection;
    private float _targetWindDirection;

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            Vector3 p = followTarget.position + positionOffset;

            if (!followY)
                p.y = transform.position.y;

            transform.position = p;
        }

        // Smooth precipitation rates
        _currentRainRate = Mathf.Lerp(_currentRainRate, _targetRainRate, Time.deltaTime * emissionBlendSpeed);
        _currentSnowRate = Mathf.Lerp(_currentSnowRate, _targetSnowRate, Time.deltaTime * emissionBlendSpeed);

        // Smooth wind
        _currentRainWindSpeed = Mathf.Lerp(_currentRainWindSpeed, _targetRainWindSpeed, Time.deltaTime * emissionBlendSpeed);
        _currentSnowWindSpeed = Mathf.Lerp(_currentSnowWindSpeed, _targetSnowWindSpeed, Time.deltaTime * emissionBlendSpeed);
        _currentWindDirection = Mathf.LerpAngle(_currentWindDirection, _targetWindDirection, Time.deltaTime * emissionBlendSpeed);

        SetEmission(rainParticles, _currentRainRate);
        SetEmission(snowParticles, _currentSnowRate);

        SetPlaying(rainParticles, _currentRainRate > 0.5f);
        SetPlaying(snowParticles, _currentSnowRate > 0.5f);

        ApplyWind(rainParticles, _currentRainWindSpeed, _currentWindDirection);
        ApplyWind(snowParticles, _currentSnowWindSpeed, _currentWindDirection);
    }

    public void ApplyClimate(float temperatureC, float precipitationMm, float windSpeed, float windDirection)
    {
        float precip01 = Mathf.Clamp01(precipitationMm / precipitationToMaxRate);

        if (precipitationMm <= 0.05f)
        {
            _targetRainRate = 0f;
            _targetSnowRate = 0f;
            _targetRainWindSpeed = 0f;
            _targetSnowWindSpeed = 0f;
            _targetWindDirection = windDirection;

            if (rainWaterLevelController != null)
                rainWaterLevelController.ApplyClimate(temperatureC, precipitationMm);

            return;
        }

        // 0 = full snow, 1 = full rain
        float rainBlend = Mathf.InverseLerp(fullSnowTemperature, fullRainTemperature, temperatureC);
        float snowBlend = 1f - rainBlend;

        _targetRainRate = precip01 * rainMaxRate * rainBlend;
        _targetSnowRate = precip01 * snowMaxRate * snowBlend;

        _targetRainWindSpeed = windSpeed;
        _targetSnowWindSpeed = windSpeed * 0.3f;
        _targetWindDirection = windDirection;

        if (rainWaterLevelController != null)
            rainWaterLevelController.ApplyClimate(temperatureC, precipitationMm);
    }

    private void SetEmission(ParticleSystem ps, float rate)
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

    void ApplyWind(ParticleSystem ps, float windSpeed, float windDirectionDeg)
    {
        if (ps == null) return;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;

        float rad = windDirectionDeg * Mathf.Deg2Rad;
        Vector3 windDir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

        Vector3 wind = windDir * windSpeed * windInfluence;
        float variation = 0.1f;

        vel.x = new ParticleSystem.MinMaxCurve(wind.x - variation, wind.x + variation);
        vel.z = new ParticleSystem.MinMaxCurve(wind.z - variation, wind.z + variation);
    }
}
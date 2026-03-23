using UnityEngine;

public class WeatherPrecipitationController : MonoBehaviour
{
    [Header("References")]
    public Transform followTarget;
    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;

    [Header("Emission")]
    public float rainMaxRate = 800f;
    public float snowMaxRate = 400f;
    public float precipitationToMaxRate = 8f;

    [Header("Snow Threshold")]
    public float snowTemperatureThreshold = 1.5f;

    [Header("Follow")]
    public Vector3 positionOffset = new Vector3(0f, 8f, 0f);
    public bool followY = true;

    [Header("Wind")]
    public float windInfluence = 0.5f;

    public RainWaterLevelController rainWaterLevelController;

    private void LateUpdate()
    {
        if (followTarget == null) return;

        Vector3 p = followTarget.position + positionOffset;

        if (!followY)
            p.y = transform.position.y;


        transform.position = p;
    }

    public void ApplyClimate(float temperatureC, float precipitationMm, float windSpeed, float windDirection)
    {
        float precip01 = Mathf.Clamp01(precipitationMm / precipitationToMaxRate);

        bool shouldSnow = precipitationMm > 0.05f && temperatureC <= snowTemperatureThreshold;
        bool shouldRain = precipitationMm > 0.05f && temperatureC > snowTemperatureThreshold;


        SetEmission(rainParticles, shouldRain ? precip01 * rainMaxRate : 0f);
        SetEmission(snowParticles, shouldSnow ? precip01 * snowMaxRate : 0f);

        SetPlaying(rainParticles, shouldRain);
        SetPlaying(snowParticles, shouldSnow);

        ApplyWind(rainParticles, windSpeed, windDirection);
        ApplyWind(snowParticles, windSpeed * 0.3f, windDirection);

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
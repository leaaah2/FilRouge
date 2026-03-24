using UnityEngine;

public class ColdBreathController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem breathParticles;

    [Header("Temperature Blend")]
    public float fullBreathTemperature = -5f;
    public float noBreathTemperature = 6f;

    [Header("Emission")]
    public float maxEmissionRate = 12f;
    public float emissionBlendSpeed = 4f;

    [Header("Burst Feel")]
    public bool pulseInsteadOfConstant = true;
    public float pulseFrequency = 0.35f;
    public float pulseStrength = 1f;

    private float _targetEmission;
    private float _currentEmission;
    private float _time;

    private void Update()
    {
        _time += Time.deltaTime;

        _currentEmission = Mathf.Lerp(_currentEmission, _targetEmission, Time.deltaTime * emissionBlendSpeed);

        float finalRate = _currentEmission;

        if (pulseInsteadOfConstant && finalRate > 0.01f)
        {
            float pulse = Mathf.Clamp01(Mathf.Sin(_time * pulseFrequency * Mathf.PI * 2f) * 0.5f + 0.5f);
            pulse = Mathf.Lerp(0.35f, 1f, pulse * pulseStrength);
            finalRate *= pulse;
        }

        SetEmissionRate(finalRate);
        SetPlaying(finalRate > 0.05f);
    }

    public void ApplyTemperature(float temperatureC)
    {
        // colder = more visible breath
        float breath01 = 1f - Mathf.InverseLerp(fullBreathTemperature, noBreathTemperature, temperatureC);
        breath01 = Mathf.Clamp01(breath01);

        _targetEmission = breath01 * maxEmissionRate;
    }

    private void SetEmissionRate(float rate)
    {
        if (breathParticles == null) return;

        var emission = breathParticles.emission;
        emission.rateOverTime = rate;
    }

    private void SetPlaying(bool shouldPlay)
    {
        if (breathParticles == null) return;

        if (shouldPlay)
        {
            if (!breathParticles.isPlaying)
                breathParticles.Play();
        }
        else
        {
            if (breathParticles.isPlaying)
                breathParticles.Stop();
        }
    }
}
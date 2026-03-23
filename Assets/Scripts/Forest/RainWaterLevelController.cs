using UnityEngine;

public class RainWaterLevelController : MonoBehaviour
{
    public Transform waterTransform;

    [Header("Height")]
    public float dryY = -0.9f;
    public float wetY = -0.4f;
    public float moveSpeed = 1.5f;

    [Header("Accumulation")]
    [Range(0f, 1f)] public float waterAmount = 0f;
    public float accumulateSpeed = 0.08f;
    public float drainSpeed = 0.03f;
    public float frozenDrainMultiplier = 0f;

    [Header("Temperature")]
    public float freezeTemperature = 0f;

    [Header("Rain Input")]
    public float maxConsideredPrecipitation = 8f;
    public float minRainThreshold = 0.05f;

    [Header("Debug")]
    public bool logChanges = false;

    private float _currentTemperature = 10f;
    private float _currentPrecipitation = 0f;

    private void Update()
    {
        if (waterTransform == null) return;

        bool raining = _currentPrecipitation > minRainThreshold;
        bool frozen = _currentTemperature <= freezeTemperature;

        float precip01 = Mathf.Clamp01(_currentPrecipitation / maxConsideredPrecipitation);

        // Only liquid rain increases puddles
        if (raining && !frozen)
        {
            waterAmount += accumulateSpeed * precip01 * Time.deltaTime;
        }
        else
        {
            float drain = drainSpeed;

            // Frozen puddles barely drain or stop draining
            if (frozen)
                drain *= frozenDrainMultiplier;

            waterAmount -= drain * Time.deltaTime;
        }

        waterAmount = Mathf.Clamp01(waterAmount);

        float targetY = Mathf.Lerp(dryY, wetY, waterAmount);

        Vector3 p = waterTransform.position;
        p.y = Mathf.Lerp(p.y, targetY, Time.deltaTime * moveSpeed);
        waterTransform.position = p;
    }

    public void ApplyClimate(float temperatureC, float precipitationMm)
    {
        _currentTemperature = temperatureC;
        _currentPrecipitation = precipitationMm;
    }

    public void SetWaterAmount(float value01)
    {
        waterAmount = Mathf.Clamp01(value01);
    }

    public void ResetToDry()
    {
        waterAmount = 0f;

        if (waterTransform != null)
        {
            Vector3 p = waterTransform.position;
            p.y = dryY;
            waterTransform.position = p;
        }
    }
}
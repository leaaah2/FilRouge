using UnityEngine;

public class RainWaterLevelController : MonoBehaviour
{
    public Transform waterTransform;

    [Header("Height")]
    public float dryY = -0.9f;
    public float wetY = -0.4f;
    public float riseSpeed = 1.5f;

    [Header("Input")]
    public float maxConsideredPrecipitation = 8f;

    private float _targetY;

    private void Awake()
    {
        _targetY = dryY;
    }

    private void Update()
    {
        if (waterTransform == null) return;

        Vector3 p = waterTransform.position;
        p.y = Mathf.Lerp(p.y, _targetY, Time.deltaTime * riseSpeed);
        waterTransform.position = p;
    }

    public void ApplyPrecipitation(float precipitationMm)
    {
        float t = Mathf.Clamp01(precipitationMm / maxConsideredPrecipitation);
        _targetY = Mathf.Lerp(dryY, wetY, t);
    }
}
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ClimateScatterRule
{
    public string name = "Rule";

    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Temperature Range")]
    public float minTemperature = -100f;
    public float maxTemperature = 100f;

    [Header("Humidity Range")]
    [Range(0f, 100f)] public float minHumidity = 0f;
    [Range(0f, 100f)] public float maxHumidity = 100f;

    [Header("Spawn Control")]
    [Range(0f, 1f)] public float spawnChance = 1f;
    public float weight = 1f;

    [Header("Scale Override")]
    public bool overrideScaleRange = false;
    public Vector2 scaleMultiplierRange = new Vector2(1f, 1f);
}

[ExecuteAlways]
public class ClimateScatter : MonoBehaviour
{
    public enum DistributionMode
    {
        Random,
        PoissonDisc
    }

    [Header("Climate Input")]
    public float currentTemperature = 15f;
    [Range(0f, 100f)] public float currentHumidity = 60f;

    [Header("Rules")]
    public ClimateScatterRule[] rules;

    [Header("Spawn Area")]
    public Vector2 areaSize = new Vector2(20f, 20f);
    public float rayStartHeight = 50f;
    public LayerMask includeMask;
    public LayerMask excludeMask;

    [Header("Amount")]
    public int baseCount = 100;
    public bool scaleCountWithHumidity = true;
    public Vector2 humidityCountMultiplier = new Vector2(0.2f, 1.5f);

    [Header("Distribution")]
    public DistributionMode distribution = DistributionMode.Random;
    public float minDistance = 2f;

    [Header("Donut Distribution")]
    public bool useDonut = false;
    public float innerRadius = 10f;
    public float outerRadius = 25f;

    [Header("Placement Rules")]
    [Range(0f, 90f)] public float maxSlopeAngle = 30f;
    public bool alignToNormal = false;
    public bool randomYRotation = true;
    public Vector2 globalScaleMultiplierRange = new Vector2(1f, 1f);

    [Header("Deterministic")]
    public int seed = 12345;

    [Header("Parenting")]
    public Transform container;
    public string spawnedContainerName = "Spawned";

    public void Generate()
    {
        if (rules == null || rules.Length == 0)
        {
            Debug.LogWarning($"[{name}] No climate rules assigned.");
            return;
        }

        List<ClimateScatterRule> validRules = GetValidRules();
        if (validRules.Count == 0)
        {
            Debug.LogWarning($"[{name}] No valid rules for temperature={currentTemperature}, humidity={currentHumidity}");
            return;
        }

        Transform parent = GetOrCreateSpawnedContainer();

        Random.State oldState = Random.state;
        Random.InitState(seed);

        int effectiveCount = GetAdjustedCount();

        List<Vector2> points = distribution == DistributionMode.PoissonDisc
            ? GeneratePoissonPoints(effectiveCount)
            : GenerateRandomPoints(effectiveCount);

        int spawnedCount = 0;

        foreach (Vector2 p in points)
        {
            Vector3 rayOrigin = transform.TransformPoint(new Vector3(p.x, rayStartHeight, p.y));

            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayStartHeight * 2f, includeMask))
                continue;

            if (excludeMask.value != 0)
            {
                int hitLayer = 1 << hit.collider.gameObject.layer;
                if ((excludeMask.value & hitLayer) != 0)
                    continue;
            }

            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > maxSlopeAngle)
                continue;

            ClimateScatterRule rule = PickRule(validRules);
            if (rule == null)
                continue;

            if (rule.prefabs == null || rule.prefabs.Length == 0)
                continue;

            if (Random.value > rule.spawnChance)
                continue;

            GameObject prefab = rule.prefabs[Random.Range(0, rule.prefabs.Length)];
            if (prefab == null)
                continue;

#if UNITY_EDITOR
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(obj, "Climate Scatter Spawn");
#else
            GameObject obj = Instantiate(prefab, parent);
#endif

            obj.transform.position = hit.point;

            if (alignToNormal)
            {
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);

                if (randomYRotation)
                    rot = Quaternion.AngleAxis(Random.Range(0f, 360f), hit.normal) * rot;

                obj.transform.rotation = rot;
            }
            else
            {
                obj.transform.rotation = Quaternion.Euler(
                    0f,
                    randomYRotation ? Random.Range(0f, 360f) : 0f,
                    0f
                );
            }

            Vector2 scaleRange = rule.overrideScaleRange ? rule.scaleMultiplierRange : globalScaleMultiplierRange;
            float scaleMult = Random.Range(scaleRange.x, scaleRange.y);
            obj.transform.localScale = prefab.transform.localScale * scaleMult;

            spawnedCount++;
        }

        Random.state = oldState;

        Debug.Log($"[{name}] Climate scatter generated {spawnedCount} objects. T={currentTemperature}, H={currentHumidity}");
    }

    public void Clear()
    {
        if (container == null)
            return;

        Transform spawned = container.Find(spawnedContainerName);
        if (spawned == null)
            return;

        for (int i = spawned.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(spawned.GetChild(i).gameObject);
#else
            DestroyImmediate(spawned.GetChild(i).gameObject);
#endif
        }
    }

    public void SetClimate(float temperature, float humidity)
    {
        currentTemperature = temperature;
        currentHumidity = Mathf.Clamp(humidity, 0f, 100f);
    }

    public int GetAdjustedCount()
    {
        if (!scaleCountWithHumidity)
            return baseCount;

        float humidity01 = Mathf.Clamp01(currentHumidity / 100f);
        float mult = Mathf.Lerp(humidityCountMultiplier.x, humidityCountMultiplier.y, humidity01);
        return Mathf.Max(1, Mathf.RoundToInt(baseCount * mult));
    }

    private List<ClimateScatterRule> GetValidRules()
    {
        List<ClimateScatterRule> valid = new();

        foreach (ClimateScatterRule rule in rules)
        {
            if (rule == null)
                continue;

            if (rule.prefabs == null || rule.prefabs.Length == 0)
                continue;

            if (currentTemperature < rule.minTemperature || currentTemperature > rule.maxTemperature)
                continue;

            if (currentHumidity < rule.minHumidity || currentHumidity > rule.maxHumidity)
                continue;

            valid.Add(rule);
        }

        return valid;
    }

    private ClimateScatterRule PickRule(List<ClimateScatterRule> validRules)
    {
        float totalWeight = 0f;

        foreach (ClimateScatterRule rule in validRules)
            totalWeight += Mathf.Max(0f, rule.weight);

        if (totalWeight <= 0f)
            return null;

        float pick = Random.Range(0f, totalWeight);
        float running = 0f;

        foreach (ClimateScatterRule rule in validRules)
        {
            running += Mathf.Max(0f, rule.weight);
            if (pick <= running)
                return rule;
        }

        return validRules[validRules.Count - 1];
    }

    private List<Vector2> GenerateRandomPoints(int targetCount)
    {
        List<Vector2> points = new();
        int attempts = 0;
        int maxAttempts = targetCount * 20;

        while (points.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;

            Vector2 p = SamplePoint();
            bool ok = true;

            if (minDistance > 0f)
            {
                foreach (Vector2 q in points)
                {
                    if ((p - q).sqrMagnitude < minDistance * minDistance)
                    {
                        ok = false;
                        break;
                    }
                }
            }

            if (ok)
                points.Add(p);
        }

        return points;
    }

    private List<Vector2> GeneratePoissonPoints(int targetCount)
    {
        List<Vector2> points = new();
        List<Vector2> active = new();

        Vector2 first = SamplePoint();
        points.Add(first);
        active.Add(first);

        const int k = 30;

        while (active.Count > 0 && points.Count < targetCount)
        {
            int idx = Random.Range(0, active.Count);
            Vector2 center = active[idx];
            bool found = false;

            for (int i = 0; i < k; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(minDistance, minDistance * 2f);
                Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (!IsInsideArea(candidate))
                    continue;

                bool ok = true;
                foreach (Vector2 p in points)
                {
                    if ((candidate - p).sqrMagnitude < minDistance * minDistance)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    points.Add(candidate);
                    active.Add(candidate);
                    found = true;
                    break;
                }
            }

            if (!found)
                active.RemoveAt(idx);
        }

        return points;
    }

    private Vector2 SamplePoint()
    {
        if (useDonut)
        {
            float r = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));
            float a = Random.Range(0f, Mathf.PI * 2f);
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        }

        return new Vector2(
            Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f)
        );
    }

    private bool IsInsideArea(Vector2 p)
    {
        if (useDonut)
        {
            float d = p.magnitude;
            return d >= innerRadius && d <= outerRadius;
        }

        return Mathf.Abs(p.x) <= areaSize.x * 0.5f &&
               Mathf.Abs(p.y) <= areaSize.y * 0.5f;
    }

    private Transform GetOrCreateSpawnedContainer()
    {
        if (container == null)
            container = transform;

        Transform existing = container.Find(spawnedContainerName);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(spawnedContainerName);

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(go, "Create Climate Scatter Container");
#endif

        go.transform.SetParent(container, false);
        return go.transform;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ClimateScatter))]
public class ClimateScatterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClimateScatter s = (ClimateScatter)target;

        GUILayout.Space(8);

        if (GUILayout.Button("Generate (One-time)"))
            s.Generate();

        if (GUILayout.Button("Clear Spawned"))
            s.Clear();

        GUILayout.Space(8);

        if (GUILayout.Button("Dry Test"))
        {
            s.SetClimate(18f, 20f);
            EditorUtility.SetDirty(s);
        }

        if (GUILayout.Button("Neutral Test"))
        {
            s.SetClimate(14f, 50f);
            EditorUtility.SetDirty(s);
        }

        if (GUILayout.Button("Wet Test"))
        {
            s.SetClimate(14f, 85f);
            EditorUtility.SetDirty(s);
        }

        if (GUILayout.Button("Cold Wet Test"))
        {
            s.SetClimate(4f, 90f);
            EditorUtility.SetDirty(s);
        }
    }
}
#endif
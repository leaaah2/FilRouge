using System.Collections;
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

    [Header("Transitions")]
    public float growDuration = 0.35f;
    public float shrinkDuration = 0.25f;
    public bool animateAtRuntime = true;

    [Header("Deterministic")]
    public int seed = 12345;

    [Header("Layout Variation")]
    public bool rotateScatterBeforeRebuild = true;
    public Vector2 randomYRotationRange = new Vector2(-20f, 20f);

    [Header("Parenting")]
    public Transform container;
    public string spawnedContainerName = "Spawned";

    [System.Serializable]
    private class ScatterPointState
    {
        public Vector3 position;
        public Vector3 normal;
        public Quaternion rotation;
        public float scaleMultiplier;

        public GameObject currentInstance;
        public GameObject currentPrefab;
        public Coroutine transitionRoutine;
    }

    private readonly List<ScatterPointState> _points = new();
    private bool _pointsBuilt = false;

    private string _lastRuleSignature = "";
    private bool _hasAppliedClimateOnce = false;

    public void SetClimate(float temperature, float humidity)
    {
        currentTemperature = temperature;
        currentHumidity = Mathf.Clamp(humidity, 0f, 100f);
    }

    public void ApplyClimateAndRefresh(float temperature, float humidity)
    {
        SetClimate(temperature, humidity);
        EnsurePointsBuilt();
        RefreshPoints();
    }

    public void EnsurePointsBuilt()
    {
        if (_pointsBuilt && _points.Count > 0)
            return;

        BuildSpawnPoints();
    }

    public void BuildSpawnPoints()
    {
        ClearSpawnedInstances();
        _points.Clear();

        int targetCount = GetPointCapacity();
        if (targetCount <= 0)
            return;

        var oldState = Random.state;
        Random.InitState(seed);

        List<Vector2> points2D = distribution == DistributionMode.PoissonDisc
            ? GeneratePoissonPoints(targetCount)
            : GenerateRandomPoints(targetCount);

        foreach (Vector2 p in points2D)
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

            Quaternion rotation;
            if (alignToNormal)
            {
                rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                if (randomYRotation)
                    rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), hit.normal) * rotation;
            }
            else
            {
                rotation = Quaternion.Euler(0f, randomYRotation ? Random.Range(0f, 360f) : 0f, 0f);
            }

            float scaleMult = Random.Range(globalScaleMultiplierRange.x, globalScaleMultiplierRange.y);

            _points.Add(new ScatterPointState
            {
                position = hit.point,
                normal = hit.normal,
                rotation = rotation,
                scaleMultiplier = scaleMult
            });
        }

        Random.state = oldState;
        _pointsBuilt = true;

        Debug.Log($"[{name}] Built {_points.Count} scatter points.");
    }

    public void RefreshPoints()
    {
        EnsurePointsBuilt();

        List<ClimateScatterRule> validRules = GetValidRules();
        float activeRatio = GetActiveRatio();

        for (int i = 0; i < _points.Count; i++)
        {
            ScatterPointState point = _points[i];

            bool shouldOccupy = Hash01(i, 11) <= activeRatio;
            GameObject desiredPrefab = null;
            ClimateScatterRule chosenRule = null;

            if (shouldOccupy && validRules.Count > 0)
            {
                chosenRule = PickRuleDeterministic(validRules, i);
                if (chosenRule != null && chosenRule.prefabs != null && chosenRule.prefabs.Length > 0)
                {
                    float spawnRoll = Hash01(i, 23);
                    if (spawnRoll <= chosenRule.spawnChance)
                    {
                        desiredPrefab = PickPrefabDeterministic(chosenRule, i);
                    }
                }
            }

            ApplyPointTarget(point, desiredPrefab, chosenRule);
        }
    }

    public void ClearSpawnedInstances()
    {
        foreach (ScatterPointState point in _points)
        {
            if (point.transitionRoutine != null && Application.isPlaying)
                StopCoroutine(point.transitionRoutine);

            if (point.currentInstance != null)
                DestroyScatterObject(point.currentInstance);

            point.currentInstance = null;
            point.currentPrefab = null;
            point.transitionRoutine = null;
        }

        if (container == null) return;
        Transform spawned = container.Find(spawnedContainerName);
        if (spawned == null) return;

        for (int i = spawned.childCount - 1; i >= 0; i--)
        {
            DestroyScatterObject(spawned.GetChild(i).gameObject);
        }
    }

    public void ClearAll()
    {
        ClearSpawnedInstances();
        _points.Clear();
        _pointsBuilt = false;
    }

    private void ApplyPointTarget(ScatterPointState point, GameObject desiredPrefab, ClimateScatterRule rule)
    {
        if (desiredPrefab == point.currentPrefab && point.currentInstance != null)
            return;

        Vector2 scaleRange = rule != null && rule.overrideScaleRange
            ? rule.scaleMultiplierRange
            : globalScaleMultiplierRange;

        float ruleScaleMult = Mathf.Lerp(scaleRange.x, scaleRange.y, Hash01(GetPointStableId(point), 99));
        Vector3 targetScale = desiredPrefab != null
            ? desiredPrefab.transform.localScale * ruleScaleMult
            : Vector3.zero;

        if (!Application.isPlaying || !animateAtRuntime)
        {
            ReplaceImmediate(point, desiredPrefab, targetScale);
            return;
        }

        if (point.transitionRoutine != null)
            StopCoroutine(point.transitionRoutine);

        point.transitionRoutine = StartCoroutine(ReplaceAnimated(point, desiredPrefab, targetScale));
    }

    private void ReplaceImmediate(ScatterPointState point, GameObject desiredPrefab, Vector3 targetScale)
    {
        if (point.currentInstance != null)
            DestroyScatterObject(point.currentInstance);

        point.currentInstance = null;
        point.currentPrefab = null;

        if (desiredPrefab == null)
            return;

        GameObject obj = CreateScatterObject(desiredPrefab, GetOrCreateSpawnedContainer());
        obj.transform.position = point.position;
        obj.transform.rotation = point.rotation;
        obj.transform.localScale = targetScale;

        point.currentInstance = obj;
        point.currentPrefab = desiredPrefab;
    }

    private IEnumerator ReplaceAnimated(ScatterPointState point, GameObject desiredPrefab, Vector3 targetScale)
    {
        if (point.currentInstance != null)
        {
            yield return ScaleObject(point.currentInstance.transform, point.currentInstance.transform.localScale, Vector3.zero, shrinkDuration);
            DestroyScatterObject(point.currentInstance);
            point.currentInstance = null;
            point.currentPrefab = null;
        }

        if (desiredPrefab != null)
        {
            GameObject obj = CreateScatterObject(desiredPrefab, GetOrCreateSpawnedContainer());
            obj.transform.position = point.position;
            obj.transform.rotation = point.rotation;
            obj.transform.localScale = Vector3.zero;

            point.currentInstance = obj;
            point.currentPrefab = desiredPrefab;

            yield return ScaleObject(obj.transform, Vector3.zero, targetScale, growDuration);
        }

        point.transitionRoutine = null;
    }

    private IEnumerator ScaleObject(Transform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null)
            yield break;

        if (duration <= 0.0001f)
        {
            target.localScale = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            if (target == null)
                yield break;

            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            a = a * a * (3f - 2f * a); // smoothstep
            target.localScale = Vector3.LerpUnclamped(from, to, a);
            yield return null;
        }

        if (target != null)
            target.localScale = to;
    }

    private List<ClimateScatterRule> GetValidRules()
    {
        List<ClimateScatterRule> valid = new();

        if (rules == null)
            return valid;

        foreach (ClimateScatterRule rule in rules)
        {
            if (rule == null || rule.prefabs == null || rule.prefabs.Length == 0)
                continue;

            if (currentTemperature < rule.minTemperature || currentTemperature > rule.maxTemperature)
                continue;

            if (currentHumidity < rule.minHumidity || currentHumidity > rule.maxHumidity)
                continue;

            valid.Add(rule);
        }

        return valid;
    }

    private ClimateScatterRule PickRuleDeterministic(List<ClimateScatterRule> validRules, int pointIndex)
    {
        float totalWeight = 0f;
        foreach (ClimateScatterRule rule in validRules)
            totalWeight += Mathf.Max(0f, rule.weight);

        if (totalWeight <= 0f)
            return null;

        float pick = Hash01(pointIndex, 37) * totalWeight;
        float running = 0f;

        foreach (ClimateScatterRule rule in validRules)
        {
            running += Mathf.Max(0f, rule.weight);
            if (pick <= running)
                return rule;
        }

        return validRules[validRules.Count - 1];
    }

    private GameObject PickPrefabDeterministic(ClimateScatterRule rule, int pointIndex)
    {
        if (rule.prefabs == null || rule.prefabs.Length == 0)
            return null;

        int idx = Mathf.FloorToInt(Hash01(pointIndex, 53) * rule.prefabs.Length);
        idx = Mathf.Clamp(idx, 0, rule.prefabs.Length - 1);
        return rule.prefabs[idx];
    }

    private int GetPointCapacity()
    {
        float mult = scaleCountWithHumidity
            ? Mathf.Max(humidityCountMultiplier.x, humidityCountMultiplier.y)
            : 1f;

        return Mathf.Max(1, Mathf.CeilToInt(baseCount * Mathf.Max(1f, mult)));
    }

    private float GetActiveRatio()
    {
        if (_points.Count == 0)
            return 0f;

        int targetActiveCount = baseCount;

        if (scaleCountWithHumidity)
        {
            float humidity01 = Mathf.Clamp01(currentHumidity / 100f);
            float mult = Mathf.Lerp(humidityCountMultiplier.x, humidityCountMultiplier.y, humidity01);
            targetActiveCount = Mathf.RoundToInt(baseCount * mult);
        }

        return Mathf.Clamp01((float)targetActiveCount / _points.Count);
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
        if (!Application.isPlaying)
            Undo.RegisterCreatedObjectUndo(go, "Create Climate Scatter Animated Container");
#endif
        go.transform.SetParent(container, false);
        return go.transform;
    }

    private GameObject CreateScatterObject(GameObject prefab, Transform parent)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
#endif
        return Instantiate(prefab, parent);
    }

    private void DestroyScatterObject(GameObject obj)
    {
        if (obj == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.DestroyObjectImmediate(obj);
            return;
        }
#endif
        Destroy(obj);
    }

    private int GetPointStableId(ScatterPointState point)
    {
        return Mathf.RoundToInt(point.position.x * 100f) ^
               Mathf.RoundToInt(point.position.y * 100f) ^
               Mathf.RoundToInt(point.position.z * 100f);
    }

    private float Hash01(int a, int b)
    {
        unchecked
        {
            uint x = (uint)(a * 73856093 ^ b * 19349663 ^ seed * 83492791);
            x ^= x >> 17;
            x *= 0xed5ad4bbU;
            x ^= x >> 11;
            x *= 0xac4c1b51U;
            x ^= x >> 15;
            x *= 0x31848babU;
            x ^= x >> 14;
            return (x & 0x00FFFFFF) / 16777215f;
        }
    }

    private string BuildRuleSignature(float temperature, float humidity)
    {
        if (rules == null || rules.Length == 0)
            return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < rules.Length; i++)
        {
            ClimateScatterRule rule = rules[i];
            if (rule == null) continue;

            bool valid =
                temperature >= rule.minTemperature &&
                temperature <= rule.maxTemperature &&
                humidity >= rule.minHumidity &&
                humidity <= rule.maxHumidity &&
                rule.prefabs != null &&
                rule.prefabs.Length > 0;

            sb.Append(valid ? '1' : '0');
        }

        return sb.ToString();
    }

    public void ApplyClimateIfRuleSetChanged(float temperature, float humidity)
    {
        string newSignature = BuildRuleSignature(temperature, humidity);

        if (_hasAppliedClimateOnce && newSignature == _lastRuleSignature)
            return;

        _hasAppliedClimateOnce = true;
        _lastRuleSignature = newSignature;

        RebuildLayoutAndRefresh(temperature, humidity);
    }

    private void ApplyRandomScatterRotation()
    {
        if (!rotateScatterBeforeRebuild)
            return;

        Vector3 euler = transform.localEulerAngles;
        euler.y = Random.Range(randomYRotationRange.x, randomYRotationRange.y);
        transform.localEulerAngles = euler;
    }

    public void RebuildLayoutAndRefresh(float temperature, float humidity)
    {
        SetClimate(temperature, humidity);

        ClearSpawnedInstances();

        _points.Clear();
        _pointsBuilt = false;

        ApplyRandomScatterRotation();
        BuildSpawnPoints();
        RefreshPoints();
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

        if (GUILayout.Button("Build Spawn Points"))
            s.BuildSpawnPoints();

        if (GUILayout.Button("Refresh Current Climate"))
            s.RefreshPoints();

        if (GUILayout.Button("Clear Spawned"))
            s.ClearSpawnedInstances();

        if (GUILayout.Button("Rebuild All"))
            s.ClearAll();
    }
}
#endif
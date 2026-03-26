using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class UniversalScatter : MonoBehaviour
{
    public enum DistributionMode
    {
        Random,
        PoissonDisc
    }

    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Spawn Area")]
    public Vector2 areaSize = new Vector2(20f, 20f);
    public float rayStartHeight = 50f;
    public LayerMask includeMask;
    public LayerMask excludeMask;

    [Header("Amount")]
    public int count = 100;

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
    public Vector2 scaleMultiplierRange = new Vector2(1f, 1f);

    [Header("Deterministic")]
    public int seed = 12345;

    [Header("Parenting")]
    public Transform container;
    public string spawnedContainerName = "Spawned";

    // ============================
    // PUBLIC API
    // ============================

    public void Generate()
    {
        if (prefabs == null || prefabs.Length == 0) return;

        Transform parent = GetOrCreateSpawnedContainer();

        var oldState = Random.state;
        Random.InitState(seed);

        List<Vector2> points = distribution == DistributionMode.PoissonDisc
            ? GeneratePoissonPoints()
            : GenerateRandomPoints();

        foreach (var p in points)
        {
            Vector3 rayOrigin = transform.TransformPoint(new Vector3(p.x, rayStartHeight, p.y));

            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayStartHeight * 2f, includeMask))
                continue;

            if (excludeMask.value != 0)
            {
                int hitLayer = 1 << hit.collider.gameObject.layer;
                if ((excludeMask.value & hitLayer) != 0) continue;
            }

            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > maxSlopeAngle) continue;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

#if UNITY_EDITOR
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(obj, "Scatter Spawn");
#else
            GameObject obj = Instantiate(prefab, parent);
#endif

            obj.transform.position = hit.point;

            // Rotation
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

            // Scale
            float mult = Random.Range(scaleMultiplierRange.x, scaleMultiplierRange.y);
            obj.transform.localScale = prefab.transform.localScale * mult;
        }

        Random.state = oldState;
    }

    public void Clear()
    {
        if (container == null) return;
        Transform spawned = container.Find(spawnedContainerName);
        if (spawned == null) return;

        for (int i = spawned.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(spawned.GetChild(i).gameObject);
#else
            DestroyImmediate(spawned.GetChild(i).gameObject);
#endif
        }
    }

    // ============================
    // RANDOM DISTRIBUTION
    // ============================

    List<Vector2> GenerateRandomPoints()
    {
        List<Vector2> points = new();
        int attempts = 0;
        int maxAttempts = count * 20;

        while (points.Count < count && attempts < maxAttempts)
        {
            attempts++;
            Vector2 p = SamplePoint();
            bool ok = true;

            if (minDistance > 0f)
            {
                foreach (var q in points)
                {
                    if ((p - q).sqrMagnitude < minDistance * minDistance)
                    {
                        ok = false;
                        break;
                    }
                }
            }

            if (ok) points.Add(p);
        }

        return points;
    }

    // ============================
    // POISSON-DISC SAMPLING
    // ============================

    List<Vector2> GeneratePoissonPoints()
    {
        List<Vector2> points = new();
        List<Vector2> active = new();

        Vector2 first = SamplePoint();
        points.Add(first);
        active.Add(first);

        const int k = 30;

        while (active.Count > 0 && points.Count < count)
        {
            int idx = Random.Range(0, active.Count);
            Vector2 center = active[idx];
            bool found = false;

            for (int i = 0; i < k; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(minDistance, minDistance * 2f);
                Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (!IsInsideArea(candidate)) continue;

                bool ok = true;
                foreach (var p in points)
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

    // ============================
    // SAMPLING HELPERS
    // ============================

    Vector2 SamplePoint()
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

    bool IsInsideArea(Vector2 p)
    {
        if (useDonut)
        {
            float d = p.magnitude;
            return d >= innerRadius && d <= outerRadius;
        }

        return Mathf.Abs(p.x) <= areaSize.x * 0.5f &&
               Mathf.Abs(p.y) <= areaSize.y * 0.5f;
    }

    Transform GetOrCreateSpawnedContainer()
    {
        if (container == null) container = transform;

        Transform existing = container.Find(spawnedContainerName);
        if (existing != null) return existing;

        GameObject go = new GameObject(spawnedContainerName);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(go, "Create Spawned Container");
#endif
        go.transform.SetParent(container, false);
        return go.transform;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UniversalScatter))]
public class UniversalScatterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var s = (UniversalScatter)target;

        GUILayout.Space(8);
        if (GUILayout.Button("Generate (One-time)")) s.Generate();
        if (GUILayout.Button("Clear Spawned")) s.Clear();
    }
}
#endif
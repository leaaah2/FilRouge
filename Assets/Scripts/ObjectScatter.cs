using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class UniversalScatter : MonoBehaviour
{
    [Header("Prefabs to spawn")]
    public GameObject[] prefabs;

    [Header("Spawn area (local space box)")]
    public Vector2 areaSize = new Vector2(20f, 20f); // XZ
    public float rayStartHeight = 50f;
    public LayerMask includeMask; // e.g. Ground layer

    [Header("Optional exclusion (skip hits on these layers)")]
    public LayerMask excludeMask; // e.g. Path/Water (optional)

    [Header("Amount")]
    public int count = 200;

    [Header("Deterministic placement")]
    public int seed = 12345;

    [Header("Placement rules")]
    [Tooltip("Minimum distance between spawned objects (world units). 0 disables.")]
    public float minDistance = 0f;

    [Tooltip("Max slope angle allowed (degrees). 90 = no limit.")]
    [Range(0f, 90f)]
    public float maxSlopeAngle = 35f;

    [Tooltip("If true, align object's up axis to the ground normal (good for rocks).")]
    public bool alignToNormal = false;

    [Header("Randomization")]
    public bool randomYRotation = true;

    [Tooltip("Uniform scale range multiplier applied to prefab's current scale.")]
    public Vector2 scaleMultiplierRange = new Vector2(1.0f, 1.0f);

    [Header("Donut distribution (optional)")]
    public bool useDonut = false;
    [Min(0f)] public float innerRadius = 10f;
    [Min(0f)] public float outerRadius = 25f;

    [Header("Parenting")]
    [Tooltip("Where to create the spawned container. Usually a folder GameObject like ForestGrass/ForestRocks/ForestTrees.")]
    public Transform container;

    [Tooltip("Child under 'container' that will hold spawned instances.")]
    public string spawnedContainerName = "Spawned";

    [Header("Debug")]
    public bool drawGizmo = true;

    public void Generate()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning($"{name}: No prefabs assigned.");
            return;
        }

        if (useDonut && outerRadius <= innerRadius)
        {
            Debug.LogWarning($"{name}: Donut invalid (outerRadius must be > innerRadius).");
            return;
        }

        Transform spawnedParent = GetOrCreateSpawnedContainer();

        // Save & set deterministic random state
        var oldState = Random.state;
        Random.InitState(seed);

        // Track positions for min-distance
        List<Vector3> placed = (minDistance > 0f) ? new List<Vector3>(count) : null;

        int attempts = 0;
        int maxAttempts = Mathf.Max(count * 25, 3000); // avoid infinite loops

        int placedCount = 0;

        while (placedCount < count && attempts < maxAttempts)
        {
            attempts++;

            // Pick spawn point in local XZ:
            Vector2 p;
            if (useDonut)
            {
                // Uniform area distribution in an annulus:
                float r = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));
                float a = Random.Range(0f, Mathf.PI * 2f);
                p = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            else
            {
                p = new Vector2(
                    Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                    Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f)
                );
            }

            Vector3 rayOrigin = transform.TransformPoint(new Vector3(p.x, rayStartHeight, p.y));

            // Raycast down
            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayStartHeight * 2f, includeMask))
                continue;

            // Optional exclusion (if hit collider's layer is excluded, skip)
            if (excludeMask.value != 0)
            {
                int hitLayerMask = 1 << hit.collider.gameObject.layer;
                if ((excludeMask.value & hitLayerMask) != 0)
                    continue;
            }

            // Slope limit
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > maxSlopeAngle)
                continue;

            // Min distance check
            if (minDistance > 0f)
            {
                bool tooClose = false;
                float minDistSq = minDistance * minDistance;
                for (int i = 0; i < placed.Count; i++)
                {
                    if ((placed[i] - hit.point).sqrMagnitude < minDistSq)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;
            }

            // Pick prefab
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

#if UNITY_EDITOR
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, spawnedParent);
            Undo.RegisterCreatedObjectUndo(obj, "Scatter Spawn");
#else
            GameObject obj = Instantiate(prefab, spawnedParent);
#endif

            // Position
            obj.transform.position = hit.point;

            // Rotation
            Quaternion rot;
            if (alignToNormal)
            {
                rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                if (randomYRotation)
                {
                    float yaw = Random.Range(0f, 360f);
                    rot = Quaternion.AngleAxis(yaw, hit.normal) * rot;
                }
            }
            else
            {
                float yaw = randomYRotation ? Random.Range(0f, 360f) : 0f;
                rot = Quaternion.Euler(0f, yaw, 0f);
            }
            obj.transform.rotation = rot;

            // Scale (multiplier of prefab scale)
            float mult = Random.Range(scaleMultiplierRange.x, scaleMultiplierRange.y);
            obj.transform.localScale = prefab.transform.localScale * mult;

            // Record
            placed?.Add(hit.point);
            placedCount++;
        }

        Random.state = oldState;

        if (placedCount < count)
            Debug.LogWarning($"{name}: Placed {placedCount}/{count}. Try increasing area/outerRadius, lowering minDistance, or increasing maxAttempts.");
    }

    public void Clear()
    {
        if (container == null) container = transform;
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

    private Transform GetOrCreateSpawnedContainer()
    {
        if (container == null) container = transform;

        Transform existing = container.Find(spawnedContainerName);
        if (existing != null) return existing;

        GameObject go = new GameObject(spawnedContainerName);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(go, "Create Spawned Container");
#endif
        go.transform.SetParent(container, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        // Box gizmo (for non-donut usage)
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaSize.x, 0.1f, areaSize.y));

#if UNITY_EDITOR
        // Donut gizmos (approximate circles in XZ)
        if (useDonut)
        {
            DrawCircleGizmo(innerRadius);
            DrawCircleGizmo(outerRadius);
        }
#endif
    }

#if UNITY_EDITOR
    private void DrawCircleGizmo(float radius)
    {
        if (radius <= 0f) return;
        const int segments = 64;
        Vector3 prev = new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float t = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = new Vector3(Mathf.Cos(t) * radius, 0f, Mathf.Sin(t) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
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

        if (GUILayout.Button("Generate (One-time)"))
            s.Generate();

        if (GUILayout.Button("Clear Spawned"))
            s.Clear();
    }
}
#endif
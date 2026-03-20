using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class MeshInverter : MonoBehaviour
{
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = Instantiate(mf.sharedMesh);

        InsideOut(mesh);

        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/InvertedMesh.asset");
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Asset saved at: " + path);
    }

    void InsideOut(Mesh mesh)
    {
        var triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int tmp = triangles[i + 1];
            triangles[i + 1] = triangles[i + 2];
            triangles[i + 2] = tmp;
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}
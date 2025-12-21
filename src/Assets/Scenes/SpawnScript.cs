using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class SpawnScript : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] float spawnInterval = 1f;
    [SerializeField] MeshFilter meshFilter = default!;

    List<Transform> transforms = new();

    void Spawn()
    {
        var obj = Instantiate(prefab, transform.position, Quaternion.identity);
        transforms.Add(obj.transform);
    }

    void Start()
    {
        InvokeRepeating(nameof(Spawn), 0f, spawnInterval);
    }

    void Update()
    {
        if (transforms.Count > 0 && transforms[0] == null)
        {
            transforms.RemoveAt(0);
        }

        if (transforms.Count >= 3)
        {
            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        int n = transforms.Count;

        int verexCount = n;
        Vector3[] vertices = new Vector3[verexCount];
        for(int i = 0; i < n; i++)
        {
            if(transforms[i] == null)
            {
                vertices[i] = Vector3.zero;
            }
            else
                vertices[i] = transforms[i].position;
        }

        Color[] colors = new Color[verexCount];
        for (int i = 0; i < verexCount; i++)
        {
            float t = (float)i / verexCount;
            colors[i] = new Color(t, t, t);
        }

        int polygonCount = 2 * (n - 2);
        int[] triangles = new int[3 * polygonCount];
        int idx = 0;
        int vtx = 0;
        while (idx < 3 * polygonCount)
        {
            triangles[idx + 0] = vtx + 0;
            triangles[idx + 1] = vtx + 2;
            triangles[idx + 2] = vtx + 1;

            triangles[idx + 3] = vtx + 0;
            triangles[idx + 4] = vtx + 1;
            triangles[idx + 5] = vtx + 2;

            idx += 6;
            vtx += 1;
        }

        Mesh mesh = meshFilter.mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}

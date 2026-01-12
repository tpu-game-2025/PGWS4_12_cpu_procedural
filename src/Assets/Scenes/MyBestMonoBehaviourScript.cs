using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

using RangeAttribute = UnityEngine.RangeAttribute;

public class MyBestMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter = default!;

    [SerializeField][Range(0.0f, 90.0f)] private float bendMax = 70.0f;
    [SerializeField][Range(0.0f, 1.0f)] private float gravityRate = 0.3f;
    [SerializeField][Range(0.0f, 100.0f)] private float distanceMin = 3.0f;
    [SerializeField][Range(0.0f, 100.0f)] private float distanceMax = 20.0f;

    float time = 0.0f;

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        if (time <= 0.0f)
        {
            Generate();
            time = Random.Range(1.0f, 3.0f);
        }
    }
    void Generate()
    {
        float HEIGHT = 1000.0f;
        Vector3 pos = new Vector3(0.0f, HEIGHT, 0.0f);
        Vector3 dir = Vector3.down;

        List<Vector3> path = new List<Vector3>();
        path.Add(pos);
        for (int i = 0; i < 1000; i++)
        {
            float angle = Random.Range(-bendMax, +bendMax);
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            dir = q * dir;
            dir = Vector3.Lerp(dir, Vector3.down, gravityRate);

            float distance = Random.Range(distanceMin, distanceMax);
            pos += dir.normalized * distance;
            path.Add(pos);

            if (pos.y < 0.0f) break;
        }

        UpdateMesh(path);
    }

    void UpdateMesh(List<Vector3> path)
    {
        int n = path.Count;

        int vertexCount = 2 * n;
        Vector3[] vertices = new Vector3[vertexCount];
        float baseHalfWidth = 3.0f;

        int vtx = 0;
        {
            Vector3 center = path[0];
            vertices[vtx + 0] = center + Vector3.left * baseHalfWidth;
            vertices[vtx + 1] = center - Vector3.left * baseHalfWidth;
            vtx += 2;
        }

        for (int i = 1; i < n - 1; i++)
        {
            Vector3 center = path[i];

            float t = (float)i / (n - 1);
            float halfWidth = Mathf.Lerp(baseHalfWidth, 0.2f, t);

            Vector3 forward = (path[i + 1] - path[i - 1]).normalized;
            Vector3 right = Vector3.Cross(Vector3.forward, forward).normalized;

            vertices[vtx + 0] = center + right * halfWidth;
            vertices[vtx + 1] = center - right * halfWidth;
            vtx += 2;
        }
        {
            Vector3 center = path[n - 1];
            vertices[vtx + 0] = center + Vector3.left * baseHalfWidth;
            vertices[vtx + 1] = center - Vector3.left * baseHalfWidth;
            vtx += 2;
        }

        Color[] colors = new Color[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            float height01 = Mathf.InverseLerp(0.0f, 1000.0f, vertices[i].y);
            colors[i] = Color.Lerp(Color.red, Color.white, height01);
        }

        int polygonCount = 2 * (n - 1);
        int[] triangles = new int[3 * polygonCount];
        int idx = 0;
        vtx = 0;
        while (idx < 3 * polygonCount)
        {
            triangles[idx + 0] = vtx + 0;
            triangles[idx + 1] = vtx + 2;
            triangles[idx + 2] = vtx + 1;

            triangles[idx + 3] = vtx + 1;
            triangles[idx + 4] = vtx + 2;
            triangles[idx + 5] = vtx + 3;

            idx += 6;
            vtx += 2;
        }

        Mesh Mesh = meshFilter.mesh;

        Mesh.Clear();
        Mesh.vertices = vertices;
        Mesh.colors = colors;
        Mesh.triangles = triangles;

        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();
    }
}

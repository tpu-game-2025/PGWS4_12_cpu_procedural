using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class RandomWalk_Script : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter = default!;

    [SerializeField][Range(0f, 90f)] float bendMax = 70f;
    [SerializeField][Range(0f, 1f)] float gravityRate = 0.3f;
    [SerializeField][Range(0f, 100f)] float distanceMin = 70f;
    [SerializeField][Range(0f, 100f)] float distanceMax = 70f;

    float timer = 0.0f;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0.0f)
        {
            Generate();
            timer = UnityEngine.Random.Range(1f, 3f);
        }
    }

    void Generate()
    {
        float HEIGHT = 1000f;
        Vector3 pos = new Vector3(0f, HEIGHT, 0f);
        Vector3 dir = Vector3.down;

        List<Vector3> path = new();
        path.Add(pos);

        for(int i = 0; i < 1000; i++)
        {
            float angle=UnityEngine.Random.Range(-bendMax, bendMax);
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            dir = q * dir;
            dir = Vector3.Lerp(dir, Vector3.down, gravityRate);

            float distance = UnityEngine.Random.Range(distanceMin, distanceMax);
            pos += dir.normalized * distance;
            path.Add(pos);

            if (pos.y < 0f) break;
        }

        UpdateMesh(path);
    }

    void UpdateMesh(List<Vector3> path)
    {
        int n = path.Count;

        int verexCount = 2 * (n + 0);
        Vector3[] vertices = new Vector3[verexCount];
        float halfWidth = 3f;

        int vtx = 0;
        {   // start
            Vector3 center = path[0];
            vertices[vtx + 0] = center + Vector3.left * halfWidth;
            vertices[vtx + 1] = center + Vector3.right * halfWidth;
            vtx += 2;
        }
        for (int i = 1; i < n-1; i++)
        {
            Vector3 center = path[i];
            Vector3 right = (path[i+1] - path[i-1]).normalized;
            right = new Vector3(-right.y, right.x, right.z);
            vertices[vtx + 0] = center + right * halfWidth;
            vertices[vtx + 1] = center - right * halfWidth;
            vtx += 2;
        }
        {   // end
            Vector3 center = path[n - 1];
            vertices[vtx + 0] = center + Vector3.left * halfWidth;
            vertices[vtx + 1] = center + Vector3.right * halfWidth;
            vtx += 2;
        }

        Color[] colors = new Color[verexCount];
        for (int i = 0; i < verexCount; i++)
        {
            colors[i] = Color.white;
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

        Mesh mesh = meshFilter.mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}

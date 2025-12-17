using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Thunder_Script : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter = default!;
    [SerializeField] Material material = default!;
    Texture2D texture = null;
    [SerializeField] int TEX_WIDTH = 128;
    [SerializeField] int TEX_HEIGHT = 64;

    Lichtenberg lichtenberg = null;

    float timer = 0.0f;

    void Start()
    {
        texture = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.RGBA32, false);
        material.SetTexture("_Texture2D", texture);

        lichtenberg = new(TEX_HEIGHT, TEX_WIDTH, Lichtenberg.MODE.FINISH_AT_FIRST_ARRLIVE);
        UpdateTexture();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if( timer < 0.0f)
        {
            Generate();
            timer = UnityEngine.Random.Range(1f, 3f);
        }
    }

    void Generate()
    {
        int x = TEX_WIDTH / 2;
        int y = TEX_HEIGHT - 1;
        lichtenberg.Initialize(y, x);

        while (lichtenberg.Update() == Lichtenberg.State.Running) ;
        UpdateTexture();

        List<int> path = new();

        int p = lichtenberg.ArriveIndex;
        while (lichtenberg.Parent[p] != p)
        {
            path.Add(p);
            p = lichtenberg.Parent[p];
        }
        path.Add(p);

        UpdateMesh(path);
    }

    void UpdateTexture()
    {
        var pixelData = texture.GetPixelData<Color32>(0);

        ushort[] value = lichtenberg.Value;
        double vmax = (double)lichtenberg.ValueMax;

        int idx = 0;
        for (int y = 0; y < TEX_HEIGHT; y++)
        {
            for (int x = 0; x < TEX_WIDTH; x++)
            {
                byte c = (byte)(256.0 * (double)value[idx] / (vmax + 1));
                pixelData[idx] = new Color32(c, c, c, 255);
                idx++;
            }
        }

        texture.Apply();
    }

    void UpdateMesh(List<int> path)
    {
        int n = path.Count;

        int verexCount = 2 * (n + 2);
        Vector3[] vertices = new Vector3[verexCount];
        float SIZE = 1000.0f / (float)TEX_HEIGHT;
        float halfWidth = SIZE * 0.5f;

        int vtx = 0;
        {   // start
            int index = path[0];
            int x = index % TEX_WIDTH - TEX_WIDTH / 2;
            Vector3 center = new Vector3(halfWidth * 2f * (float)x, 0f, 0f);
            vertices[vtx + 0] = center + Vector3.left * halfWidth;
            vertices[vtx + 1] = center + Vector3.right * halfWidth;
            vtx += 2;
        }
        for(int i = 0; i < n; i++)
        {
            int index = path[i];
            int x = index % TEX_WIDTH - TEX_WIDTH / 2;
            int y = index / TEX_WIDTH;
            Vector3 center = new Vector3(halfWidth * 2f * (float)x, SIZE * (0.5f + (float)y), 0f);
            Vector3 right;
            if (i == 0)
            {
                int nextIndex = path[1];
                int nextX = nextIndex % TEX_WIDTH;
                int nextY = nextIndex / TEX_WIDTH;
                int prevIndex = path[0];
                int prevX = prevIndex % TEX_WIDTH;
                int prevY = prevIndex / TEX_WIDTH - 1;
                right = new Vector3(-(float)(nextY - prevY), (float)(nextX - prevX), 0f);
            }
            else if(i < n - 1)
            {
                int nextIndex = path[i + 1];
                int nextX = nextIndex % TEX_WIDTH;
                int nextY = nextIndex / TEX_WIDTH;
                int prevIndex = path[i - 1];
                int prevX = prevIndex % TEX_WIDTH;
                int prevY = prevIndex / TEX_WIDTH;
                right = new Vector3(-(float)(nextY - prevY), (float)(nextX - prevX), 0f);
            }
            else
            {
                int nextIndex = path[n - 1];
                int nextX = nextIndex % TEX_WIDTH;
                int nextY = nextIndex / TEX_WIDTH + 1;
                int prevIndex = path[n - 2];
                int prevX = prevIndex % TEX_WIDTH;
                int prevY = prevIndex / TEX_WIDTH;
                right = new Vector3(-(float)(nextY - prevY), (float)(nextX - prevX), 0f);
            }
            vertices[vtx + 0] = center + right * 2f * halfWidth / Vector3.Dot(right, right);
            vertices[vtx + 1] = center - right * 2f * halfWidth / Vector3.Dot(right, right);
            vtx += 2;
        }
        {   // end
            int index = path[n - 1];
            int x = index % TEX_WIDTH - TEX_WIDTH / 2;
            int y = TEX_HEIGHT;
            Vector3 center = new Vector3(halfWidth * 2f * (float)x, SIZE * (float)TEX_HEIGHT, 0f);
            vertices[vtx + 0] = center + Vector3.left * halfWidth;
            vertices[vtx + 1] = center + Vector3.right * halfWidth;
            vtx += 2;
        }

        Color[] colors = new Color[verexCount];
        for(int i = 0; i < verexCount; i++)
        {
            colors[i] = Color.white;
        }

        int polygonCount = 2 * (n + 1);
        int[] triangles = new int[3 * polygonCount];
        int idx = 0;
        vtx = 0;
        while(idx < 3 * polygonCount)
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

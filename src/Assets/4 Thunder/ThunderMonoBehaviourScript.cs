using UnityEngine;
using System.Collections.Generic;

public class ThunderMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter = default!;
    [SerializeField] Material material = default!;
    Texture2D texture = null;
    [SerializeField] int TEX_WIDTH = 128;
    [SerializeField] int TEX_HEIGHT = 64;

    Lichtenberg lichtenberg = null;

    float time = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        texture = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.RGBA32, false);
        material.SetTexture("_Texture2D", texture);

        // テクスチャと関連するもの初期化
        lichtenberg = new Lichtenberg(TEX_HEIGHT, TEX_WIDTH,
            Lichtenberg.MODE.FINISH_AT_FIRST_ARRIVE);
        UpdateTexture();
    }

    void Update()
    {
        time -= Time.deltaTime; 
        if (time <= 0.0f)
        {
            Generate();
            time = Random.Range(1.0f, 3.0f);// 1秒から3秒の間隔で再生成
        }
    }

    void Generate()
    {
        // 検索の初期化
        int x = TEX_WIDTH / 2;
        int y = TEX_HEIGHT - 1;// yが大きい方が上
        lichtenberg.Initialize(y, x);

        // 雷テクスチャの生成
        while (lichtenberg.Update() == Lichtenberg.State.Running) ;
        UpdateTexture();

        // 到達経路の取得
        List<int> path = new List<int>();

        int p = lichtenberg.ArriveIndex;
        while (lichtenberg.Parent[p] != p)
        {
            path.Add(p);
            p = lichtenberg.Parent[p];
        }
        path.Add(p);// 最後に根元を追加

        // メッシュの構築
        UpdateMesh(path);
    }

    void UpdateTexture()
    {
        var pixelData = texture.GetPixelData<Color32>(0);

        ushort[] value = lichtenberg.Value;
        double vMax = (double)lichtenberg.ValueMax;

        int idx = 0;
        for (int y = 0; y < TEX_HEIGHT; y++)
        {
            for (int x = 0; x < TEX_WIDTH; x++)
            {
                byte c = (byte)(256.0 * (double)value[idx] / (vMax + 1));// 256にはならないようにする
                pixelData[idx] = new Color32(c, c, c, 255);
                idx++;
            }
        }

        texture.Apply();
    }
    void UpdateMesh(List<int> path)
    {
        int n = path.Count;

        // 頂点の生成
        int vertexCount = 2 * (n + 2);// 始点と終点に2頂点ずつ追加
        Vector3[] vertices = new Vector3[vertexCount];
        float SIZE = 1000.0f / (float)TEX_HEIGHT;
        float halfWidth = SIZE * 0.5f;

        int vtx = 0;
        // 始点
        {
            int index = path[0];
            int x = index % TEX_WIDTH - TEX_WIDTH / 2;
            Vector3 center = new Vector3(halfWidth * 2.0f * (float)x, 0.0f, 0.0f);
            vertices[vtx + 0] = center + Vector3.left * halfWidth;
            vertices[vtx + 1] = center - Vector3.left * halfWidth;
            vtx += 2;
        }
        for (int i = 0; i < n; i++)
        {
            int index = path[i];
            int x = index % TEX_WIDTH - TEX_WIDTH / 2;
            int y = index / TEX_WIDTH;
            Vector3 center = new Vector3(halfWidth * 2.0f * (float)x, SIZE * (0.5f + (float)y), 0.0f);
            // 幅方向のベクトルを計算
            Vector3 right;
            if (i == 0)
            {
                // 始点は次の点と始点の一つ下の点の差分で方向を決定
                int nextIndex = path[1];
                int nextX = nextIndex % TEX_WIDTH;
                int nextY = nextIndex / TEX_WIDTH;
                int prevIndex = path[0];
                int prevX = prevIndex % TEX_WIDTH;
                int prevY = prevIndex / TEX_WIDTH - 1;
                right = new Vector3(-(float)(nextY - prevY), (float)(nextX - prevX), 0.0f);
            }
            else if(i < n - 1)
            {
                // 端点以外は前後の差分で方向を決定
                int nextIndex = path[i + 1];
                int nextX = nextIndex % TEX_WIDTH;
                int nextY = nextIndex / TEX_WIDTH;
                int prevIndex = path[i - 1];
                int prevX = prevIndex % TEX_WIDTH;
                int prevY = prevIndex / TEX_WIDTH;
                right = new Vector3(-(float)(nextY - prevY), (float)(nextX - prevX), 0.0f);
            }
            else
            {
                // 終点は前の点と終点の一つ上の点の差分で方向を決定
                int nextIndex = path[n - 1];
                int nextX = nextIndex % TEX_WIDTH;
                int nextY = nextIndex / TEX_WIDTH + 1;
                int prevIndex = path[n - 2];
                int prevX = prevIndex % TEX_WIDTH;
                int prevY = prevIndex / TEX_WIDTH;
                right = new Vector3(-(float)(nextY - prevY), (float)(nextX - prevX), 0.0f);
            }
            vertices[vtx + 0] = center + right * 2.0f * halfWidth / Vector3.Dot(right, right);
            vertices[vtx + 1] = center - right * 2.0f * halfWidth / Vector3.Dot(right, right);
            vtx += 2;
        }
        // 終点
        {
            int index = path[n - 1];
            int x = index % TEX_WIDTH - TEX_WIDTH / 2;
            int y = TEX_HEIGHT;
            Vector3 center = new Vector3(halfWidth * 2.0f * (float)x, SIZE * (float)TEX_HEIGHT, 0.0f);
            vertices[vtx + 0] = center + Vector3.left * halfWidth;
            vertices[vtx + 1] = center - Vector3.left * halfWidth;
            vtx += 2;
        }

        Color[] colors = new Color[vertexCount];
        for(int i = 0; i < vertexCount; i++)
        {
            colors[i] = Color.white;
        }

        // インデックスの生成
        int polygonCount = 2 * (n + 1);// 四角形の数
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

        //  メッシュの更新
        Mesh Mesh = meshFilter.mesh;

        Mesh.Clear();
        Mesh.vertices = vertices;
        Mesh.colors = colors;
        Mesh.triangles = triangles;

        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();
    }
}

using UnityEngine;

public class Polygon_Script : MonoBehaviour
{
    void Start()
    {
        Vector3[] vertices =
        {
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
        };

        Color[] colors = { Color.red, Color.green, Color.blue, };
        int[] triangles = { 0, 1, 2 };

        Mesh Mesh = GetComponent<MeshFilter>().mesh;

        Mesh.Clear();
        Mesh.vertices = vertices;
        Mesh.colors = colors;
        Mesh.triangles = triangles;

        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();
    }
}

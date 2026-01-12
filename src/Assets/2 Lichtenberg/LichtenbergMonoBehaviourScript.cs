using System.Collections;
using UnityEngine;

public class LichtenbergMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] Material material = default!;
    Texture2D texture = null;
    [SerializeField] int TEX_WIDTH = 128;
    [SerializeField] int TEX_HEIGHT = 64;

    LichtenbergEmptyCSharpScript lichtenberg = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        texture = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.RGBA32, false);
        material.SetTexture("_Texture2D", texture);

        lichtenberg = new LichtenbergEmptyCSharpScript(TEX_HEIGHT, TEX_WIDTH);

        int x = TEX_WIDTH / 2;
        int y = TEX_HEIGHT - 1;
        lichtenberg.Initialize(y, x);

        StartCoroutine(Simulate());
    }

    private IEnumerator Simulate()
    {
        while (lichtenberg.Update()==LichtenbergEmptyCSharpScript.State.Running)
        {
            UpdateTexture();
            yield return null;
        }
    }

    void UpdateTexture()
    {
        var pixelDate = texture.GetPixelData<Color32>(0);

        ushort[] value = lichtenberg.Value;
        double vMax = (double)lichtenberg.ValueMax;

        int idx = 0;
        for(int y = 0; y < TEX_HEIGHT; y++)
        {
            for(int x = 0; x < TEX_WIDTH; x++)
            {
                byte c = (byte)(256.0 * (double)value[idx] / (vMax + 1));
                pixelDate[idx] = new Color32(c, c, c, 255);
                idx++;
            }
        }

        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

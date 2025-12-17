using UnityEngine;

public class Texture_Script : MonoBehaviour
{
    [SerializeField] Material material = default!;
    Texture2D texture = null;
    [SerializeField] int TEX_WIDTH = 1920;
    [SerializeField] int TEX_HEIGHT = 1080;

    void Start()
    {
        texture = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.RGBA32, false);
        material.SetTexture("_Texture2D", texture);

        UpdateTexture();
    }

    void UpdateTexture()
    {
        var pixelData = texture.GetPixelData<Color32>(0);

        for(int y=0;y<TEX_HEIGHT; y++)
        {
            byte g = (byte)((256.0 / (double)(TEX_HEIGHT + 1)) * (double)y);

            for(int x = 0; x < TEX_WIDTH; x++)
            {
                byte r = (byte)((256.0 / (double)(TEX_WIDTH + 1)) * (double)x);

                pixelData[y * TEX_WIDTH + x] = new Color32(r, g, 0, 255);
            }
        }

        texture.Apply();
    }
}

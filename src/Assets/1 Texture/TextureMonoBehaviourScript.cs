using UnityEngine;

public class TextureMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] Material material = default!;
    Texture2D texture = null;
    [SerializeField] int TEX_WIDTH = 1980;
    [SerializeField] int TEX_HEIGHT = 1080;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        texture = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.RGBA32, false);
        material.SetTexture("_Texture2D", texture);

        UpdateTexture();
    }

    void UpdateTexture()
    {
        var pixelData = texture.GetPixelData<Color32>(0);

        for (int y = 0; y < TEX_HEIGHT; y++)
        {
            // 縦方向は緑のグラデーション: [0,256)
            byte g = (byte)((256.0 / (double)TEX_HEIGHT) * (double)y);

            for (int x = 0; x < TEX_WIDTH; x++)
            {
                // 横方向は赤のグラデーション: [0,256)
                byte r = (byte)((256.0 / (double)TEX_WIDTH) * (double)x); 

                pixelData[y * TEX_WIDTH + x] = new Color32(r, g, 0, 255);// 青は0固定
            }
        }

        texture.Apply();
    }
}

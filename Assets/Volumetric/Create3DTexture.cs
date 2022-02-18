using UnityEditor;
using UnityEngine;

public class Create3DTexture : MonoBehaviour {

    [MenuItem("Custom/3DTexture")]
    static void CreateTexture3D() {
        int size = 128;

        Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBA32, false);

        Color[] colors = new Color[size * size * size];

        for (int x = 0; x < size; ++x) {
            for (int y = 0; y < size; ++y) {
                for (int z = 0; z < size; ++z) {
                    colors[x * size * size + y * size + z] = (x + y + z) % 20 < 5
                        ? new Color(x, y, z, size) / size
                        : new Color(0.0f, 0.0f, 0.0f, 0.0f);
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        AssetDatabase.CreateAsset(texture, "Assets/3DTexture/3DTexture.asset");
    }

}
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Volumetric : MonoBehaviour {

    public Shader shader;
    public Transform box;

    private Material material;

    private void OnEnable() {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (material == null) {
            material = new Material(shader);
        }

        material.SetVector("boxBoundsMin", box.position - box.localScale / 2.0f);
        material.SetVector("boxBoundsMax", box.position + box.localScale / 2.0f);

        Graphics.Blit(src, dest, material);
    }

}
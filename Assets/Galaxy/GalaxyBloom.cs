using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class GalaxyBloom : MonoBehaviour {

    public Shader bloomShader;

    [Range(0, 10)]
    public float intensity = 1;

    [Range(0, 5)]
    public float threshold = 1;

    [Range(0, 1)]
    public float softThreshold = 0.5f;

    [Range(1, 16)]
    public int iterations = 1;

    private Material bloomMaterial;

    private const int boxDownPrefilterPass = 0;
    private const int boxDownPass = 1;
    private const int boxUpPass = 2;
    private const int applyBloomPass = 3;

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (bloomMaterial == null) {
            bloomMaterial = new Material(bloomShader);
            bloomMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        float knee = threshold * softThreshold;
        Vector4 filter = new Vector4(threshold, threshold - knee, 2f * knee, 0.25f / (knee + 0.00001f));
        bloomMaterial.SetVector("_Filter", filter);
        bloomMaterial.SetFloat("_Intensity", Mathf.GammaToLinearSpace(intensity));

        int width = source.width / 2;
        int height = source.height / 2;
        RenderTextureFormat format = source.format;

        RenderTexture[] textures = new RenderTexture[16];

        RenderTexture currentDestination = textures[0] = RenderTexture.GetTemporary(width, height, 0, format);
        Graphics.Blit(source, currentDestination, bloomMaterial, boxDownPrefilterPass);
        RenderTexture currentSource = currentDestination;

        int i = 1;
        for (; i < iterations; i++) {
            width /= 2;
            height /= 2;
            if (height < 2) {
                break;
            }

            currentDestination = textures[i] = RenderTexture.GetTemporary(width, height, 0, format);
            Graphics.Blit(currentSource, currentDestination, bloomMaterial, boxDownPass);
            currentSource = currentDestination;
        }

        for (i -= 2; i >= 0; i--) {
            currentDestination = textures[i];
            textures[i] = null;
            Graphics.Blit(currentSource, currentDestination, bloomMaterial, boxUpPass);
            RenderTexture.ReleaseTemporary(currentSource);
            currentSource = currentDestination;
        }

        bloomMaterial.SetTexture("_SourceTex", source);
        Graphics.Blit(currentSource, destination, bloomMaterial, applyBloomPass);
        RenderTexture.ReleaseTemporary(currentSource);
    }

}
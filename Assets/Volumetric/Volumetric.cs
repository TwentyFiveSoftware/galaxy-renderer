using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Volumetric : MonoBehaviour {

    public ComputeShader computeShader;

    private ComputeBuffer pointsBuffer;
    private RenderTexture renderTexture;

    struct Point {
        public Vector3 position;
        public float radius;
        public Vector4 color;
    };

    private void OnEnable() {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;

        Point[] points = new Point[10000];
        for (int i = 0; i < points.Length; i++) {
            points[i] = new Point {
                position = new Vector3(Random.value, Random.value, Random.value),
                radius = 0.005f + Random.value * 0.02f,
                color = new Vector4(1.0f, 0.7f, 0.0f, 1)
            };
        }

        pointsBuffer?.Release();
        pointsBuffer = new ComputeBuffer(points.Length, sizeof(float) * 8);
        pointsBuffer.SetData(points);
    }

    private void OnDisable() {
        pointsBuffer?.Release();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (renderTexture == null) {
            renderTexture = new RenderTexture(src.width, src.height, 0, GraphicsFormat.R8G8B8A8_UNorm);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        int kernelIndex = computeShader.FindKernel("CSMain");

        computeShader.SetBuffer(kernelIndex, "pointBuffer", pointsBuffer);
        computeShader.SetTexture(kernelIndex, "renderTexture", renderTexture);

        int yDispatchCount = 50;
        int xGroups = Mathf.CeilToInt(renderTexture.width / 8.0f);
        int yGroups = Mathf.CeilToInt((renderTexture.height / 8.0f) / yDispatchCount);
        
        for (int y = 0; y < yDispatchCount; y++) {
            computeShader.SetInt("yPixelOffset", (yGroups * y) * 8);
            computeShader.Dispatch(kernelIndex, xGroups, yGroups, 1);
        }
        
        Graphics.Blit(renderTexture, dest);
    }

}
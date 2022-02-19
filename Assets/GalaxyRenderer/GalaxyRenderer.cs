using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

// [ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class GalaxyRenderer : MonoBehaviour {

    public ComputeShader computeShader;

    private ComputeBuffer galaxyBuffer;
    private ComputeBuffer shaderInternalGalaxyBuffer;
    private RenderTexture renderTexture;

    struct GalaxyParticle {
        public float angularPosition;
        public float distanceToCenter;
        public float yOffset;
        public float size;
        public Vector4 color;
    };

    private void OnEnable() {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;

        GalaxyParticle[] particles = new GalaxyParticle[10000];
        for (int i = 0; i < particles.Length; i++) {
            float distanceToCenter = Random.value;

            particles[i] = new GalaxyParticle() {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                distanceToCenter = distanceToCenter,
                yOffset = Random.value * 0.4f - 0.2f,
                color = new Vector4(1, 1, 1, 1),
                size = 0.002f + Random.value * 0.005f
            };
        }

        galaxyBuffer?.Release();
        galaxyBuffer = new ComputeBuffer(particles.Length, sizeof(float) * 8);
        galaxyBuffer.SetData(particles);

        shaderInternalGalaxyBuffer?.Release();
        shaderInternalGalaxyBuffer = new ComputeBuffer(particles.Length, sizeof(float) * 8);
    }

    private void OnDisable() {
        galaxyBuffer?.Release();
    }

    // private void OnRenderImage(RenderTexture src, RenderTexture dest) {
    // Debug.Log("RENDER");
    //
    // if (renderTexture == null) {
    //     renderTexture = new RenderTexture(src.width, src.height, 0, GraphicsFormat.R8G8B8A8_UNorm);
    //     renderTexture.enableRandomWrite = true;
    //     renderTexture.Create();
    // }
    //
    // int kernelIndex = computeShader.FindKernel("CSMain");
    //
    // computeShader.SetBuffer(kernelIndex, "galaxyBuffer", galaxyBuffer);
    // computeShader.SetBuffer(kernelIndex, "internalGalaxyBuffer", shaderInternalGalaxyBuffer);
    // computeShader.SetTexture(kernelIndex, "renderTexture", renderTexture);
    // computeShader.SetInt("particleAmount", galaxyBuffer.count);
    //
    // // int yDispatchCount = 100;
    // // int xGroups = Mathf.CeilToInt(renderTexture.width / 8.0f);
    // // int yGroups = Mathf.CeilToInt((renderTexture.height / 8.0f) / yDispatchCount);
    //
    // int xGroups = Mathf.CeilToInt(renderTexture.width / 8.0f);
    // int yGroups = Mathf.CeilToInt(renderTexture.height / 8.0f);
    //
    // for (int y = 0; y < yGroups; y++) {
    //     computeShader.SetInt("yPixelOffset", y * 8);
    //     computeShader.SetInt("calculateParticlePositions", y == 0 ? 1 : 0);
    //     computeShader.Dispatch(kernelIndex, xGroups, 1, 1);
    // }
    //
    // Graphics.Blit(renderTexture, dest);
    // }

    private int currentYGroup = 0;

    private void Start() {
        currentYGroup = 0;
    }

    private void Update() {
        if (renderTexture == null) {
            renderTexture = new RenderTexture(1920, 1080, 0, GraphicsFormat.R8G8B8A8_UNorm);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        int xGroups = Mathf.CeilToInt(renderTexture.width / 8.0f);
        int yGroups = Mathf.CeilToInt(renderTexture.height / 8.0f);

        if (currentYGroup > yGroups)
            return;

        Debug.Log("RENDER");


        int kernelIndex = computeShader.FindKernel("CSMain");

        if (currentYGroup == 0) {
            computeShader.SetBuffer(kernelIndex, "galaxyBuffer", galaxyBuffer);
            computeShader.SetBuffer(kernelIndex, "internalGalaxyBuffer", shaderInternalGalaxyBuffer);
            computeShader.SetTexture(kernelIndex, "renderTexture", renderTexture);
            computeShader.SetInt("particleAmount", galaxyBuffer.count);
        }

        // int yDispatchCount = 100;
        // int xGroups = Mathf.CeilToInt(renderTexture.width / 8.0f);
        // int yGroups = Mathf.CeilToInt((renderTexture.height / 8.0f) / yDispatchCount);

        computeShader.SetInt("yPixelOffset", currentYGroup * 8);
        computeShader.SetInt("calculateParticlePositions", currentYGroup == 0 ? 1 : 0);
        computeShader.Dispatch(kernelIndex, xGroups, 1, 1);

        currentYGroup++;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (renderTexture != null) {
            Graphics.Blit(renderTexture, dest);
        }
    }

    // private void Render() {
    //     if (renderTexture == null) {
    //         renderTexture = new RenderTexture(1920, 1080, 0, GraphicsFormat.R8G8B8A8_UNorm);
    //         renderTexture.enableRandomWrite = true;
    //         renderTexture.Create();
    //     }
    //
    //     int kernelIndex = computeShader.FindKernel("CSMain");
    //
    //     computeShader.SetBuffer(kernelIndex, "galaxyBuffer", galaxyBuffer);
    //     computeShader.SetBuffer(kernelIndex, "internalGalaxyBuffer", shaderInternalGalaxyBuffer);
    //     computeShader.SetTexture(kernelIndex, "renderTexture", renderTexture);
    //     computeShader.SetInt("particleAmount", galaxyBuffer.count);
    //
    //     int yDispatchCount = 100;
    //     int xGroups = Mathf.CeilToInt(renderTexture.width / 8.0f);
    //     int yGroups = Mathf.CeilToInt((renderTexture.height / 8.0f) / yDispatchCount);
    //
    //     for (int y = 0; y < yDispatchCount; y++) {
    //         computeShader.SetInt("yPixelOffset", yGroups * y * 8);
    //         computeShader.SetInt("calculateParticlePositions", y == 0 ? 1 : 0);
    //         computeShader.Dispatch(kernelIndex, xGroups, yGroups, 1);
    //     }
    // }
    //
    // [CustomEditor(typeof(GalaxyRenderer))]
    // public class GalaxyRendererEditor : Editor {
    //     public override void OnInspectorGUI() {
    //         base.OnInspectorGUI();
    //         if (GUILayout.Button("Render")) {
    //             ((GalaxyRenderer)target).Render();
    //         }
    //     }
    // }

}
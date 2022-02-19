using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Volumetric : MonoBehaviour {

    public Shader shader;
    public Transform box;

    private Material material;
    private ComputeBuffer pointsBuffer;
    private RenderTexture texture;

    struct Point {
        public Vector3 position;
        public float radius;
        public Vector4 color;
    };

    private void OnEnable() {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        Point[] points = new Point[100];
        for (int i = 0; i < points.Length; i++) {
            points[i] = new Point() {
                position = new Vector3(Random.value, Random.value, Random.value),
                radius = 0.005f + Random.value * 0.02f,
                color = new Vector4(1.0f, 0.7f, 0.0f, 0.5f)
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
        if (material == null) {
            material = new Material(shader);
        }

        material.SetVector("boxBoundsMin", box.position - box.localScale / 2.0f);
        material.SetVector("boxBoundsMax", box.position + box.localScale / 2.0f);
        material.SetBuffer("pointBuffer", pointsBuffer);
        material.SetTexture("volumetricTexture", texture);

        Graphics.Blit(src, dest, material);
    }

}
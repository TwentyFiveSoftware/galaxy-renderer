using UnityEngine;
using Random = UnityEngine.Random;

public class ParticleSystem : MonoBehaviour {

    private struct Particle {
        public Vector3 position;
        public Vector3 velocity;
    }

    public int particleCount = 1000;

    public Material particleMaterial;

    public ComputeShader computeShader;

    private ComputeBuffer particleBuffer;

    private int computeShaderKernelID;

    private void Start() {
        Particle[] particles = new Particle[particleCount];

        for (int i = 0; i < particles.Length; ++i) {
            particles[i].position = new Vector3(Random.value * 2 - 1.0f, 0, Random.value * 2 - 1.0f);
            particles[i].velocity = new Vector3(Random.value * 0.2f - 0.1f, Random.value * 0.2f - 0.1f, Random.value * 0.2f - 0.1f);
        }

        particleBuffer = new ComputeBuffer(particleCount, sizeof(float) * 6);
        particleBuffer.SetData(particles);

        computeShaderKernelID = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(computeShaderKernelID, "particleBuffer", particleBuffer);

        particleMaterial.SetBuffer("particleBuffer", particleBuffer);
    }

    private void OnDestroy() {
        particleBuffer?.Release();
    }

    private void Update() {
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / 256.0f), 1, 1);
    }

    private void OnRenderObject() {
        particleMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, particleCount);
    }

}
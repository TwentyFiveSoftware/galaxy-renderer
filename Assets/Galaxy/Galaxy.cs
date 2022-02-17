using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour {

    public int starAmount = 20000;
    public int dustAmount = 20000;

    public Material galaxyMaterial;
    public bool generateGalaxyEveryFrame = false;

    public float bulgeRadius = 0.25f;
    public float galaxyRadius = 1.2f;
    public float farFieldFactor = 1.5f;

    public float ellipseA = 0.8f;
    public float ellipseB = 1.0f;
    public float ellipseTilt = 7.0f;

    public float particleSizeFactor = 1.0f;
    public float velocityFactor = 0.25f;
    public bool useConstantVelocity = false;

    public float maximumIntensity = 1.0f;
    public float bulgeIntensity = 0.1f;

    public float intensityCurveStart = 0.0f;
    public float intensityCurveEnd = 1.0f;
    public int intensityApproximationSteps = 1000;
    public float intensityAccuracy = 100.0f;

    private ComputeBuffer _galaxyBuffer;

    private struct GalaxyParticle {
        public float angularPosition;
        public float distanceToCenter;
        public float size;
        public Vector4 color;
        public int type; // 0 = star, 1 = dust
    }

    private void GenerateGalaxy() {
        GalaxyParticleDistribution.DistributionSettings starDistributionSettings =
            new GalaxyParticleDistribution.DistributionSettings {
                bulgeRadius = bulgeRadius,
                maximumIntensity = maximumIntensity,
                starFalloffModifier = galaxyRadius / 10.0f,
                bulgeIntensity = bulgeIntensity
            };

        List<float> intensityProbabilityDistribution =
            GalaxyParticleDistribution.CalculateIntensityProbabilityDistribution(intensityCurveStart, intensityCurveEnd,
                intensityApproximationSteps, intensityAccuracy, starDistributionSettings);

        GalaxyParticle[] galaxyParticles = new GalaxyParticle[starAmount + dustAmount];

        for (int i = 0; i < starAmount; ++i) {
            float distanceToCenter =
                GalaxyParticleDistribution.SelectRandomValueBasedOnProbabilityDistribution(
                    intensityProbabilityDistribution) * farFieldFactor * galaxyRadius;

            galaxyParticles[i] = new GalaxyParticle {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                distanceToCenter = distanceToCenter,
                size = 0.4f + Random.value * 1.6f,
                color = StarTemperature.CalculateColorFromTemperature(3000.0f + Random.value * 5000.0f),
                type = 1
            };
        }

        _galaxyBuffer?.Release();
        _galaxyBuffer = new ComputeBuffer(starAmount + dustAmount, sizeof(float) * 8);
        _galaxyBuffer.SetData(galaxyParticles);

        UpdateShaderVariables();
        galaxyMaterial.SetBuffer("galaxy_buffer", _galaxyBuffer);
    }

    private void UpdateShaderVariables() {
        galaxyMaterial.SetFloat("bulge_radius", bulgeRadius);
        galaxyMaterial.SetFloat("galaxy_radius", galaxyRadius);
        galaxyMaterial.SetFloat("far_field_factor", farFieldFactor);
        galaxyMaterial.SetFloat("ellipse_a", ellipseA);
        galaxyMaterial.SetFloat("ellipse_b", ellipseB);
        galaxyMaterial.SetFloat("ellipse_tilt", ellipseTilt);
        galaxyMaterial.SetFloat("particle_size_factor", particleSizeFactor);
        galaxyMaterial.SetFloat("velocity_factor", velocityFactor);
        galaxyMaterial.SetInt("use_constant_velocity", useConstantVelocity ? 1 : 0);
        galaxyMaterial.SetFloat("time", Time.time);
        galaxyMaterial.SetVector("position_offset", transform.position);
    }

    private void Start() {
        GenerateGalaxy();
    }

    private void Update() {
        if (generateGalaxyEveryFrame || (_galaxyBuffer != null && (starAmount + dustAmount) != _galaxyBuffer.count))
            GenerateGalaxy();

        UpdateShaderVariables();
    }

    private void OnDestroy() {
        _galaxyBuffer?.Release();
    }

    private void OnRenderObject() {
        galaxyMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, _galaxyBuffer.count);
    }

}
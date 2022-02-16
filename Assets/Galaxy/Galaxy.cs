using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour {

    public int starAmount = 1000;
    public Material galaxyMaterial;
    public bool generateGalaxyEveryFrame = false;

    public float bulgeRadius = 0.25f;
    public float galaxyRadius = 1.2f;
    public float farFieldFactor = 1.5f;

    public float ellipseA = 0.8f;
    public float ellipseB = 1.0f;
    public float ellipseTilt = 7.0f;

    public float starSize = 5.0f;
    public float velocityFactor = 0.25f;
    public bool useConstantVelocity = false;

    public float maximumIntensity = 1.0f;
    public float bulgeIntensity = 0.1f;

    public float intensityCurveStart = 0.0f;
    public float intensityCurveEnd = 1.0f;
    public int intensityApproximationSteps = 1000;
    public float intensityAccuracy = 100.0f;

    private ComputeBuffer _starBuffer;

    private void GenerateGalaxy() {
        StarDistribution.StarDistributionSettings starDistributionSettings =
            new StarDistribution.StarDistributionSettings {
                bulgeRadius = bulgeRadius,
                maximumIntensity = maximumIntensity,
                starFalloffModifier = galaxyRadius / 10.0f,
                bulgeIntensity = bulgeIntensity
            };

        List<float> intensityProbabilityDistribution = StarDistribution.CalculateIntensityProbabilityDistribution(
            intensityCurveStart, intensityCurveEnd, intensityApproximationSteps, intensityAccuracy,
            starDistributionSettings);

        Star[] stars = new Star[starAmount];

        for (int i = 0; i < starAmount; ++i) {
            float distanceToCenter =
                StarDistribution.SelectRandomValueBasedOnProbabilityDistribution(intensityProbabilityDistribution) *
                farFieldFactor * galaxyRadius;

            stars[i] = new Star {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                distanceToCenter = distanceToCenter,
                color = StarTemperature.CalculateColorFromTemperature(3000.0f + Random.value * 5000.0f)
            };
        }

        _starBuffer?.Release();
        _starBuffer = new ComputeBuffer(starAmount, sizeof(float) * 6);
        _starBuffer.SetData(stars);

        UpdateShaderVariables();
        galaxyMaterial.SetBuffer("star_buffer", _starBuffer);
    }

    private void UpdateShaderVariables() {
        galaxyMaterial.SetFloat("bulge_radius", bulgeRadius);
        galaxyMaterial.SetFloat("galaxy_radius", galaxyRadius);
        galaxyMaterial.SetFloat("far_field_factor", farFieldFactor);
        galaxyMaterial.SetFloat("ellipse_a", ellipseA);
        galaxyMaterial.SetFloat("ellipse_b", ellipseB);
        galaxyMaterial.SetFloat("ellipse_tilt", ellipseTilt);
        galaxyMaterial.SetFloat("star_size", starSize);
        galaxyMaterial.SetFloat("velocity_factor", velocityFactor);
        galaxyMaterial.SetInt("use_constant_velocity", useConstantVelocity ? 1 : 0);
        galaxyMaterial.SetFloat("time", Time.time);
        galaxyMaterial.SetVector("position_offset", transform.position);
    }

    private void Start() {
        GenerateGalaxy();
    }

    private void Update() {
        if (generateGalaxyEveryFrame || (_starBuffer != null && starAmount != _starBuffer.count))
            GenerateGalaxy();

        UpdateShaderVariables();
    }

    private void OnDestroy() {
        _starBuffer?.Release();
    }

    private void OnRenderObject() {
        galaxyMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, starAmount);
    }

}
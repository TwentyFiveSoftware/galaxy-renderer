using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Galaxy : MonoBehaviour {

    public int starAmount = 60000;
    public int dustAmount = 60000;
    public int dustFilamentAmount = 60000;

    public Material galaxyMaterial;
    public bool generateGalaxyEveryFrame = false;

    public float bulgeRadius = 0.1f;
    public float galaxyRadius = 1.0f;
    public float farFieldFactor = 1.5f;

    public float yOffsetFactor = 0.05f;

    public float ellipseA = 0.85f;
    public float ellipseB = 1.0f;
    public float ellipseTilt = -8.0f;

    public float ellipseDisturbanceNumber = 0.0f;
    public float ellipseDisturbanceDampingFactor = 0.0f;

    public float particleSizeFactor = 1.0f;
    public float starSizeFactor = 3.0f;
    public float dustSizeFactor = 300.0f;
    public float dustFilamentSizeFactor = 1.0f;

    public float largerStarFraction = 0.015f;

    public float dustTransparency = 0.05f;
    public float dustFilamentTransparency = 0.07f;

    public float dustTransparencyCurveFactor = 0.9f;
    public float dustFilamentTransparencyCurveFactor = 0.9f;

    public float dustBaseKelvin = 4000.0f;
    public float dustKelvinExponent = 3.0f;

    public float velocityFactor = 0.25f;
    public bool useConstantVelocity = false;

    public float maximumIntensity = 1.0f;
    public float bulgeIntensity = 0.02f;

    public float intensityCurveStart = 0.0f;
    public float intensityCurveEnd = 1.0f;
    public int intensityApproximationSteps = 1000;
    public float intensityAccuracy = 100.0f;

    private ComputeBuffer _galaxyBuffer;

    private struct GalaxyParticle {
        public float angularPosition;
        public float distanceToCenter;
        public float size;
        public float yOffset;
        public Vector4 color;
        public int type; // 0 = star, 1 = dust, 2 = dust filament
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

        GalaxyParticle[] galaxyParticles = new GalaxyParticle[starAmount + dustAmount + dustFilamentAmount];

        for (int i = 0; i < starAmount; ++i) {
            float distanceToCenter =
                GalaxyParticleDistribution.SelectRandomValueBasedOnProbabilityDistribution(
                    intensityProbabilityDistribution) * farFieldFactor * galaxyRadius;

            float size = 0.1f + Random.value * 0.4f;
            float yOffset = Random.value * yOffsetFactor;

            galaxyParticles[i] = new GalaxyParticle {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                distanceToCenter = distanceToCenter,
                color = StarTemperature.CalculateColorFromTemperature(3000.0f + Random.value * 5000.0f),
                size = i < starAmount * largerStarFraction ? 2 * size : size,
                yOffset = yOffset,
                type = 0
            };
        }

        for (int i = 0; i < dustAmount; ++i) {
            float distanceToCenter = Random.value * galaxyRadius;

            float yOffset = Random.value * yOffsetFactor;

            Color color = Random.value < 0.5f
                ? Color.Lerp(new Color(0.32f, 0.49f, 0.82f, 1.0f), new Color(0.05f, 0.10f, 0.39f, 1.0f), Random.value)
                : Color.Lerp(new Color(0.34f, 0.23f, 0.67f, 1.0f), new Color(0.63f, 0.44f, 0.80f, 1.0f), Random.value);

            galaxyParticles[starAmount + i] = new GalaxyParticle {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                distanceToCenter = distanceToCenter,
                size = 0.02f + Random.value * 0.15f,
                color = new Vector4(color.r, color.g, color.b, 1),
                yOffset = yOffset,
                type = 1
            };
        }

        for (int i = 0; i < Mathf.FloorToInt(dustFilamentAmount / 100.0f); ++i) {
            float distanceToCenter = Random.value * galaxyRadius;
            float angularPosition = Random.value * 360.0f;
            float kelvin = Mathf.Min(20000.0f,
                dustBaseKelvin * Mathf.Exp(distanceToCenter * distanceToCenter * dustKelvinExponent));

            float yOffset = Random.value * yOffsetFactor;

            for (int j = 0; j < 100; j++) {
                distanceToCenter = distanceToCenter - 0.05f + 0.1f * Random.value;

                galaxyParticles[starAmount + dustAmount + i * 100 + j] = new GalaxyParticle {
                    angularPosition = (angularPosition - 10.0f + 20.0f * Random.value) * Mathf.Deg2Rad,
                    distanceToCenter = distanceToCenter,
                    size = 0.1f + Random.value * 0.075f,
                    color = StarTemperature.CalculateColorFromTemperature(kelvin),
                    yOffset = yOffset,
                    type = 2
                };
            }
        }

        _galaxyBuffer?.Release();
        _galaxyBuffer = new ComputeBuffer(starAmount + dustAmount + dustFilamentAmount, sizeof(float) * 9);
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
        galaxyMaterial.SetFloat("ellipse_disturbance_number", ellipseDisturbanceNumber);
        galaxyMaterial.SetFloat("ellipse_disturbance_dumping_factor", ellipseDisturbanceDampingFactor);
        galaxyMaterial.SetFloat("particle_size_factor", particleSizeFactor);
        galaxyMaterial.SetFloat("star_size_factor", starSizeFactor);
        galaxyMaterial.SetFloat("dust_size_factor", dustSizeFactor);
        galaxyMaterial.SetFloat("dust_filament_size_factor", dustFilamentSizeFactor);
        galaxyMaterial.SetFloat("dust_transparency", dustTransparency);
        galaxyMaterial.SetFloat("dust_filament_transparency", dustFilamentTransparency);
        galaxyMaterial.SetFloat("dust_transparency_curve_factor", dustTransparencyCurveFactor);
        galaxyMaterial.SetFloat("dust_filament_transparency_curve_factor", dustFilamentTransparencyCurveFactor);
        galaxyMaterial.SetFloat("velocity_factor", velocityFactor);
        galaxyMaterial.SetInt("use_constant_velocity", useConstantVelocity ? 1 : 0);
        galaxyMaterial.SetFloat("time", Time.time);
        galaxyMaterial.SetVector("position_offset", transform.position);
    }

    private void OnEnable() {
        GenerateGalaxy();
    }

    private void Start() {
        GenerateGalaxy();
    }

    private void Update() {
        if (generateGalaxyEveryFrame || _galaxyBuffer == null ||
            starAmount + dustAmount + dustFilamentAmount != _galaxyBuffer.count)
            GenerateGalaxy();

        UpdateShaderVariables();
    }

    private void OnDestroy() {
        _galaxyBuffer?.Release();
    }

    private void OnRenderObject() {
        if (_galaxyBuffer == null || _galaxyBuffer.count == 0)
            return;

        galaxyMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, _galaxyBuffer.count);
    }

}
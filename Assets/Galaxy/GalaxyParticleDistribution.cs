using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GalaxyParticleDistribution {

    public struct DistributionSettings {
        public float bulgeRadius;
        public float maximumIntensity;
        public float starFalloffModifier; // lower = less stars further away from disk
        public float bulgeIntensity; // lower = higher intensity in bulge
    }

    public static float SelectRandomValueBasedOnProbabilityDistribution(List<float> distribution) {
        return distribution[Mathf.FloorToInt(Random.value * (distribution.Count - 1))];
    }

    public static List<float> CalculateIntensityProbabilityDistribution(float intensityCurveStart,
        float intensityCurveEnd, int approximationSteps, float accuracy,
        DistributionSettings distributionSettings) {
        float stepDelta = (intensityCurveEnd - intensityCurveStart) / approximationSteps;

        List<float> values = new List<float>();

        for (int step = 1; step < approximationSteps; ++step) {
            float x = intensityCurveStart + step * stepDelta;
            float y = CalculateIntensityAtRadius(x, distributionSettings);

            for (int i = 0; i < Mathf.CeilToInt(y * accuracy); ++i) {
                values.Add(x);
            }
        }

        return values;
    }

    private static float CalculateIntensityAtRadius(float radius, DistributionSettings distributionSettings) {
        if (radius < distributionSettings.bulgeRadius)
            return CalculateBulgeIntensity(radius, distributionSettings);

        return CalculateBulgeIntensity(distributionSettings.bulgeRadius, distributionSettings) *
               Mathf.Exp(-(radius - distributionSettings.bulgeRadius) / distributionSettings.starFalloffModifier);
    }

    private static float CalculateBulgeIntensity(float radius, DistributionSettings distributionSettings) {
        return distributionSettings.maximumIntensity *
               Mathf.Exp(-distributionSettings.bulgeIntensity * Mathf.Pow(radius, 0.25f));
    }

}
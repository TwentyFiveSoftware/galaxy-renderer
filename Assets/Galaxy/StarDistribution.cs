using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StarDistribution {

    public struct StarDistributionSettings {
        public float bulgeRadius;
        public float maximumIntensity;
        public float discScaleLength;
        public float bulgeIntensityK;
    }

    public static float SelectRandomValueBasedOnProbabilityDistribution(List<float> distribution) {
        return distribution[(int)(Random.value * distribution.Count)];
    }

    public static List<float> CalculateIntensityProbabilityDistribution(float intensityCurveStart,
        float intensityCurveEnd, int approximationSteps, float accuracy,
        StarDistributionSettings distributionSettings) {
        float stepDelta = (intensityCurveEnd - intensityCurveStart) / approximationSteps;

        // float[] cumulativeDistributionFunction = new float[approximationSteps];
        // cumulativeDistributionFunction[0] = 0.0f;
        //
        // float cumulativeY = 0.0f;
        //
        // for (int step = 1; step < cumulativeDistributionFunction.Length; ++step) {
        //     float x = intensityCurveStart + step * stepDelta;
        //     cumulativeY += CalculateIntensityAtRadius(x, distributionSettings) * stepDelta;
        //     cumulativeDistributionFunction[step] = cumulativeY;
        // }
        //
        // float maxCumulativeY = cumulativeDistributionFunction[^1];
        // for (int x = 0; x < cumulativeDistributionFunction.Length; ++x) {
        //     cumulativeDistributionFunction[x] /= maxCumulativeY;
        // }
        //
        // List<float> values = new List<float>();
        // for (int step = 1; step < cumulativeDistributionFunction.Length; ++step) {
        //     float x = intensityCurveStart + step * stepDelta;
        //     float dy = cumulativeDistributionFunction[step] - cumulativeDistributionFunction[step - 1];
        //
        //     for (int i = 0; i < Mathf.CeilToInt(dy * accuracy); ++i) {
        //         values.Add(x);
        //     }
        // }
        
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

    private static float CalculateIntensityAtRadius(float radius, StarDistributionSettings distributionSettings) {
        if (radius < distributionSettings.bulgeRadius)
            return CalculateBulgeIntensity(radius, distributionSettings);

        return CalculateBulgeIntensity(distributionSettings.bulgeRadius, distributionSettings) *
               Mathf.Exp(-(radius - distributionSettings.bulgeRadius) / distributionSettings.discScaleLength);
    }

    private static float CalculateBulgeIntensity(float radius, StarDistributionSettings distributionSettings) {
        return distributionSettings.maximumIntensity *
               Mathf.Exp(-distributionSettings.bulgeIntensityK * Mathf.Pow(radius, 0.25f));
    }

}
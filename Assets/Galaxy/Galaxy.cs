using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Galaxy {

    private readonly Star[] _stars;

    public Galaxy(int starAmount, bool useLinearStarDistribution) {
        _stars = new Star[starAmount];

        StarDistribution.StarDistributionSettings starDistributionSettings =
            new StarDistribution.StarDistributionSettings {
                bulgeRadius = 0.25f, maximumIntensity = 1.0f, bulgeIntensityK = 0.2f, discScaleLength = 0.4f
            };

        List<float> intensityProbabilityDistribution =
            StarDistribution.CalculateIntensityProbabilityDistribution(0, 1, 1000, 1000.0f, starDistributionSettings);

        Debug.Log(intensityProbabilityDistribution.Count);
        
        float[] y = new float[20];

        for (int i = 0; i < 1000000; i++) {
            float v = StarDistribution.SelectRandomValueBasedOnProbabilityDistribution(
                intensityProbabilityDistribution);
            y[Mathf.FloorToInt(v * 20)] += 1.0f / 1000000;
        }
        
        Debug.Log(String.Join(", ", y));


        for (int i = 0; i < _stars.Length; ++i) {
            float distanceToBulge = useLinearStarDistribution
                ? i / (float)starAmount
                : StarDistribution.SelectRandomValueBasedOnProbabilityDistribution(intensityProbabilityDistribution);

            _stars[i] = new Star {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                angularVelocity = 0.1f,
                ellipseTiltAngle = distanceToBulge * -6.0f,
                ellipseA = distanceToBulge * 0.72f,
                ellipseB = distanceToBulge * -0.6f,
                size = 2.0f,
                color = new Vector4(1.0f, 1.0f, 1.0f, 0.5f)
            };

            // // VISIBLE ELLIPSES
            // int ellipseAmount = 30;
            // _stars[i] = new Star {
            //     angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
            //     angularVelocity = 0.1f,
            //     ellipseTiltAngle = Mathf.FloorToInt(i / ((float)starAmount / ellipseAmount)) * -0.2f,
            //     ellipseA = Mathf.FloorToInt(i / ((float)starAmount / ellipseAmount)) * 0.024f,
            //     ellipseB = Mathf.FloorToInt(i / ((float)starAmount / ellipseAmount)) * -0.02f,
            //     size = 2.0f,
            //     color = new Vector4(1.0f, 1.0f, 1.0f, 0.5f)
            // };
        }
    }

    public Star[] Stars => _stars;

}
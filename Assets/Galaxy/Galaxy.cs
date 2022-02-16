﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Galaxy {

    private readonly Star[] _stars;

    public Galaxy(int starAmount) {
        _stars = new Star[starAmount];

        float bulgeRadius = 0.25f;
        float galaxyRadius = 1.2f;
        float farFieldRadius = 1.5f * galaxyRadius;

        float ellipseA = 0.8f;
        float ellipseB = 1.0f;
        float ellipseTilt = 7.0f;


        StarDistribution.StarDistributionSettings starDistributionSettings =
            new StarDistribution.StarDistributionSettings {
                bulgeRadius = bulgeRadius,
                maximumIntensity = 1.0f,
                bulgeIntensityK = 0.2f,
                discScaleLength = galaxyRadius / 3.0f
            };

        List<float> intensityProbabilityDistribution =
            StarDistribution.CalculateIntensityProbabilityDistribution(0, 1, 1000, 100.0f, starDistributionSettings);

        // float[] y = new float[10];
        //
        // for (int i = 0; i < 10000; i++) {
        //     float v = StarDistribution.SelectRandomValueBasedOnProbabilityDistribution(
        //         intensityProbabilityDistribution);
        //     y[Mathf.FloorToInt(v * 10)] += 1.0f / 10000;
        // }

        // Debug.Log(intensityProbabilityDistribution.Count);
        // Debug.Log(String.Join(", ", y));


        // float[] e = new float[20];
        // for (int i = 0; i < 20; i++) {
        //     e[i] = getEccentricity((i * farFieldRadius) / 20.0f, bulgeRadius, galaxyRadius, farFieldRadius, 0.8f, 1.0f);
        // }
        //
        // Debug.Log(String.Join(", ", e));

        for (int i = 0; i < _stars.Length; ++i) {
            float distanceToCenter =
                StarDistribution.SelectRandomValueBasedOnProbabilityDistribution(intensityProbabilityDistribution) *
                farFieldRadius;

            _stars[i] = new Star {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                angularVelocity = 0.1f,
                ellipseTiltAngle = distanceToCenter * ellipseTilt,
                // ellipseTiltAngle = 0.0f,
                // ellipseA = 0.0f,
                // ellipseB = 0.25f,
                // ellipseA = distanceToCenter * 0.72f,
                // ellipseB = distanceToCenter * 0.6f,
                ellipseA = distanceToCenter,
                // ellipseB = distanceToCenter * 2.0f,
                ellipseB = distanceToCenter * getEccentricity(distanceToCenter, bulgeRadius, galaxyRadius,
                    farFieldRadius, ellipseA, ellipseB),
                size = 5.0f,
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

    private float getEccentricity(float distanceToCenter, float bulgeRadius, float galaxyRadius, float farFieldRadius,
        float ellipseA, float ellipseB) {
        if (distanceToCenter < bulgeRadius)
            return 1.0f + (distanceToCenter / bulgeRadius) * (ellipseA - 1.0f);

        if (distanceToCenter < galaxyRadius)
            return ellipseA + (distanceToCenter - bulgeRadius) / (galaxyRadius - bulgeRadius) * (ellipseB - ellipseA);

        if (distanceToCenter < farFieldRadius)
            return ellipseB + (distanceToCenter - galaxyRadius) / (farFieldRadius - galaxyRadius) * (1.0f - ellipseB);

        return 1.0f;
    }

}
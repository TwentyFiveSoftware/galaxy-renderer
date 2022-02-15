using UnityEngine;

public class Galaxy {

    private readonly Star[] _stars;

    public Galaxy(int starAmount) {
        _stars = new Star[starAmount];

        int ellipseAmount = 30;

        for (int i = 0; i < _stars.Length; ++i) {
            _stars[i] = new Star {
                angularPosition = Random.value * 360.0f * Mathf.Deg2Rad,
                angularVelocity = 0.1f,
                ellipseTiltAngle = Mathf.FloorToInt(i / ((float)starAmount / ellipseAmount)) * -0.2f,
                // ellipseTiltAngle = 0.0f,
                ellipseA = Mathf.FloorToInt(i / ((float)starAmount / ellipseAmount)) * 0.024f,
                ellipseB = Mathf.FloorToInt(i / ((float)starAmount / ellipseAmount)) * -0.02f,
                size = 2.0f,
                color = new Vector4(1.0f, 1.0f, 1.0f, 0.5f)
            };
        }
    }

    public Star[] Stars => _stars;

}
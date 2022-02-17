using UnityEngine;

public class StarTemperature {

    // https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html
    public static Vector4 CalculateColorFromTemperature(float kelvin) {
        kelvin = Mathf.Clamp(kelvin, 1000.0f, 40000.0f) / 100.0f;

        int r = kelvin < 66.0f
            ? 255
            : Mathf.Clamp(Mathf.FloorToInt(329.698727446f * Mathf.Pow(kelvin - 60.0f, -0.1332047592f)), 0, 255);

        int g = Mathf.Clamp(
            kelvin < 66.0f
                ? Mathf.FloorToInt(99.4708025861f * Mathf.Log(kelvin) - 161.1195681661f)
                : Mathf.FloorToInt(288.1221695283f * Mathf.Pow(kelvin - 60, -0.0755148492f)), 0, 255);

        int b = kelvin >= 66 ? 255 :
            kelvin < 19 ? 0 :
            Mathf.Clamp(Mathf.FloorToInt(138.5177312231f * Mathf.Log(kelvin - 10.0f) - 305.0447927307f), 0, 255);

        return new Vector4(r, g, b, 255) / 255.0f;
    }

}
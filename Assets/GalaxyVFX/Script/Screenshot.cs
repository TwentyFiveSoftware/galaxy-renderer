using System;
using UnityEditor;
using UnityEngine;

public class Screenshot : MonoBehaviour {

    public void CaptureScreenshot() {
        ScreenCapture.CaptureScreenshot("screenshot_" + DateTime.Now.ToFileTime() + ".png");
    }

}

[CustomEditor(typeof(Screenshot))]
public class ScreenshotEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Screenshot")) {
            ((Screenshot)target).CaptureScreenshot();
        }
    }
}
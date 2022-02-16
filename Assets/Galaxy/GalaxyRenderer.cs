using UnityEngine;

public class GalaxyRenderer : MonoBehaviour {

    public int starAmount = 1000;
    public Material galaxyMaterial;

    private ComputeBuffer _starBuffer;

    private void Start() {
        Galaxy galaxy = new Galaxy(starAmount);

        _starBuffer = new ComputeBuffer(starAmount, sizeof(float) * 10);
        _starBuffer.SetData(galaxy.Stars);

        galaxyMaterial.SetFloat("time", 0.0f);
        galaxyMaterial.SetVector("positionOffset", transform.position);
        galaxyMaterial.SetBuffer("starBuffer", _starBuffer);
    }

    private void Update() {
        galaxyMaterial.SetFloat("time", Time.time);
    }

    private void OnDestroy() {
        _starBuffer?.Release();
    }

    private void OnRenderObject() {
        galaxyMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, starAmount);
    }

}
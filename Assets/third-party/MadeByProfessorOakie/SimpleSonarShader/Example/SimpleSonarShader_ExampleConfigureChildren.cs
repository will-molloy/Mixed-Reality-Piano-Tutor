// SimpleSonarShader scripts and shaders were written by Drew Okenfuss.

using UnityEngine;

public class SimpleSonarShader_ExampleConfigureChildren : MonoBehaviour
{
    public Material SonarMaterial;

    private void Start()
    {
        foreach (var col in GetComponentsInChildren<Collider>(true))
            col.gameObject.AddComponent<SimpleSonarShader_ExampleCollision>();

        foreach (var rend in GetComponentsInChildren<Renderer>(true))
        {
            var mainTex = rend.material.mainTexture;
            rend.material = SonarMaterial;
            rend.material.mainTexture = mainTex;
        }
    }
}
// SimpleSonarShader scripts and shaders were written by Drew Okenfuss.

using UnityEngine;

public class SimpleSonarShader_ExampleConfigureChildren : MonoBehaviour {

    public Material SonarMaterial;

    private void Start()
    {
        foreach(Collider col in GetComponentsInChildren<Collider>(true))
        {
            col.gameObject.AddComponent<SimpleSonarShader_ExampleCollision>();
        }

        foreach(Renderer rend in GetComponentsInChildren<Renderer>(true))
        {
            Texture mainTex = rend.material.mainTexture;
            rend.material = SonarMaterial;
            rend.material.mainTexture = mainTex;
        }
    }

}

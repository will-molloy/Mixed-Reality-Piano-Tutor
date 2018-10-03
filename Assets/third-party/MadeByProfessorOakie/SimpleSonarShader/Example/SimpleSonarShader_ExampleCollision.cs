// SimpleSonarShader scripts and shaders were written by Drew Okenfuss.

using UnityEngine;

public class SimpleSonarShader_ExampleCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Start sonar ring from the contact point
        var parent = GetComponentInParent<SimpleSonarShader_Parent>();
        if (parent) parent.StartSonarRing(collision.contacts[0].point, collision.impulse.magnitude / 10.0f);
    }
}
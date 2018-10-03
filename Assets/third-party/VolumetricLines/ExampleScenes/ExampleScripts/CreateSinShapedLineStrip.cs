using UnityEngine;
using VolumetricLines;

/// <summary>
///     Create a sin shaped line strip using a volumetric line strip
/// </summary>
public class CreateSinShapedLineStrip : MonoBehaviour
{
    public Color m_color;
    public float m_end = Mathf.PI;
    public int m_numVertices = 50;
    public float m_start;
    public Material m_volumetricLineStripMaterial;

    // Use this for initialization
    private void Start()
    {
        // Create an empty game object
        var go = new GameObject();
        go.transform.parent = transform;

        // Add the MeshFilter component, VolumetricLineStripBehavior requires it
        go.AddComponent<MeshFilter>();

        // Add a MeshRenderer, VolumetricLineStripBehavior requires it
        go.AddComponent<MeshRenderer>();

        // Add the VolumetricLineStripBehavior and set parameters, like color and all the vertices of the line
        var volLineStrip = go.AddComponent<VolumetricLineStripBehavior>();
        volLineStrip.DoNotOverwriteTemplateMaterialProperties = false;
        volLineStrip.TemplateMaterial = m_volumetricLineStripMaterial;
        volLineStrip.LineColor = m_color;
        volLineStrip.LineWidth = 55.0f;
        volLineStrip.LightSaberFactor = 0.83f;

        var lineVertices = new Vector3[m_numVertices];
        for (var i = 0; i < m_numVertices; ++i)
        {
            var x = Mathf.Lerp(m_start, m_end, i / (float) (m_numVertices - 1));
            var y = Mathf.Sin(x);
            lineVertices[i] = gameObject.transform.TransformPoint(new Vector3(x, y, 0f));
        }

        volLineStrip.UpdateLineVertices(lineVertices);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (var i = 0; i < m_numVertices; ++i)
        {
            var x = Mathf.Lerp(m_start, m_end, i / (float) (m_numVertices - 1));
            var y = Mathf.Sin(x);
            Gizmos.DrawSphere(gameObject.transform.TransformPoint(new Vector3(x, y, 0f)), 5f);
        }
    }
}
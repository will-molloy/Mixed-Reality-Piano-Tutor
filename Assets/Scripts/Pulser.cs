using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulser : MonoBehaviour
{
    public void Pulse()
    {
        StartCoroutine(PulseCoroutine());
    }

    private IEnumerator PulseCoroutine()
    {
        var obj = Instantiate(this);
        obj.transform.position = this.transform.position;
        var renderer = obj.GetComponent<MeshRenderer>();
        var alpha = 1f;
        var scale = 0.01f;
        var color = renderer.material.color;
        color = new Color(color.r, color.g, color.b, 1f);
        for (int i = 0; i < 1000; i++)
        {
            yield return new WaitForSeconds(0.001f);
            renderer.material.color = new Color(color.r, color.g, color.b, Mathf.Max(alpha, 0.0f));
            var lscale = obj.transform.localScale;
            obj.transform.localScale = lscale + new Vector3(scale, scale, scale);

            alpha -= 0.05f;
            scale += 0.01f;
        }
        Destroy(obj);
    }
}

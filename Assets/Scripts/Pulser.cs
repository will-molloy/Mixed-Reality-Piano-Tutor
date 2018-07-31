using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpaceCraftControl))]
public class Pulser : MonoBehaviour
{

    private float fillUpPercent = 0f;
    private GameObject fillUpObj;
    public static Vector3 fillupVect = new Vector3(1.0001f, 1f, 1.0001f);
    public static Vector3 fillupVect0 = new Vector3(1.0001f, 0f, 1.0001f);
    public void Pulse()
    {
        StartCoroutine(PulseCoroutine());
    }

    public void FillUp(float percent) {
        Debug.Log("Fill up" + percent);
        if (fillUpObj == null) {
            fillUpObj = Instantiate(this.gameObject);
            fillUpObj.transform.SetParent(this.transform);
            fillUpObj.transform.localScale = fillupVect;
            var rend = fillUpObj.GetComponent<MeshRenderer>();
            rend.material.color = Color.blue;
        }
        var newPercent = fillUpPercent + percent;
        if (newPercent > 1.0f) {
            // Fire laser
        }
        StartCoroutine(AnimateScale(newPercent, 0.1f, fillUpObj));
        fillUpPercent = newPercent;
    }

    private IEnumerator AnimateScale(float newPercent, float speed, GameObject obj) {
        if (speed > 0f) {
            var frac = 0f;
            while(frac < 1f) {
                obj.transform.localScale = Vector3.Lerp(fillupVect, new Vector3(1.0001f, newPercent, 1.0001f), frac);
                frac += speed;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void FireLaser() {
            GetComponent<SpaceCraftControl>().StagedDestory();
    }

    private IEnumerator PulseCoroutine()
    {
        var obj = Instantiate(this.gameObject);
        obj.transform.position = this.transform.position;
        //obj.transform.SetParent(this.transform);
        //obj.transform.rotation = this.transform.rotation;
        var renderer = obj.GetComponent<MeshRenderer>();
        var alpha = 1f;
        var scale = 0.01f;
        var color = renderer.material.color;
        color = new Color(color.r, color.g, color.b, 1f);
        for (int i = 0; i < 50; i++)
        {
            yield return new WaitForSeconds(0.001f);
            renderer.material.color = new Color(color.r, color.g, color.b, Mathf.Max(alpha, 0.0f));
            var lscale = obj.transform.localScale;

            if(i < 25) {
                alpha -= 0.08f;
                //scale += 0.01f;
                obj.transform.localScale = lscale + new Vector3(scale, scale, scale);
            } else {
                alpha += 0.08f;
                //scale -= 0.01f;
                obj.transform.localScale = lscale - new Vector3(scale, scale, scale);
            }
        }
        GameObject.DestroyObject(obj);
        GameObject.DestroyImmediate(obj);
        GameObject.Destroy(obj);
    }
}

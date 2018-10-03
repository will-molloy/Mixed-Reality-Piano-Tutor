using System.Collections;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     - Pulser and energy bar objects which animates every beat
    /// </summary>
    [RequireComponent(typeof(SpaceCraftControl))]
    public class Pulser : MonoBehaviour
    {
        public static Vector3 fillupVect = new Vector3(1.0001f, 1f, 1.0001f);
        private GameObject fillUpObj;

        private float fillUpPercent;

        public void Pulse()
        {
            StartCoroutine(PulseCoroutine());
        }

        public void FillUp(float percent)
        {
            Debug.Log("Fill up" + percent);
            if (fillUpObj == null)
            {
                fillUpObj = Instantiate(gameObject);
                fillUpObj.transform.SetParent(transform);
                fillUpObj.transform.localScale = fillupVect;
                var rend = fillUpObj.GetComponent<MeshRenderer>();
                rend.material.color = Color.blue;
            }

            var newPercent = fillUpPercent + percent;

            StartCoroutine(AnimateScale(newPercent, 0.1f, fillUpObj));
            fillUpPercent = newPercent;
        }

        private IEnumerator AnimateScale(float newPercent, float speed, GameObject obj)
        {
            if (speed > 0f)
            {
                var frac = 0f;
                while (frac < 1f)
                {
                    obj.transform.localScale = Vector3.Lerp(fillupVect, new Vector3(1.0001f, newPercent, 1.0001f), frac);
                    frac += speed;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        private IEnumerator PulseCoroutine()
        {
            var obj = Instantiate(gameObject);
            obj.transform.position = transform.position;
            var renderer = obj.GetComponent<MeshRenderer>();
            var alpha = 1f;
            var scale = 0.01f;
            var color = renderer.material.color;
            color = new Color(color.r, color.g, color.b, 1f);
            for (var i = 0; i < 50; i++)
            {
                yield return new WaitForSeconds(0.001f);
                renderer.material.color = new Color(color.r, color.g, color.b, Mathf.Max(alpha, 0.0f));
                var lscale = obj.transform.localScale;

                if (i < 25)
                {
                    alpha -= 0.08f;
                    obj.transform.localScale = lscale + new Vector3(scale, scale, scale);
                }
                else
                {
                    alpha += 0.08f;
                    obj.transform.localScale = lscale - new Vector3(scale, scale, scale);
                }
            }

            DestroyObject(obj);
            DestroyImmediate(obj);
            Destroy(obj);
        }
    }
}
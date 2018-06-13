using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PianoCameraHookScript : MonoBehaviour
{
    internal bool isScriptEnabled = true;

    void Update()
    {
        if (!isScriptEnabled) { return; }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var cam = GetComponent<Camera>();
            PianoBuilder.instance.PlacePianoInfrontOfTransform(cam.transform);
            isScriptEnabled = false;
        }

    }
}

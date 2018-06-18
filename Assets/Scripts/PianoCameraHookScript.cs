using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>  
/// - Hook ZED camera to Piano Builder
/// </summary> 
[RequireComponent(typeof(Camera))]
public class PianoCameraHookScript : MonoBehaviour
{
    public bool isScriptEnabled;
    
    void Update()
    {
        if (isScriptEnabled) { return; }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var cam = GetComponent<Camera>();
            PianoBuilder.instance.PlacePianoInfrontOfTransform(cam.transform);
            isScriptEnabled = true;
        }
    }
}

using UnityEngine;

/// <summary>
///     - Prints all webcam device names to console log
/// </summary>
public class WebcamDevicesHelper : MonoBehaviour
{
    private void Start()
    {
        var devices = WebCamTexture.devices;
        Debug.Log("Found " + devices.Length + " webcam device(s).");
        foreach (var d in devices) Debug.Log("Found: " + d.name);
    }
}
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

[RequireComponent(typeof(PianoBuilderOculusControllerHook))]
public class CalibrationController : MonoBehaviour
{

    [SerializeField]
    private string JSON_PATH = "Assets/Resources/piano-calibration.json";

    private PianoBuilderOculusControllerHook cameraHook;

    void Start()
    {
        cameraHook = GetComponent<PianoBuilderOculusControllerHook>();
        LoadMarker();
    }

    private void LoadMarker()
    {
        Debug.Log("Loading marker position from disk.");
        var marker = cameraHook.GetMarkerTransform();
        var json = File.ReadAllText(JSON_PATH);
        var dto = JsonConvert.DeserializeObject<PianoCalibrationDTO>(json);
        marker.localPosition = dto.markerPos;
        marker.localScale = dto.markerScale;
        marker.localEulerAngles = dto.markerEulerAngle;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            SaveMarker();
        }
    }

    private void SaveMarker()
    {
        var marker = cameraHook.GetMarkerTransform();
        Debug.Log("Saving marker position to disk." + marker.localPosition.ToString("F5"));
        var dto = new PianoCalibrationDTO(marker.localPosition, marker.localScale, marker.localEulerAngles);
        var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
        File.WriteAllText(JSON_PATH, json);
    }
}

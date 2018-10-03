using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Virtual_Piano;

namespace Calibration
{
    /// <summary>
    ///     - Saves (on backspace press) and loads (at start) PianoBuilder calibration to/from disk via JSON
    /// </summary>
    [RequireComponent(typeof(PianoBuilder))]
    [RequireComponent(typeof(PianoBuilderMarkerHook))]
    public class CalibrationController : MonoBehaviour
    {
        private PianoBuilder builder;

        private PianoBuilderMarkerHook cameraHook;

        [SerializeField] private string JSON_PATH = "Assets/Resources/piano-calibration.json";

        private bool loadedMarker;

        private void Start()
        {
            cameraHook = GetComponent<PianoBuilderMarkerHook>();
            builder = GetComponent<PianoBuilder>();
        }

        private void LoadMarker()
        {
            Debug.Log("Loading marker position from disk.");
            var marker = cameraHook.GetMarkerTransform();
            var json = File.ReadAllText(JSON_PATH);
            var dto = JsonConvert.DeserializeObject<PianoCalibrationDto>(json);
            marker.localPosition = dto.markerPos;
            marker.localScale = dto.markerScale;
            marker.localEulerAngles = dto.markerEulerAngle;
        }

        private void Update()
        {
            if (!loadedMarker)
            {
                LoadMarker();
                loadedMarker = true;
            }

            builder.transform.localEulerAngles = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.Backspace)) SaveMarker();
        }

        private void SaveMarker()
        {
            var marker = cameraHook.GetMarkerTransform();
            Debug.Log("Saving marker position to disk." + marker.localPosition.ToString("F5"));
            var dto = new PianoCalibrationDto(marker.localPosition, marker.localScale, marker.localEulerAngles);
            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            File.WriteAllText(JSON_PATH, json);
        }
    }
}
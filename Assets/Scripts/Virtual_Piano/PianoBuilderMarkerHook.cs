using UnityEngine;

namespace Virtual_Piano
{
    /// <summary>
    ///     - Builds Virtual Piano (PianoBuilder) at location of some marker (LeftMarker) otherwise centre of two markers
    ///         (represented by new marker object)
    ///     - Update method forces PianoBuilder to follow the chosen marker
    ///     - Also accepts keyboard input to
    ///         - Move, rotate, scale the marker and hence PianoBuilder
    ///         - Enable/disable ZED occlusion
    /// </summary>
    [RequireComponent(typeof(PianoBuilder))]
    public class PianoBuilderMarkerHook : MonoBehaviour
    {
        [SerializeField] private GameObject LeftMarker;

        private GameObject marker;
        private PianoBuilder PianoBuilder;

        [SerializeField] private GameObject RightMarker;

        [SerializeField] private bool twoMarkers;

        [SerializeField] private ZEDManager ZEDManager;

        public Transform GetMarkerTransform()
        {
            return marker.transform;
        }

        private void Start()
        {
            marker = new GameObject("Marker");
            if (!twoMarkers) marker.transform.SetParent(LeftMarker.transform);
            PianoBuilder = GetComponent<PianoBuilder>();
            PianoBuilder.BuildPianoAsChildOfTransform(marker.transform);
        }

        private void UpdatePosition()
        {
            var leftPos = LeftMarker.transform.position;
            var rightPos = RightMarker.transform.position;
            var markerPos = marker.transform.position;
            var whiteKeyWidth = PianoBuilder.GetKeyObj(PianoKeys.First()).transform.localScale.x;
            var pianoCenterXOffset = (whiteKeyWidth + PianoBuilder.pianoKeyGap) * 12;
            marker.transform.position = new Vector3((rightPos.x + leftPos.x) / 2 + pianoCenterXOffset, markerPos.y,
                (rightPos.z + leftPos.z) / 2);
        }

        private void Update()
        {
            if (twoMarkers) UpdatePosition();
            var scale = marker.transform.localScale;
            var position = marker.transform.position;

            if (Input.GetKey(KeyCode.Plus)) scale *= 1.001f;
            if (Input.GetKey(KeyCode.Minus)) scale /= 1.001f;
            if (Input.GetKey(KeyCode.A)) position -= marker.transform.right * 0.001f;
            if (Input.GetKey(KeyCode.D)) position += marker.transform.right * 0.001f;
            if (Input.GetKey(KeyCode.W)) position += marker.transform.forward * 0.001f;
            if (Input.GetKey(KeyCode.S)) position -= marker.transform.forward * 0.001f;
            if (Input.GetKey(KeyCode.Q)) position += marker.transform.up * 0.001f;
            if (Input.GetKey(KeyCode.E)) position -= marker.transform.up * 0.001f;
            marker.transform.localScale = scale;
            marker.transform.position = position;

            // Rotation enabled even if two marker
            if (Input.GetKey(KeyCode.UpArrow)) marker.transform.Rotate(Vector3.right * 0.1f);
            if (Input.GetKey(KeyCode.DownArrow)) marker.transform.Rotate(Vector3.left * 0.1f);
            if (Input.GetKey(KeyCode.Z)) marker.transform.Rotate(Vector3.down * 0.1f);
            if (Input.GetKey(KeyCode.X)) marker.transform.Rotate(Vector3.up * 0.1f);
            if (Input.GetKey(KeyCode.LeftArrow)) marker.transform.Rotate(Vector3.forward * 0.1f);
            if (Input.GetKey(KeyCode.RightArrow)) marker.transform.Rotate(Vector3.back * 0.1f);
        
            // Reset position 
            if (Input.GetKeyDown(KeyCode.R)) reset();
        
            // Enable/disable ZED occlusion
            if (Input.GetKeyDown(KeyCode.O))
            {
                ZEDManager.depthOcclusion = !ZEDManager.depthOcclusion;
                ZEDManager.setRenderingSettingsPublic();
            }
        }

        private void reset()
        {
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localEulerAngles = Vector3.zero;
            marker.transform.localScale = Vector3.one;
        }
    }
}
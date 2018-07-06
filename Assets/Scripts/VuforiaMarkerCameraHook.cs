using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(PianoBuilder))]
public class VuforiaMarkerCameraHook : MonoBehaviour
{
    private GameObject leftMarker;

    private PianoBuilder PianoBuilder;

    private Vector3 lastKnownMarkerPosition;

    void Start()
    {
        leftMarker = GameObject.Find("Cone (Left/Blue)");
        PianoBuilder = GetComponent<PianoBuilder>();
        PianoBuilder.PlacePianoInfrontOfTransform(leftMarker.transform);
    }

    void Update()
    {
        if (isTrackingMarker("Left ImageTarget"))
        {
            lastKnownMarkerPosition = leftMarker.transform.position;
        }
        // TODO offset; currently anchors leftmost key on marker.
        PianoBuilder.SetPosition(lastKnownMarkerPosition);
    }

    private bool isTrackingMarker(string imageTargetName)
    {
        var imageTarget = GameObject.Find(imageTargetName);
        var trackable = imageTarget.GetComponent<TrackableBehaviour>();
        var status = trackable.CurrentStatus;
        return status == TrackableBehaviour.Status.TRACKED;
    }

}

using UnityEngine;
using Virtual_Piano;


/// <summary>
///     - Builds Piano on location of an Aruco Marker
/// </summary>
[RequireComponent(typeof(PianoBuilder))]
public class PianoBuilderArucoMarkerHook : MonoBehaviour
{
    private GameObject leftMarker, rightMarker;

    private PianoBuilder PianoBuilder;

    private void Start()
    {
        leftMarker = GameObject.Find("CubeLeft");
        rightMarker = GameObject.Find("CubeRight");
        PianoBuilder = GetComponent<PianoBuilder>();
        PianoBuilder.BuildPianoAsChildOfTransform(leftMarker.transform);
    }
}
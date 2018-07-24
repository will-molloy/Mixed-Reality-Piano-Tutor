using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PianoBuilder))]
public class PianoBuilderArucoMarkerHook : MonoBehaviour
{

    private PianoBuilder PianoBuilder;

    private GameObject leftMarker, rightMarker;

    void Start()
    {
        leftMarker = GameObject.Find("CubeLeft");
        rightMarker = GameObject.Find("CubeRight");
        PianoBuilder = GetComponent<PianoBuilder>();
        PianoBuilder.BuildPianoAsChildOfTransform(leftMarker.transform);
    }

}

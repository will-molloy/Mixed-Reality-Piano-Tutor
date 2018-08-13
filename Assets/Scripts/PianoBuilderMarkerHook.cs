using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PianoBuilder))]
public class PianoBuilderMarkerHook : MonoBehaviour
{
    private PianoBuilder PianoBuilder;

    [SerializeField] private GameObject LeftMarker;

    [SerializeField] private GameObject RightMarker;

    [SerializeField] private bool twoMarkers;

    private GameObject marker;

    public Transform GetMarkerTransform()
    {
        return marker.transform;
    }

    void Start()
    {
        marker = new GameObject("Marker");

        if (twoMarkers)
        {
            updatePosition();
        }
        else
        {
            marker.transform.SetParent(LeftMarker.transform);
        }
        PianoBuilder = GetComponent<PianoBuilder>();
        PianoBuilder.BuildPianoAsChildOfTransform(marker.transform);
    }

    private void updatePosition()
    {
        var leftPos = LeftMarker.transform.position;
        var rightPos = RightMarker.transform.position;
        var markerPos = marker.transform.position;
        marker.transform.position = new Vector3((rightPos.x + leftPos.x) / 2, markerPos.y, (rightPos.z + leftPos.z) / 2);
    }

    void Update()
    {
        if (twoMarkers)
        {
            updatePosition();
        }
        var scale = marker.transform.localScale;
        var position = marker.transform.position;

        if (Input.GetKey(KeyCode.Plus))
        {
            scale *= 1.001f;
        }
        if (Input.GetKey(KeyCode.Minus))
        {
            scale /= 1.001f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            position -= marker.transform.right * 0.001f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            position += marker.transform.right * 0.001f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            position += marker.transform.forward * 0.001f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            position -= marker.transform.forward * 0.001f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            position += marker.transform.up * 0.001f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            position -= marker.transform.up * 0.001f;
        }
        marker.transform.localScale = scale;
        marker.transform.position = position;

        // Rotation enabled even if two marker
        if (Input.GetKey(KeyCode.UpArrow))
        {
            marker.transform.Rotate(Vector3.right * 0.1f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            marker.transform.Rotate(Vector3.left * 0.1f);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            marker.transform.Rotate(Vector3.down * 0.1f);
        }
        if (Input.GetKey(KeyCode.X))
        {
            marker.transform.Rotate(Vector3.up * 0.1f);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            marker.transform.Rotate(Vector3.forward * 0.1f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            marker.transform.Rotate(Vector3.back * 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            reset();
        }
    }

    private void reset()
    {
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localEulerAngles = Vector3.zero;
        marker.transform.localScale = Vector3.one;
    }
}

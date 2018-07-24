using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PianoBuilder))]
public class PianoBuilderOculusControllerHook : MonoBehaviour
{
    private PianoBuilder PianoBuilder;

    [SerializeField]
    private GameObject Marker;

    private Vector3 savedPos;

    private Quaternion savedRot;

    private Vector3 initialScale;

    public Transform GetMarkerTransform()
    {
        return Marker.transform;
    }

    void Start()
    {
        PianoBuilder = GetComponent<PianoBuilder>();
        Marker.GetComponent<MeshRenderer>().enabled = false;
        PianoBuilder.BuildPianoAsChildOfTransform(Marker.transform);
        PianoBuilder.transform.localPosition += new Vector3(-0.5f, 0.5f, 0);
        initialScale = Marker.transform.localScale;
    }

    void Update()
    {
        var scale = Marker.transform.localScale;
        if (Input.GetKey(KeyCode.Plus))
        {
            scale *= 1.001f;
        }
        if (Input.GetKey(KeyCode.Minus))
        {
            scale /= 1.001f;
        }
        if (Input.GetKey(KeyCode.R))
        {
            Marker.transform.localScale = initialScale;
        }
        Marker.transform.localScale = scale;
        var position = Marker.transform.position;
        var angle = Marker.transform.localEulerAngles;
        var forward = Marker.transform.forward;
        var up = Marker.transform.up;
        var right = Marker.transform.right;
        if (Input.GetKey(KeyCode.A))
        {
            position -= Marker.transform.right * 0.001f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            position += Marker.transform.right * 0.001f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            position += Marker.transform.forward * 0.001f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            position -= Marker.transform.forward * 0.001f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            position += Marker.transform.up * 0.001f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            position -= Marker.transform.up * 0.001f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            angle += new Vector3(0f, 0.1f, 0f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            angle -= new Vector3(0f, 0.1f, 0f);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            angle += new Vector3(0f, 0f, 0.1f);
        }
        if (Input.GetKey(KeyCode.X))
        {
            angle -= new Vector3(0f, 0f, 0.1f);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            angle += new Vector3(0.1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            angle -= new Vector3(0.1f, 0f, 0f);
        }
        Marker.transform.position = position;
        Marker.transform.localEulerAngles = angle;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PianoBuilder))]
public class PianoBuilderOculusControllerHook : MonoBehaviour
{
    private PianoBuilder PianoBuilder;

    [SerializeField]
    private GameObject Marker;

    public Transform transform;

    public Transform GetMarkerTransform()
    {
        return Marker.transform;
    }

    void Start()
    {
        PianoBuilder = GetComponent<PianoBuilder>();
        PianoBuilder.BuildPianoAsChildOfTransform(Marker.transform);
    }

    void Update()
    {
        var scale = Marker.transform.localScale;
        var position = Marker.transform.position;

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
            Marker.transform.Rotate(Vector3.right * 0.1f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Marker.transform.Rotate(Vector3.left * 0.1f);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            Marker.transform.Rotate(Vector3.down * 0.1f);
        }
        if (Input.GetKey(KeyCode.X))
        {
            Marker.transform.Rotate(Vector3.up * 0.1f);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Marker.transform.Rotate(Vector3.forward * 0.1f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Marker.transform.Rotate(Vector3.back * 0.1f);
        }
        Marker.transform.localScale = scale;
        Marker.transform.position = position;
        if (Input.GetKeyDown(KeyCode.R))
        {
            reset();
        }
    }

    private void reset()
    {
        Marker.transform.localPosition = Vector3.zero;
        Marker.transform.localEulerAngles = Vector3.zero;
        Marker.transform.localScale = Vector3.one;
    }
}

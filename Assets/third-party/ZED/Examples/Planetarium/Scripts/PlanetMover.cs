using UnityEngine;

public class PlanetMover : MonoBehaviour
{
    public Vector3 axis = Vector3.up;

    public Transform center;
    private Vector3 dir;

    private Vector3 originPos;
    public float speed = 10.0f;
    public float speedRevolution = 10;

    private void OnEnable()
    {
        //originPos = transform.localPosition;
    }

    private void Start()
    {
        dir = center.up;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.RotateAround(center.position, center.TransformDirection(dir), Time.deltaTime * speedRevolution);

        transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
    }
}
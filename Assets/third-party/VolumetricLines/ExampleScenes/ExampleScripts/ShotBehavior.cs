using UnityEngine;

public class ShotBehavior : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position += transform.forward * Time.deltaTime * 1000f;
    }
}
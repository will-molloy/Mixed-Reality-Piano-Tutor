using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class PianoBuilderV2 : MonoBehaviour
{

    [SerializeField]
    private GameObject debugLineObj;

    private GameObject debugLine;

    private GameObject leftMarker;

    private GameObject rightMarker;

    void Start()
    {
        leftMarker = GameObject.Find("Cone (Left/Blue)");
        rightMarker = GameObject.Find("Cone (Right/Red)");
    }

    void Update()
    {
        if (isTrackingMarkers())
        {
            // Debug.Log(rightMarker.transform.position.x + ", " + rightMarker.transform.position.y + ", " + rightMarker.transform.position.z);
            if (!debugLine)
            {
                Debug.Log("Instantiating");
                debugLine = Instantiate(debugLineObj);
            }
            var distApart = leftMarker.transform.position.x - rightMarker.transform.position.x;
            Debug.Log(distApart);

        }
        else 
        {
            if (debugLine) // wouldn't destroy piano
            {
                Debug.Log("Destryoing");
                Destroy(debugLine);
            }
        }
    }

    private bool isTrackingMarkers()
    {
        return isTrackingMarker("Left ImageTarget") && isTrackingMarker("Right ImageTarget");
    }

    private bool isTrackingMarker(string imageTargetName)
    {
        var imageTarget = GameObject.Find(imageTargetName);
        var trackable = imageTarget.GetComponent<TrackableBehaviour>();
        var status = trackable.CurrentStatus;
        return status == TrackableBehaviour.Status.TRACKED;
    }

}

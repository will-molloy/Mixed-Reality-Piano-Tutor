//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

public struct GazeEventArgs
{
    public float distance;
}

public delegate void GazeEventHandler(object sender, GazeEventArgs e);

public class SteamVR_GazeTracker : MonoBehaviour
{
    public float gazeInCutoff = 0.15f;
    public float gazeOutCutoff = 0.4f;

    // Contains a HMD tracked object that we can use to find the user's gaze
    private Transform hmdTrackedObject;
    public bool isInGaze;
    public event GazeEventHandler GazeOn;
    public event GazeEventHandler GazeOff;

    // Use this for initialization
    private void Start()
    {
    }

    public virtual void OnGazeOn(GazeEventArgs e)
    {
        if (GazeOn != null)
            GazeOn(this, e);
    }

    public virtual void OnGazeOff(GazeEventArgs e)
    {
        if (GazeOff != null)
            GazeOff(this, e);
    }

    // Update is called once per frame
    private void Update()
    {
        // If we haven't set up hmdTrackedObject find what the user is looking at
        if (hmdTrackedObject == null)
        {
            var trackedObjects = FindObjectsOfType<SteamVR_TrackedObject>();
            foreach (var tracked in trackedObjects)
                if (tracked.index == SteamVR_TrackedObject.EIndex.Hmd)
                {
                    hmdTrackedObject = tracked.transform;
                    break;
                }
        }

        if (hmdTrackedObject)
        {
            var r = new Ray(hmdTrackedObject.position, hmdTrackedObject.forward);
            var p = new Plane(hmdTrackedObject.forward, transform.position);

            var enter = 0.0f;
            if (p.Raycast(r, out enter))
            {
                var intersect = hmdTrackedObject.position + hmdTrackedObject.forward * enter;
                var dist = Vector3.Distance(intersect, transform.position);
                //Debug.Log("Gaze dist = " + dist);
                if (dist < gazeInCutoff && !isInGaze)
                {
                    isInGaze = true;
                    GazeEventArgs e;
                    e.distance = dist;
                    OnGazeOn(e);
                }
                else if (dist >= gazeOutCutoff && isInGaze)
                {
                    isInGaze = false;
                    GazeEventArgs e;
                    e.distance = dist;
                    OnGazeOff(e);
                }
            }
        }
    }
}
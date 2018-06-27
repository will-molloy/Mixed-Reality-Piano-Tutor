// SimpleSonarShader scripts and shaders were written by Drew Okenfuss.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSonarShader_Object : MonoBehaviour
{

    // All the renderers that will have the sonar data sent to their shaders.
    private Renderer[] ObjectRenderers;

    // Throwaway values to set position to at the start.
    private static readonly Vector4 GarbagePosition = new Vector4(-5000, -5000, -5000, -5000);

    // The number of rings that can be rendered at once.
    // Must be the samve value as the array size in the shader.
    private static int QueueSize = 20;

    // Queue of start positions of sonar rings.
    // The xyz values hold the xyz of position.
    // The w value holds the time that position was started.
    private static Queue<Vector4> positionsQueue = new Queue<Vector4>(QueueSize);

    // Queue of intensity values for each ring.
    // These are kept in the same order as the positionsQueue.
    private static Queue<float> intensityQueue = new Queue<float>(QueueSize);

    // Make sure only 1 object initializes the queues.
    private static bool NeedToInitQueues = true;

    // Will call the SendSonarData for each object.
    private delegate void Delegate();
    private static Delegate RingDelegate;

    private void Start()
    {
        // Get renderers that will have effect applied to them
        ObjectRenderers = GetComponentsInChildren<Renderer>();

        if(NeedToInitQueues)
        {
            NeedToInitQueues = false;
            // Fill queues with starting values that are garbage values
            for (int i = 0; i < QueueSize; i++)
            {
                positionsQueue.Enqueue(GarbagePosition);
                intensityQueue.Enqueue(-5000f);
            }
        }

        // Add this objects function to the static delegate
        RingDelegate += SendSonarData;
    }

    /// <summary>
    /// Starts a sonar ring from this position with the given intensity.
    /// </summary>
    public void StartSonarRing(Vector4 position, float intensity)
    {
        // Put values into the queue
        position.w = Time.time;
        positionsQueue.Dequeue();
        positionsQueue.Enqueue(position);

        intensityQueue.Dequeue();
        intensityQueue.Enqueue(intensity);

        RingDelegate();
    }

    /// <summary>
    /// Sends the sonar data to the shader.
    /// </summary>
    private void SendSonarData()
    {
        // Send updated queues to the shaders
        foreach (Renderer r in ObjectRenderers)
        {
            r.material.SetVectorArray("_hitPts", positionsQueue.ToArray());
            r.material.SetFloatArray("_Intensity", intensityQueue.ToArray());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Start sonar ring from the contact point
        StartSonarRing(collision.contacts[0].point, collision.impulse.magnitude / 10.0f);
    }

}

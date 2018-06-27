// SimpleSonarShader scripts and shaders were written by Drew Okenfuss.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSonarShader_Parent : MonoBehaviour
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
    private Queue<Vector4> positionsQueue = new Queue<Vector4>(QueueSize);

    // Queue of intensity values for each ring.
    // These are kept in the same order as the positionsQueue.
    private Queue<float> intensityQueue = new Queue<float>(QueueSize);


    private void Start()
    {
        // Get renderers that will have effect applied to them
        ObjectRenderers = GetComponentsInChildren<Renderer>();

        // Fill queues with starting values that are garbage values
        for (int i = 0; i < QueueSize; i++)
        {
            positionsQueue.Enqueue(GarbagePosition);
            intensityQueue.Enqueue(-5000f);
        }

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

        // Send updated queues to the shaders
        foreach (Renderer r in ObjectRenderers)
        {
            r.material.SetVectorArray("_hitPts", positionsQueue.ToArray());
            r.material.SetFloatArray("_Intensity", intensityQueue.ToArray());
        }
    }

}

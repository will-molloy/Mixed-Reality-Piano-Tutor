//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Estimates the velocity of an object based on change in position
//
//=============================================================================

using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class VelocityEstimator : MonoBehaviour
    {
        [Tooltip("How many frames to average over for computing angular velocity")]
        public int angularVelocityAverageFrames = 11;

        private Vector3[] angularVelocitySamples;

        public bool estimateOnAwake;

        private Coroutine routine;
        private int sampleCount;

        [Tooltip("How many frames to average over for computing velocity")]
        public int velocityAverageFrames = 5;

        private Vector3[] velocitySamples;


        //-------------------------------------------------
        public void BeginEstimatingVelocity()
        {
            FinishEstimatingVelocity();

            routine = StartCoroutine(EstimateVelocityCoroutine());
        }


        //-------------------------------------------------
        public void FinishEstimatingVelocity()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }


        //-------------------------------------------------
        public Vector3 GetVelocityEstimate()
        {
            // Compute average velocity
            var velocity = Vector3.zero;
            var velocitySampleCount = Mathf.Min(sampleCount, velocitySamples.Length);
            if (velocitySampleCount != 0)
            {
                for (var i = 0; i < velocitySampleCount; i++) velocity += velocitySamples[i];
                velocity *= 1.0f / velocitySampleCount;
            }

            return velocity;
        }


        //-------------------------------------------------
        public Vector3 GetAngularVelocityEstimate()
        {
            // Compute average angular velocity
            var angularVelocity = Vector3.zero;
            var angularVelocitySampleCount = Mathf.Min(sampleCount, angularVelocitySamples.Length);
            if (angularVelocitySampleCount != 0)
            {
                for (var i = 0; i < angularVelocitySampleCount; i++) angularVelocity += angularVelocitySamples[i];
                angularVelocity *= 1.0f / angularVelocitySampleCount;
            }

            return angularVelocity;
        }


        //-------------------------------------------------
        public Vector3 GetAccelerationEstimate()
        {
            var average = Vector3.zero;
            for (var i = 2 + sampleCount - velocitySamples.Length; i < sampleCount; i++)
            {
                if (i < 2)
                    continue;

                var first = i - 2;
                var second = i - 1;

                var v1 = velocitySamples[first % velocitySamples.Length];
                var v2 = velocitySamples[second % velocitySamples.Length];
                average += v2 - v1;
            }

            average *= 1.0f / Time.deltaTime;
            return average;
        }


        //-------------------------------------------------
        private void Awake()
        {
            velocitySamples = new Vector3[velocityAverageFrames];
            angularVelocitySamples = new Vector3[angularVelocityAverageFrames];

            if (estimateOnAwake) BeginEstimatingVelocity();
        }


        //-------------------------------------------------
        private IEnumerator EstimateVelocityCoroutine()
        {
            sampleCount = 0;

            var previousPosition = transform.position;
            var previousRotation = transform.rotation;
            while (true)
            {
                yield return new WaitForEndOfFrame();

                var velocityFactor = 1.0f / Time.deltaTime;

                var v = sampleCount % velocitySamples.Length;
                var w = sampleCount % angularVelocitySamples.Length;
                sampleCount++;

                // Estimate linear velocity
                velocitySamples[v] = velocityFactor * (transform.position - previousPosition);

                // Estimate angular velocity
                var deltaRotation = transform.rotation * Quaternion.Inverse(previousRotation);

                var theta = 2.0f * Mathf.Acos(Mathf.Clamp(deltaRotation.w, -1.0f, 1.0f));
                if (theta > Mathf.PI) theta -= 2.0f * Mathf.PI;

                var angularVelocity = new Vector3(deltaRotation.x, deltaRotation.y, deltaRotation.z);
                if (angularVelocity.sqrMagnitude > 0.0f)
                    angularVelocity = theta * velocityFactor * angularVelocity.normalized;

                angularVelocitySamples[w] = angularVelocity;

                previousPosition = transform.position;
                previousRotation = transform.rotation;
            }
        }
    }
}
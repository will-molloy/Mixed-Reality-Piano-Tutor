//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Triggers haptic pulses based on distance between 2 positions
//
//=============================================================================

using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class DistanceHaptics : MonoBehaviour
    {
        public AnimationCurve distanceIntensityCurve = AnimationCurve.Linear(0.0f, 800.0f, 1.0f, 800.0f);
        public Transform firstTransform;
        public AnimationCurve pulseIntervalCurve = AnimationCurve.Linear(0.0f, 0.01f, 1.0f, 0.0f);
        public Transform secondTransform;

        //-------------------------------------------------
        private IEnumerator Start()
        {
            while (true)
            {
                var distance = Vector3.Distance(firstTransform.position, secondTransform.position);

                var trackedObject = GetComponentInParent<SteamVR_TrackedObject>();
                if (trackedObject)
                {
                    var pulse = distanceIntensityCurve.Evaluate(distance);
                    SteamVR_Controller.Input((int) trackedObject.index).TriggerHapticPulse((ushort) pulse);
                }

                var nextPulse = pulseIntervalCurve.Evaluate(distance);

                yield return new WaitForSeconds(nextPulse);
            }
        }
    }
}
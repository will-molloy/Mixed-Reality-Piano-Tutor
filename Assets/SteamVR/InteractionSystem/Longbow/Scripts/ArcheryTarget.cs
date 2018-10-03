//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Target that sends events when hit by an arrow
//
//=============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class ArcheryTarget : MonoBehaviour
    {
        private const float targetRadius = 0.25f;

        public Transform baseTransform;
        public Transform fallenDownTransform;
        public float fallTime = 0.5f;

        public bool onceOnly;
        public UnityEvent onTakeDamage;
        public Transform targetCenter;

        private bool targetEnabled = true;


        //-------------------------------------------------
        private void ApplyDamage()
        {
            OnDamageTaken();
        }


        //-------------------------------------------------
        private void FireExposure()
        {
            OnDamageTaken();
        }


        //-------------------------------------------------
        private void OnDamageTaken()
        {
            if (targetEnabled)
            {
                onTakeDamage.Invoke();
                StartCoroutine(FallDown());

                if (onceOnly) targetEnabled = false;
            }
        }


        //-------------------------------------------------
        private IEnumerator FallDown()
        {
            if (baseTransform)
            {
                var startingRot = baseTransform.rotation;

                var startTime = Time.time;
                var rotLerp = 0f;

                while (rotLerp < 1)
                {
                    rotLerp = Util.RemapNumberClamped(Time.time, startTime, startTime + fallTime, 0f, 1f);
                    baseTransform.rotation = Quaternion.Lerp(startingRot, fallenDownTransform.rotation, rotLerp);
                    yield return null;
                }
            }

            yield return null;
        }
    }
}
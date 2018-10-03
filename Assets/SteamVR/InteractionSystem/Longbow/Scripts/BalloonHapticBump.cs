//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Provides a haptic bump when colliding with balloons
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class BalloonHapticBump : MonoBehaviour
    {
        public GameObject physParent;

        //-------------------------------------------------
        private void OnCollisionEnter(Collision other)
        {
            var contactBalloon = other.collider.GetComponentInParent<Balloon>();
            if (contactBalloon != null)
            {
                var hand = physParent.GetComponentInParent<Hand>();
                if (hand != null) hand.controller.TriggerHapticPulse(500);
            }
        }
    }
}
//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object's rigidbody goes to sleep when created
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class SleepOnAwake : MonoBehaviour
    {
        //-------------------------------------------------
        private void Awake()
        {
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody) rigidbody.Sleep();
        }
    }
}
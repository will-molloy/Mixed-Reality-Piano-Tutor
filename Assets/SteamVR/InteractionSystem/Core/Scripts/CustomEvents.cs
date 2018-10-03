//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Custom Unity Events that take in additional parameters
//
//=============================================================================

using System;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public static class CustomEvents
    {
        //-------------------------------------------------
        [Serializable]
        public class UnityEventSingleFloat : UnityEvent<float>
        {
        }


        //-------------------------------------------------
        [Serializable]
        public class UnityEventHand : UnityEvent<Hand>
        {
        }
    }
}
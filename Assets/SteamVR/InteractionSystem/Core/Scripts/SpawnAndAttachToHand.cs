//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Creates an object and attaches it to the hand
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class SpawnAndAttachToHand : MonoBehaviour
    {
        public Hand hand;
        public GameObject prefab;


        //-------------------------------------------------
        public void SpawnAndAttach(Hand passedInhand)
        {
            var handToUse = passedInhand;
            if (passedInhand == null) handToUse = hand;

            if (handToUse == null) return;

            var prefabObject = Instantiate(prefab);
            handToUse.AttachObject(prefabObject);
        }
    }
}
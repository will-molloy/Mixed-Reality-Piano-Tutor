//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Unparents an object and keeps track of the old parent
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class Unparent : MonoBehaviour
    {
        private Transform oldParent;

        //-------------------------------------------------
        private void Start()
        {
            oldParent = transform.parent;
            transform.parent = null;
            gameObject.name = oldParent.gameObject.name + "." + gameObject.name;
        }


        //-------------------------------------------------
        private void Update()
        {
            if (oldParent == null)
                Destroy(gameObject);
        }


        //-------------------------------------------------
        public Transform GetOldParent()
        {
            return oldParent;
        }
    }
}
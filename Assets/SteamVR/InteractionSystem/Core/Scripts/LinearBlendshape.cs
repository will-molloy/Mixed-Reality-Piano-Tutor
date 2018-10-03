//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Set the blend shape weight based on a linear mapping
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class LinearBlendshape : MonoBehaviour
    {
        private float lastValue;
        public LinearMapping linearMapping;
        public SkinnedMeshRenderer skinnedMesh;


        //-------------------------------------------------
        private void Awake()
        {
            if (skinnedMesh == null) skinnedMesh = GetComponent<SkinnedMeshRenderer>();

            if (linearMapping == null) linearMapping = GetComponent<LinearMapping>();
        }


        //-------------------------------------------------
        private void Update()
        {
            var value = linearMapping.value;

            //No need to set the blend if our value hasn't changed.
            if (value != lastValue)
            {
                var blendValue = Util.RemapNumberClamped(value, 0f, 1f, 1f, 100f);
                skinnedMesh.SetBlendShapeWeight(0, blendValue);
            }

            lastValue = value;
        }
    }
}
//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Can be attached to the controller to collide with the balloons
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class BalloonColliders : MonoBehaviour
    {
        private Vector3[] colliderLocalPositions;
        private Quaternion[] colliderLocalRotations;
        public GameObject[] colliders;

        private Rigidbody rb;

        //-------------------------------------------------
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            colliderLocalPositions = new Vector3[colliders.Length];
            colliderLocalRotations = new Quaternion[colliders.Length];

            for (var i = 0; i < colliders.Length; ++i)
            {
                colliderLocalPositions[i] = colliders[i].transform.localPosition;
                colliderLocalRotations[i] = colliders[i].transform.localRotation;

                colliders[i].name = gameObject.name + "." + colliders[i].name;
            }
        }


        //-------------------------------------------------
        private void OnEnable()
        {
            for (var i = 0; i < colliders.Length; ++i)
            {
                colliders[i].transform.SetParent(transform);

                colliders[i].transform.localPosition = colliderLocalPositions[i];
                colliders[i].transform.localRotation = colliderLocalRotations[i];

                colliders[i].transform.SetParent(null);

                var fixedJoint = colliders[i].AddComponent<FixedJoint>();
                fixedJoint.connectedBody = rb;
                fixedJoint.breakForce = Mathf.Infinity;
                fixedJoint.breakTorque = Mathf.Infinity;
                fixedJoint.enableCollision = false;
                fixedJoint.enablePreprocessing = true;

                colliders[i].SetActive(true);
            }
        }


        //-------------------------------------------------
        private void OnDisable()
        {
            for (var i = 0; i < colliders.Length; ++i)
                if (colliders[i] != null)
                {
                    Destroy(colliders[i].GetComponent<FixedJoint>());

                    colliders[i].SetActive(false);
                }
        }


        //-------------------------------------------------
        private void OnDestroy()
        {
            for (var i = 0; i < colliders.Length; ++i) Destroy(colliders[i]);
        }
    }
}
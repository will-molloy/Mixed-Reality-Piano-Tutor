//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class LinearDrive : MonoBehaviour
    {
        public Transform endPosition;

        private float initialMappingOffset;
        public LinearMapping linearMapping;
        public bool maintainMomemntum = true;
        private float mappingChangeRate;
        private float[] mappingChangeSamples;
        public float momemtumDampenRate = 5.0f;
        private readonly int numMappingChangeSamples = 5;
        private float prevMapping;
        public bool repositionGameObject = true;
        private int sampleCount;
        public Transform startPosition;


        //-------------------------------------------------
        private void Awake()
        {
            mappingChangeSamples = new float[numMappingChangeSamples];
        }


        //-------------------------------------------------
        private void Start()
        {
            if (linearMapping == null) linearMapping = GetComponent<LinearMapping>();

            if (linearMapping == null) linearMapping = gameObject.AddComponent<LinearMapping>();

            initialMappingOffset = linearMapping.value;

            if (repositionGameObject) UpdateLinearMapping(transform);
        }


        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            if (hand.GetStandardInteractionButtonDown())
            {
                hand.HoverLock(GetComponent<Interactable>());

                initialMappingOffset = linearMapping.value - CalculateLinearMapping(hand.transform);
                sampleCount = 0;
                mappingChangeRate = 0.0f;
            }

            if (hand.GetStandardInteractionButtonUp())
            {
                hand.HoverUnlock(GetComponent<Interactable>());

                CalculateMappingChangeRate();
            }

            if (hand.GetStandardInteractionButton()) UpdateLinearMapping(hand.transform);
        }


        //-------------------------------------------------
        private void CalculateMappingChangeRate()
        {
            //Compute the mapping change rate
            mappingChangeRate = 0.0f;
            var mappingSamplesCount = Mathf.Min(sampleCount, mappingChangeSamples.Length);
            if (mappingSamplesCount != 0)
            {
                for (var i = 0; i < mappingSamplesCount; ++i) mappingChangeRate += mappingChangeSamples[i];
                mappingChangeRate /= mappingSamplesCount;
            }
        }


        //-------------------------------------------------
        private void UpdateLinearMapping(Transform tr)
        {
            prevMapping = linearMapping.value;
            linearMapping.value = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(tr));

            mappingChangeSamples[sampleCount % mappingChangeSamples.Length] =
                1.0f / Time.deltaTime * (linearMapping.value - prevMapping);
            sampleCount++;

            if (repositionGameObject)
                transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
        }


        //-------------------------------------------------
        private float CalculateLinearMapping(Transform tr)
        {
            var direction = endPosition.position - startPosition.position;
            var length = direction.magnitude;
            direction.Normalize();

            var displacement = tr.position - startPosition.position;

            return Vector3.Dot(displacement, direction) / length;
        }


        //-------------------------------------------------
        private void Update()
        {
            if (maintainMomemntum && mappingChangeRate != 0.0f)
            {
                //Dampen the mapping change rate and apply it to the mapping
                mappingChangeRate = Mathf.Lerp(mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime);
                linearMapping.value = Mathf.Clamp01(linearMapping.value + mappingChangeRate * Time.deltaTime);

                if (repositionGameObject)
                    transform.position =
                        Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
            }
        }
    }
}
//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object can be set on fire
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class FireSource : MonoBehaviour
    {
        public float burnTime;

        public bool canSpreadFromThisSource = true;

        public ParticleSystem customParticles;
        private GameObject fireObject;
        public GameObject fireParticlePrefab;

        private Hand hand;
        public float ignitionDelay;

        public AudioSource ignitionSound;
        private float ignitionTime;

        public bool isBurning;
        public bool startActive;

        //-------------------------------------------------
        private void Start()
        {
            if (startActive) StartBurning();
        }


        //-------------------------------------------------
        private void Update()
        {
            if (burnTime != 0 && Time.time > ignitionTime + burnTime && isBurning)
            {
                isBurning = false;
                if (customParticles != null)
                    customParticles.Stop();
                else
                    Destroy(fireObject);
            }
        }


        //-------------------------------------------------
        private void OnTriggerEnter(Collider other)
        {
            if (isBurning && canSpreadFromThisSource)
                other.SendMessageUpwards("FireExposure", SendMessageOptions.DontRequireReceiver);
        }


        //-------------------------------------------------
        private void FireExposure()
        {
            if (fireObject == null) Invoke("StartBurning", ignitionDelay);

            if (hand = GetComponentInParent<Hand>()) hand.controller.TriggerHapticPulse(1000);
        }


        //-------------------------------------------------
        private void StartBurning()
        {
            isBurning = true;
            ignitionTime = Time.time;

            // Play the fire ignition sound if there is one
            if (ignitionSound != null) ignitionSound.Play();

            if (customParticles != null)
            {
                customParticles.Play();
            }
            else
            {
                if (fireParticlePrefab != null)
                {
                    fireObject = Instantiate(fireParticlePrefab, transform.position, transform.rotation);
                    fireObject.transform.parent = transform;
                }
            }
        }
    }
}
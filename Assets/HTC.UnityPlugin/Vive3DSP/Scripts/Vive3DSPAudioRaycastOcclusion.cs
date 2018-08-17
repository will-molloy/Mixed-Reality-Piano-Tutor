//====================== Copyright 2016-2018, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using System;

namespace HTC.UnityPlugin.Vive3DSP
{
    [AddComponentMenu("VIVE/3DSP_AudioOcclusion/Raycast")]

    public class Vive3DSPAudioRaycastOcclusion : MonoBehaviour
    {
        public Vive3DSPAudio.OccEngineMode occlusionEngine;

        public int RayNumber
        {
            get { return rayNumber; }
        }
        [SerializeField]
        private int rayNumber = 12;
        public bool OcclusionEffect
        {
            set { occlusionEffect = value; }
            get { return occlusionEffect; }
        }
        [SerializeField]
        private bool occlusionEffect = true;

        public Vive3DSPAudio.OccMaterial OcclusionMaterial
        {
            set { occlusionMaterial = value; }
            get { return occlusionMaterial; }
        }
        [SerializeField]
        private Vive3DSPAudio.OccMaterial occlusionMaterial = Vive3DSPAudio.OccMaterial.Curtain;

        public float OcclusionIntensity
        {
            set { occlusionIntensity = value; }
            get { return occlusionIntensity; }
        }
        [SerializeField]
        private float occlusionIntensity = 1.5f;

        public float HighFreqAttenuation
        {
            set { highFreqAttenuation = value; }
            get { return highFreqAttenuation; }
        }
        [SerializeField]
        private float highFreqAttenuation = -50.0f;

        public float LowFreqAttenuationRatio
        {
            set { lowFreqAttenuationRatio = value; }
            get { return lowFreqAttenuationRatio; }
        }
        [SerializeField]
        private float lowFreqAttenuationRatio = 0.0f;

        // Check!
        public Vector3 Position
        {
            set { if (pos != value) pos = value; }
            get { return pos; }
        }
        private Vector3 pos = Vector3.zero;

        public Vive3DSPAudio.VIVE_3DSP_OCCLUSION_PROPERTY OcclusionPorperty
        {
            get { return occProperty; }
        }
        private Vive3DSPAudio.VIVE_3DSP_OCCLUSION_PROPERTY occProperty;

        public IntPtr OcclusionObject
        {
            get { return _occObj; }
            set { _occObj = value; }
        }
        private IntPtr _occObj = IntPtr.Zero;

        void Awake()
        {
            InitOcclusion();
            occlusionEngine = (Vive3DSPAudio.OccEngineMode)2;
        }

        void OnEnable()
        {
            if (InitOcclusion())
            {
                Vive3DSPAudio.EnableOcclusion(_occObj);
            }
            Vive3DSPAudio.UpdateAudioListener();
            Update();
            occProperty.position = transform.position;
            occProperty.radius = 1.0f;
        }

        void OnDisable()
        {
            Vive3DSPAudio.DisableOcclusion(_occObj);
        }


        void Update()
        {
            Position = transform.position;

            occProperty.density = occlusionIntensity;
            occProperty.material = occlusionMaterial;
            occProperty.position = transform.position;
            occProperty.rhf = highFreqAttenuation;
            occProperty.lfratio = lowFreqAttenuationRatio;
            occProperty.mode = occlusionEngine;
            Vive3DSPAudio.UpdateOcclusion(_occObj, occlusionEffect, OcclusionPorperty);
        }

        private void OnDestroy()
        {
            Vive3DSPAudio.DestroyRaycastOcclusion(this);
        }

        private bool InitOcclusion()
        {
            if (_occObj == IntPtr.Zero)
            {
                _occObj = Vive3DSPAudio.CreateRaycastOcclusion(this);
            }
            return _occObj != IntPtr.Zero;
        }
    }
}


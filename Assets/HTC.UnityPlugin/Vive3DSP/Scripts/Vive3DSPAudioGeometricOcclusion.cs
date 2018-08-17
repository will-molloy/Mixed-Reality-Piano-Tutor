//====================== Copyright 2016-2018, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using System;

namespace HTC.UnityPlugin.Vive3DSP
{
    [AddComponentMenu("VIVE/3DSP_AudioOcclusion/Geometric")]

    public class Vive3DSPAudioGeometricOcclusion : MonoBehaviour
    {
        public Vive3DSPAudio.OccEngineMode occlusionEngine = Vive3DSPAudio.OccEngineMode.Sphere;

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

        public Vector3 Position
        {
            get { return pos; }
            set { if (pos != value) pos = value; }
        }
        private Vector3 pos = Vector3.zero;

        public Vive3DSPAudio.VIVE_3DSP_OCCLUSION_PROPERTY OcclusionPorperty
        {
            set { occProperty = value; }
            get { return occProperty; }
        }
        private Vive3DSPAudio.VIVE_3DSP_OCCLUSION_PROPERTY occProperty;

        public IntPtr OcclusionObject
        {
            get { return _occObj; }
            set { _occObj = value; }
        }
        private IntPtr _occObj = IntPtr.Zero;

        public Vector3 OcclusionCenter
        {
            set { occlusionCenter = value; }
            get { return occlusionCenter; }
        }
        [SerializeField]
        private Vector3 occlusionCenter = Vector3.zero;

        public float OcclusionRadius
        {
            get { return occlusionRadius; }
            set { occlusionRadius = value; }
        }
        [SerializeField]
        private float occlusionRadius = 1.0f;

        public Vector3 OcclusionSize
        {
            set { occlusionSize = value; }
            get { return occlusionSize; }
        }
        [SerializeField]
        private Vector3 occlusionSize = Vector3.one;
        
        void Awake()
        {
            InitOcclusion();
        }
        
        void OnEnable()
        {
            if (InitOcclusion())
            {
                Vive3DSPAudio.EnableOcclusion(_occObj);
            }
            Vive3DSPAudio.UpdateAudioListener();
            Update();
        }

        void OnDisable()
        {
            Vive3DSPAudio.DisableOcclusion(_occObj);
        }

        void Update()
        {
            if (occlusionEngine == Vive3DSPAudio.OccEngineMode.Sphere)
            {
                var radius = (Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * occlusionRadius) / 2;
                occProperty.radius = radius;
            }
            else
            {
                occProperty.radius = 0f;
            }
            occProperty.density = occlusionIntensity;
            occProperty.material = occlusionMaterial;
            occProperty.position = (transform.position + transform.rotation * Vector3.Scale(occlusionCenter, transform.lossyScale));
            occProperty.size = Vector3.Scale(transform.lossyScale, OcclusionSize);
            occProperty.rotation = transform.rotation;
            occProperty.rhf = highFreqAttenuation;
            occProperty.lfratio = lowFreqAttenuationRatio;
            occProperty.mode = occlusionEngine;
            Vive3DSPAudio.UpdateOcclusion(_occObj, occlusionEffect, OcclusionPorperty);
        }

        private void OnDestroy()
        {
            Vive3DSPAudio.DestroyGeometricOcclusion(this);
        }
        
        private bool InitOcclusion()
        {
            if (_occObj == IntPtr.Zero)
            {
                _occObj = Vive3DSPAudio.CreateGeometricOcclusion(this);
            }
            return _occObj != IntPtr.Zero;
        }

        void OnDrawGizmosSelected()
        {
            if (occlusionEngine == Vive3DSPAudio.OccEngineMode.Sphere)
            {
                Gizmos.color = Color.black;
                var posUpdate = transform.position + transform.rotation * Vector3.Scale(occlusionCenter, transform.lossyScale);
                float maxScaleVal = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                Vector3 scaleVec = new Vector3(maxScaleVal, maxScaleVal, maxScaleVal);
                Gizmos.matrix = Matrix4x4.TRS(posUpdate, transform.rotation, scaleVec);
                Gizmos.DrawWireSphere(Vector3.zero, occlusionRadius / 2);
            }
            else
            {
                Gizmos.color = Color.black;
                var posUpdate = transform.position + transform.rotation * Vector3.Scale(occlusionCenter, transform.lossyScale);
                Gizmos.matrix = Matrix4x4.TRS(posUpdate, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(Vector3.zero, occlusionSize);
            }
        }
    }
}


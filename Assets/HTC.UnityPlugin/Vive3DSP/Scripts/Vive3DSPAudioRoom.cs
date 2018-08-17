//====================== Copyright 2016-2018, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using System;
using System.IO;

namespace HTC.UnityPlugin.Vive3DSP
{
    [AddComponentMenu("VIVE/3DSP_AudioRoom")]

    public class Vive3DSPAudioRoom : MonoBehaviour
    {
        public bool RoomEffect
        {
            set { roomEffect = value; }
            get { return roomEffect; }
        }
        [SerializeField]
        private bool roomEffect = true;
        public Vive3DSPAudio.RoomReverbPreset ReverbPreset
        {
            set { reverbPreset = value; }
            get { return reverbPreset; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomReverbPreset reverbPreset = Vive3DSPAudio.RoomReverbPreset.Generic;
        public Vive3DSPAudio.RoomBackgroundAudioType BackgroundType
        {
            set
            {
                backgroundType = value;
                backgroundClip = GetBackgroundAudioClip();
            }
            get { return backgroundType; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomBackgroundAudioType backgroundType = Vive3DSPAudio.RoomBackgroundAudioType.None;
        public IntPtr RoomObject
        {
            get { return roomObject; }
            set { roomObject = value; }
        }
        private IntPtr roomObject = IntPtr.Zero;
        public float backgroundVolume
        {
            get { return sourceVolume; }
            set
            {
                sourceVolume = value;
                if (audioSource != null)
                {
                    audioSource.volume = (float)Math.Pow(10.0, sourceVolume * 0.05);
                }
            }
        }
        [SerializeField]
        private float sourceVolume = -30.0f;
        public Vector3 Size
        {
            set { size = value; }
            get { return size; }
        }
        [SerializeField]
        private Vector3 size = Vector3.one;

        public Vive3DSPAudio.RoomPlateMaterial Ceiling
        {
            set { ceiling = value; }
            get { return ceiling; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial ceiling = Vive3DSPAudio.RoomPlateMaterial.Concrete;
        public Vive3DSPAudio.RoomPlateMaterial FrontWall
        {
            set { frontWall = value; }
            get { return frontWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial frontWall = Vive3DSPAudio.RoomPlateMaterial.Wood;
        public Vive3DSPAudio.RoomPlateMaterial BackWall
        {
            set { backWall = value; }
            get { return backWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial backWall = Vive3DSPAudio.RoomPlateMaterial.Wood;
        public Vive3DSPAudio.RoomPlateMaterial RightWall
        {
            set { rightWall = value; }
            get { return rightWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial rightWall = Vive3DSPAudio.RoomPlateMaterial.Carpet;
        public Vive3DSPAudio.RoomPlateMaterial LeftWall
        {
            set { leftWall = value; }
            get { return leftWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial leftWall = Vive3DSPAudio.RoomPlateMaterial.Wood;
        public Vive3DSPAudio.RoomPlateMaterial Floor
        {
            set { floor = value; }
            get { return floor; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial floor = Vive3DSPAudio.RoomPlateMaterial.Concrete;
        public float CeilingReflectionRate
        {
            set { ceilingReflectionRate = value; }
            get { return ceilingReflectionRate; }
        }
        [SerializeField]
        private float ceilingReflectionRate = 1.0f;
        public float FrontWallReflectionRate
        {
            set { frontWallReflectionRate = value; }
            get { return frontWallReflectionRate; }
        }
        [SerializeField]
        private float frontWallReflectionRate = 1.0f;
        public float BackWallReflectionRate
        {
            set { backWallReflectionRate = value; }
            get { return backWallReflectionRate; }
        }
        [SerializeField]
        private float backWallReflectionRate = 1.0f;
        public float RightWallReflectionRate
        {
            set { rightWallReflectionRate = value; }
            get { return rightWallReflectionRate; }
        }
        [SerializeField]
        private float rightWallReflectionRate = 1.0f;
        public float LeftWallReflectionRate
        {
            set { leftWallReflectionRate = value; }
            get { return leftWallReflectionRate; }
        }
        [SerializeField]
        private float leftWallReflectionRate = 1.0f;
        public float FloorReflectionRate
        {
            set { floorReflectionRate = value; }
            get { return floorReflectionRate; }
        }
        [SerializeField]
        private float floorReflectionRate = 1.0f;
        public float ReflectionLevel
        {
            set { reflectionLevel = value; }
            get { return reflectionLevel; }
        }
        [SerializeField]
        public float reflectionLevel = 0.0f;
        public float ReverbLevel
        {
            set { reverbLevel = value; }
            get { return reverbLevel; }
        }
        [SerializeField]
        public float reverbLevel = 0.0f;

        [SerializeField]
        private AudioClip userDefineClip = null;
        
        private AudioSource audioSource = null;
        private AudioClip sourceClip = null;

        public bool isCurrentRoom = false;

        public Vive3DSPAudio.VIVE_3DSP_ROOM_PROPERTY RoomPorperty
        {
            get { return roomProperty; }
        }
        private Vive3DSPAudio.VIVE_3DSP_ROOM_PROPERTY roomProperty;

        public AudioClip backgroundClip
        {
            get { return sourceClip; }
            set {
                if (sourceClip != value)
                {
                    sourceClip = value;
                    if (audioSource != null)
                    {
                        audioSource.clip = sourceClip;
                        PlayBackgroundAudio();
                    }
                }
            }
        }
        
        public bool isPlaying {
            get {
                if (audioSource != null) { return audioSource.isPlaying; }
                return false;
            }
        }
        
        void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.hideFlags = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
            }
            audioSource.spatialize = false;
            audioSource.playOnAwake = true;
            audioSource.loop = true;
            audioSource.dopplerLevel = 0.0f;
            audioSource.spatialBlend = 0.0f;
        }
        
        void Start()
        {
            InitRoom();
            backgroundClip = GetBackgroundAudioClip();
            backgroundVolume = sourceVolume;
        }

        private void OnEnable()
        {
            RoomEffect = true;
            Update();
        }

        private void OnDisable()
        {
            RoomEffect = false;
            UpdateRoomProperty();
        }

        void Update()
        {
            Vive3DSPAudio.CheckIfListenerInRoom(this);
            if (backgroundType == Vive3DSPAudio.RoomBackgroundAudioType.UserDefine)
            {
                backgroundClip = GetBackgroundAudioClip();
            }
            UpdateRoomProperty();
        }

        void OnDestroy()
        {
            Vive3DSPAudio.DestroyRoom(this);
            roomObject = IntPtr.Zero;
            Destroy(audioSource);
        }

        void UpdateRoomProperty()
        {
            roomProperty.position = transform.position;
            roomProperty.rotation = transform.rotation;
            roomProperty.preset = reverbPreset;
            roomProperty.reflection_level = reflectionLevel;
            roomProperty.reverb_level = reverbLevel;
            roomProperty.dry_level = 1.0f;
            roomProperty.gain = 1.0f;
            roomProperty.reflection_rate_left = leftWallReflectionRate;
            roomProperty.reflection_rate_right = rightWallReflectionRate;
            roomProperty.reflection_rate_back = backWallReflectionRate;
            roomProperty.reflection_rate_front = frontWallReflectionRate;
            roomProperty.reflection_rate_ceiling = ceilingReflectionRate;
            roomProperty.reflection_rate_floor = floorReflectionRate;
            roomProperty.material_left = leftWall;
            roomProperty.material_right = rightWall;
            roomProperty.material_back = backWall;
            roomProperty.material_front = frontWall;
            roomProperty.material_ceiling = ceiling;
            roomProperty.material_floor = floor;
            roomProperty.size = Vector3.Scale(transform.lossyScale, size);
            if (this != null && roomObject != IntPtr.Zero)
                Vive3DSPAudio.UpdateRoom(this);

            if (roomEffect)
                PlayBackgroundAudio();
            else
                StopBackgroundAudio();
        }

        private void InitRoom()
        {
            Vive3DSPAudio.CreateRoom(this);
        }

        private AudioClip GetBackgroundAudioClip()
        {
            AudioClip tempClip;
            switch (backgroundType)
            {
                case Vive3DSPAudio.RoomBackgroundAudioType.UserDefine:
                    tempClip = userDefineClip;
                    break;
                default:
                    float[] data = Vive3DSPAudio.GetBGAudioData((int)backgroundType);
                    if (data == null)
                        tempClip = null;
                    else
                    {
                        tempClip = AudioClip.Create("BG Preset", 48000, 1, 48000, false);
                        tempClip.SetData(data, 0);
                    }
                    
                    break;
            }

            return tempClip;
        }

        public void PlayBackgroundAudio()
        {
            if ((audioSource != null) && (!isPlaying) && (backgroundType != Vive3DSPAudio.RoomBackgroundAudioType.None) && (isCurrentRoom == true) && roomEffect)
            {
                audioSource.Play();
            }
        }

        public void StopBackgroundAudio()
        {
            if ((audioSource != null) && (isPlaying)) {
                audioSource.Stop();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}

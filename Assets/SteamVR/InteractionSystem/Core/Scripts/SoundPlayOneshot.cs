//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Play one-shot sounds as opposed to continuos/looping ones
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class SoundPlayOneshot : MonoBehaviour
    {
        public float pitchMax;

        public float pitchMin;

        public bool playOnAwake;
        private AudioSource thisAudioSource;
        public float volMax;

        public float volMin;
        public AudioClip[] waveFiles;


        //-------------------------------------------------
        private void Awake()
        {
            thisAudioSource = GetComponent<AudioSource>();

            if (playOnAwake) Play();
        }


        //-------------------------------------------------
        public void Play()
        {
            if (thisAudioSource != null && thisAudioSource.isActiveAndEnabled && !Util.IsNullOrEmpty(waveFiles))
            {
                //randomly apply a volume between the volume min max
                thisAudioSource.volume = Random.Range(volMin, volMax);

                //randomly apply a pitch between the pitch min max
                thisAudioSource.pitch = Random.Range(pitchMin, pitchMax);

                // play the sound
                thisAudioSource.PlayOneShot(waveFiles[Random.Range(0, waveFiles.Length)]);
            }
        }


        //-------------------------------------------------
        public void Pause()
        {
            if (thisAudioSource != null) thisAudioSource.Pause();
        }


        //-------------------------------------------------
        public void UnPause()
        {
            if (thisAudioSource != null) thisAudioSource.UnPause();
        }
    }
}
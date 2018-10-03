//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Plays one of many audio files with possible randomized parameters
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(AudioSource))]
    public class PlaySound : MonoBehaviour
    {
        private AudioSource audioSource;
        private AudioClip clip;

        [Header("Delay Time")] [Tooltip("Time to offset playback of sound")]
        public float delayOffsetTime;

        [Tooltip("After the audio clip finishes playing, disable the game object the sound is on.")]
        public bool disableOnEnd;

        [Tooltip("Loop the sound after the wave file variation has been chosen.")]
        public bool looping;

        [Tooltip("Percent chance that the wave file will not play")] [Range(0.0f, 1.0f)]
        public float percentToNotPlay;

        [Tooltip("Maximum pitch that will be used when randomly set.")] [Range(-3.0f, 3.0f)]
        public float pitchMax = 1.0f;

        [Tooltip("Minimum pitch that will be used when randomly set.")] [Range(-3.0f, 3.0f)]
        public float pitchMin = 1.0f;

        [Tooltip("Start a wave file playing on awake, but after a delay.")]
        public bool playOnAwakeWithDelay;

        [Tooltip(
            "If the sound is looping and updating it's position every frame, stop the sound at the end of the wav/clip length. ")]
        public bool stopOnEnd;

        [Tooltip("Stops the currently playing clip in the audioSource. Otherwise clips will overlap/mix.")]
        public bool stopOnPlay;

        [Tooltip("Inital time before the first repetion starts")] [Range(0.0f, 360.0f)]
        public float timeInitial;

        [Tooltip("Maximum pitch that will be used when randomly set.")] [Range(0.0f, 360.0f)]
        public float timeMax;

        [Tooltip("Minimum time that will pass before the sound is retriggered")] [Range(0.0f, 360.0f)]
        public float timeMin;

        [Header("Random Pitch")] [Tooltip("Use min and max random pitch levels when playing sounds.")]
        public bool useRandomPitch = true;

        [Header("Random Silence")] [Tooltip("Use Retrigger Time to repeat the sound within a time range")]
        public bool useRandomSilence;

        [Header("Random Volume")] public bool useRandomVolume = true;

        [Header("Random Time")] [Tooltip("Use Retrigger Time to repeat the sound within a time range")]
        public bool useRetriggerTime;

        [Tooltip("Maximum volume that will be used when randomly set.")] [Range(0.0f, 1.0f)]
        public float volMax = 1.0f;

        [Tooltip("Minimum volume that will be used when randomly set.")] [Range(0.0f, 1.0f)]
        public float volMin = 1.0f;

        [Tooltip("List of audio clips to play.")]
        public AudioClip[] waveFile;

        //-------------------------------------------------
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            clip = audioSource.clip;

            // audio source play on awake is true, just play the PlaySound immediately
            if (audioSource.playOnAwake)
            {
                if (useRetriggerTime)
                    InvokeRepeating("Play", timeInitial, Random.Range(timeMin, timeMax));
                else
                    Play();
            }

            // if playOnAwake is false, but the playOnAwakeWithDelay on the PlaySound is true, play the sound on away but with a delay
            else if (!audioSource.playOnAwake && playOnAwakeWithDelay)
            {
                PlayWithDelay(delayOffsetTime);

                if (useRetriggerTime)
                    InvokeRepeating("Play", timeInitial, Random.Range(timeMin, timeMax));
            }

            // in the case where both playOnAwake and playOnAwakeWithDelay are both set to true, just to the same as above, play the sound but with a delay
            else if (audioSource.playOnAwake && playOnAwakeWithDelay)
            {
                PlayWithDelay(delayOffsetTime);

                if (useRetriggerTime)
                    InvokeRepeating("Play", timeInitial, Random.Range(timeMin, timeMax));
            }
        }


        //-------------------------------------------------
        // Play a random clip from those available
        //-------------------------------------------------
        public void Play()
        {
            if (looping)
                PlayLooping();

            else PlayOneShotSound();
        }


        //-------------------------------------------------
        public void PlayWithDelay(float delayTime)
        {
            if (looping)
                Invoke("PlayLooping", delayTime);
            else
                Invoke("PlayOneShotSound", delayTime);
        }


        //-------------------------------------------------
        // Play random wave clip on audiosource as a one shot
        //-------------------------------------------------
        public AudioClip PlayOneShotSound()
        {
            if (!audioSource.isActiveAndEnabled)
                return null;

            SetAudioSource();
            if (stopOnPlay)
                audioSource.Stop();
            if (disableOnEnd)
                Invoke("Disable", clip.length);
            audioSource.PlayOneShot(clip);
            return clip;
        }


        //-------------------------------------------------
        public AudioClip PlayLooping()
        {
            // get audio source properties, and do any special randomizations
            SetAudioSource();

            // if the audio source has forgotten to be set to looping, set it to looping
            if (!audioSource.loop)
                audioSource.loop = true;

            // play the clip in the audio source, all the meanwhile updating it's location
            audioSource.Play();

            // if disable on end is checked, stop playing the wave file after the first loop has finished.
            if (stopOnEnd)
                Invoke("Stop", audioSource.clip.length);
            return clip;
        }


        //-------------------------------------------------
        public void Disable()
        {
            gameObject.SetActive(false);
        }


        //-------------------------------------------------
        public void Stop()
        {
            audioSource.Stop();
        }


        //-------------------------------------------------
        private void SetAudioSource()
        {
            if (useRandomVolume)
            {
                //randomly apply a volume between the volume min max
                audioSource.volume = Random.Range(volMin, volMax);

                if (useRandomSilence && Random.Range(0, 1) < percentToNotPlay) audioSource.volume = 0;
            }

            if (useRandomPitch) audioSource.pitch = Random.Range(pitchMin, pitchMax);

            if (waveFile.Length > 0)
            {
                // randomly assign a wave file from the array into the audioSource clip property
                audioSource.clip = waveFile[Random.Range(0, waveFile.Length)];
                clip = audioSource.clip;
            }
        }
    }
}
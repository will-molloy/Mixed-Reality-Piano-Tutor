//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Spawns balloons
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class BalloonSpawner : MonoBehaviour
    {
        public bool attachBalloon;

        public bool autoSpawn = true;
        public GameObject balloonPrefab;

        public Balloon.BalloonColor color = Balloon.BalloonColor.Random;
        public SoundPlayOneshot inflateSound;
        public float maxSpawnTime = 15f;
        public float minSpawnTime = 5f;
        private float nextSpawnTime;

        public bool playSounds = true;

        public float scale = 1f;

        public bool sendSpawnMessageToParent;
        public bool spawnAtStartup = true;

        public Transform spawnDirectionTransform;
        public float spawnForce;
        public SoundPlayOneshot stretchSound;


        //-------------------------------------------------
        private void Start()
        {
            if (balloonPrefab == null) return;

            if (autoSpawn && spawnAtStartup)
            {
                SpawnBalloon(color);
                nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime) + Time.time;
            }
        }


        //-------------------------------------------------
        private void Update()
        {
            if (balloonPrefab == null) return;

            if (Time.time > nextSpawnTime && autoSpawn)
            {
                SpawnBalloon(color);
                nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime) + Time.time;
            }
        }


        //-------------------------------------------------
        public GameObject SpawnBalloon(Balloon.BalloonColor color = Balloon.BalloonColor.Red)
        {
            if (balloonPrefab == null) return null;
            var balloon = Instantiate(balloonPrefab, transform.position, transform.rotation);
            balloon.transform.localScale = new Vector3(scale, scale, scale);
            if (attachBalloon) balloon.transform.parent = transform;

            if (sendSpawnMessageToParent)
                if (transform.parent != null)
                    transform.parent.SendMessage("OnBalloonSpawned", balloon, SendMessageOptions.DontRequireReceiver);

            if (playSounds)
            {
                if (inflateSound != null) inflateSound.Play();
                if (stretchSound != null) stretchSound.Play();
            }

            balloon.GetComponentInChildren<Balloon>().SetColor(color);
            if (spawnDirectionTransform != null)
                balloon.GetComponentInChildren<Rigidbody>().AddForce(spawnDirectionTransform.forward * spawnForce);

            return balloon;
        }


        //-------------------------------------------------
        public void SpawnBalloonFromEvent(int color)
        {
            // Copy of SpawnBalloon using int because we can't pass in enums through the event system
            SpawnBalloon((Balloon.BalloonColor) color);
        }
    }
}
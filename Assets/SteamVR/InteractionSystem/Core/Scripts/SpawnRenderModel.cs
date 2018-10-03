//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Spawns a render model for the controller from SteamVR
//
//=============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class SpawnRenderModel : MonoBehaviour
    {
        private static readonly List<SpawnRenderModel> spawnRenderModels = new List<SpawnRenderModel>();
        private static int lastFrameUpdated;
        private static int spawnRenderModelUpdateIndex;
        private Hand hand;
        public Material[] materials;
        private readonly List<MeshRenderer> renderers = new List<MeshRenderer>();

        private SteamVR_Events.Action renderModelLoadedAction;

        private SteamVR_RenderModel[] renderModels;


        //-------------------------------------------------
        private void Awake()
        {
            renderModels = new SteamVR_RenderModel[materials.Length];
            renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(OnRenderModelLoaded);
        }


        //-------------------------------------------------
        private void OnEnable()
        {
            ShowController();

            renderModelLoadedAction.enabled = true;

            spawnRenderModels.Add(this);
        }


        //-------------------------------------------------
        private void OnDisable()
        {
            HideController();

            renderModelLoadedAction.enabled = false;

            spawnRenderModels.Remove(this);
        }


        //-------------------------------------------------
        private void OnAttachedToHand(Hand hand)
        {
            this.hand = hand;
            ShowController();
        }


        //-------------------------------------------------
        private void OnDetachedFromHand(Hand hand)
        {
            this.hand = null;
            HideController();
        }


        //-------------------------------------------------
        private void Update()
        {
            // Only update one per frame
            if (lastFrameUpdated == Time.renderedFrameCount) return;
            lastFrameUpdated = Time.renderedFrameCount;


            // SpawnRenderModel overflow
            if (spawnRenderModelUpdateIndex >= spawnRenderModels.Count) spawnRenderModelUpdateIndex = 0;


            // Perform update
            if (spawnRenderModelUpdateIndex < spawnRenderModels.Count)
            {
                var renderModel = spawnRenderModels[spawnRenderModelUpdateIndex].renderModels[0];
                if (renderModel != null) renderModel.UpdateComponents(OpenVR.RenderModels);
            }

            spawnRenderModelUpdateIndex++;
        }


        //-------------------------------------------------
        private void ShowController()
        {
            if (hand == null || hand.controller == null) return;

            for (var i = 0; i < renderModels.Length; i++)
            {
                if (renderModels[i] == null)
                {
                    renderModels[i] = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
                    renderModels[i].updateDynamically = false; // Update one per frame (see Update() method)
                    renderModels[i].transform.parent = transform;
                    Util.ResetTransform(renderModels[i].transform);
                }

                renderModels[i].gameObject.SetActive(true);
                renderModels[i].SetDeviceIndex((int) hand.controller.index);
            }
        }


        //-------------------------------------------------
        private void HideController()
        {
            for (var i = 0; i < renderModels.Length; i++)
                if (renderModels[i] != null)
                    renderModels[i].gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private void OnRenderModelLoaded(SteamVR_RenderModel renderModel, bool success)
        {
            for (var i = 0; i < renderModels.Length; i++)
                if (renderModel == renderModels[i])
                    if (materials[i] != null)
                    {
                        renderers.Clear();
                        renderModels[i].GetComponentsInChildren(renderers);
                        for (var j = 0; j < renderers.Count; j++)
                        {
                            var mainTexture = renderers[j].material.mainTexture;
                            renderers[j].sharedMaterial = materials[i];
                            renderers[j].material.mainTexture = mainTexture;
                            renderers[j].gameObject.layer = gameObject.layer;
                            renderers[j].tag = gameObject.tag;
                        }
                    }
        }
    }
}
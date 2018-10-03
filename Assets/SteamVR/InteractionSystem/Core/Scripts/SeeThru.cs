//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Uses the see thru renderer while attached to hand
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class SeeThru : MonoBehaviour
    {
        private Renderer destRenderer;
        private Interactable interactable;

        private GameObject seeThru;
        public Material seeThruMaterial;
        private Renderer sourceRenderer;


        //-------------------------------------------------
        private void Awake()
        {
            interactable = GetComponentInParent<Interactable>();

            //
            // Create child game object for see thru renderer
            //
            seeThru = new GameObject("_see_thru");
            seeThru.transform.parent = transform;
            seeThru.transform.localPosition = Vector3.zero;
            seeThru.transform.localRotation = Quaternion.identity;
            seeThru.transform.localScale = Vector3.one;

            //
            // Copy mesh filter
            //
            var sourceMeshFilter = GetComponent<MeshFilter>();
            if (sourceMeshFilter != null)
            {
                var destMeshFilter = seeThru.AddComponent<MeshFilter>();
                destMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
            }

            //
            // Copy mesh renderer
            //
            var sourceMeshRenderer = GetComponent<MeshRenderer>();
            if (sourceMeshRenderer != null)
            {
                sourceRenderer = sourceMeshRenderer;
                destRenderer = seeThru.AddComponent<MeshRenderer>();
            }

            //
            // Copy skinned mesh renderer
            //
            var sourceSkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (sourceSkinnedMeshRenderer != null)
            {
                var destSkinnedMeshRenderer = seeThru.AddComponent<SkinnedMeshRenderer>();

                sourceRenderer = sourceSkinnedMeshRenderer;
                destRenderer = destSkinnedMeshRenderer;

                destSkinnedMeshRenderer.sharedMesh = sourceSkinnedMeshRenderer.sharedMesh;
                destSkinnedMeshRenderer.rootBone = sourceSkinnedMeshRenderer.rootBone;
                destSkinnedMeshRenderer.bones = sourceSkinnedMeshRenderer.bones;
                destSkinnedMeshRenderer.quality = sourceSkinnedMeshRenderer.quality;
                destSkinnedMeshRenderer.updateWhenOffscreen = sourceSkinnedMeshRenderer.updateWhenOffscreen;
            }

            //
            // Create see thru materials
            //
            if (sourceRenderer != null && destRenderer != null)
            {
                var materialCount = sourceRenderer.sharedMaterials.Length;
                var destRendererMaterials = new Material[materialCount];
                for (var i = 0; i < materialCount; i++) destRendererMaterials[i] = seeThruMaterial;
                destRenderer.sharedMaterials = destRendererMaterials;

                for (var i = 0; i < destRenderer.materials.Length; i++)
                    destRenderer.materials[i].renderQueue = 2001; // Rendered after geometry

                for (var i = 0; i < sourceRenderer.materials.Length; i++)
                    if (sourceRenderer.materials[i].renderQueue == 2000)
                        sourceRenderer.materials[i].renderQueue = 2002;
            }

            seeThru.gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private void OnEnable()
        {
            interactable.onAttachedToHand += AttachedToHand;
            interactable.onDetachedFromHand += DetachedFromHand;
        }


        //-------------------------------------------------
        private void OnDisable()
        {
            interactable.onAttachedToHand -= AttachedToHand;
            interactable.onDetachedFromHand -= DetachedFromHand;
        }


        //-------------------------------------------------
        private void AttachedToHand(Hand hand)
        {
            seeThru.SetActive(true);
        }


        //-------------------------------------------------
        private void DetachedFromHand(Hand hand)
        {
            seeThru.SetActive(false);
        }


        //-------------------------------------------------
        private void Update()
        {
            if (seeThru.activeInHierarchy)
            {
                var materialCount = Mathf.Min(sourceRenderer.materials.Length, destRenderer.materials.Length);
                for (var i = 0; i < materialCount; i++)
                {
                    destRenderer.materials[i].mainTexture = sourceRenderer.materials[i].mainTexture;
                    destRenderer.materials[i].color =
                        destRenderer.materials[i].color * sourceRenderer.materials[i].color;
                }
            }
        }
    }
}
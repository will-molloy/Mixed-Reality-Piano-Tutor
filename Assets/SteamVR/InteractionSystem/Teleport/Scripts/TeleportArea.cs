//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: An area that the player can teleport to
//
//=============================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class TeleportArea : TeleportMarkerBase
    {
        //Private data
        private MeshRenderer areaMesh;
        private bool highlighted;
        private Color highlightedTintColor = Color.clear;
        private Color lockedTintColor = Color.clear;
        private int tintColorId;

        private Color visibleTintColor = Color.clear;

        //Public properties
        public Bounds meshBounds { get; private set; }

        //-------------------------------------------------
        public void Awake()
        {
            areaMesh = GetComponent<MeshRenderer>();

            tintColorId = Shader.PropertyToID("_TintColor");

            CalculateBounds();
        }


        //-------------------------------------------------
        public void Start()
        {
            visibleTintColor = Teleport.instance.areaVisibleMaterial.GetColor(tintColorId);
            highlightedTintColor = Teleport.instance.areaHighlightedMaterial.GetColor(tintColorId);
            lockedTintColor = Teleport.instance.areaLockedMaterial.GetColor(tintColorId);
        }


        //-------------------------------------------------
        public override bool ShouldActivate(Vector3 playerPosition)
        {
            return true;
        }


        //-------------------------------------------------
        public override bool ShouldMovePlayer()
        {
            return true;
        }


        //-------------------------------------------------
        public override void Highlight(bool highlight)
        {
            if (!locked)
            {
                highlighted = highlight;

                if (highlight)
                    areaMesh.material = Teleport.instance.areaHighlightedMaterial;
                else
                    areaMesh.material = Teleport.instance.areaVisibleMaterial;
            }
        }


        //-------------------------------------------------
        public override void SetAlpha(float tintAlpha, float alphaPercent)
        {
            var tintedColor = GetTintColor();
            tintedColor.a *= alphaPercent;
            areaMesh.material.SetColor(tintColorId, tintedColor);
        }


        //-------------------------------------------------
        public override void UpdateVisuals()
        {
            if (locked)
                areaMesh.material = Teleport.instance.areaLockedMaterial;
            else
                areaMesh.material = Teleport.instance.areaVisibleMaterial;
        }


        //-------------------------------------------------
        public void UpdateVisualsInEditor()
        {
            areaMesh = GetComponent<MeshRenderer>();

            if (locked)
                areaMesh.sharedMaterial = Teleport.instance.areaLockedMaterial;
            else
                areaMesh.sharedMaterial = Teleport.instance.areaVisibleMaterial;
        }


        //-------------------------------------------------
        private bool CalculateBounds()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) return false;

            var mesh = meshFilter.sharedMesh;
            if (mesh == null) return false;

            meshBounds = mesh.bounds;
            return true;
        }


        //-------------------------------------------------
        private Color GetTintColor()
        {
            if (locked) return lockedTintColor;

            if (highlighted)
                return highlightedTintColor;
            return visibleTintColor;
        }
    }


#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [CustomEditor(typeof(TeleportArea))]
    public class TeleportAreaEditor : Editor
    {
        //-------------------------------------------------
        private void OnEnable()
        {
            if (Selection.activeTransform != null)
            {
                var teleportArea = Selection.activeTransform.GetComponent<TeleportArea>();
                if (teleportArea != null) teleportArea.UpdateVisualsInEditor();
            }
        }


        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Selection.activeTransform != null)
            {
                var teleportArea = Selection.activeTransform.GetComponent<TeleportArea>();
                if (GUI.changed && teleportArea != null) teleportArea.UpdateVisualsInEditor();
            }
        }
    }
#endif
}
//====================== Copyright 2016-2018, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using UnityEditor;

namespace HTC.UnityPlugin.Vive3DSP
{
    [CustomEditor(typeof(Vive3DSPAudioGeometricOcclusion))]
    [CanEditMultipleObjects]
    public class Vive3DSPAudioGeometricOcclusionEditor : Editor
    {
        private SerializedProperty occlusionEffect = null;
        private SerializedProperty occlusionMaterial = null;
        private SerializedProperty occlusionIntensity = null;
        private SerializedProperty highFreqAttenuation = null;
        private SerializedProperty lowFreqAttenuationRatio = null;
        private SerializedProperty occlusionEngine = null;
        private SerializedProperty occlusionRadius = null;
        private SerializedProperty occlusionSize = null;
        private SerializedProperty occlusionCenter = null;

        private GUIContent occlusionEffectLabel = new GUIContent("Occlusion Effect",
            "ON or OFF occlusion effect");
        private GUIContent occlusionMaterialLabel = new GUIContent("Occlusion Material",
            "Set material for occlusion object");
        private GUIContent occlusionIntensityLabel = new GUIContent("Occlusion Intensity",
            "Set occlusion intensity");
        private GUIContent highFreqAttenuationTapLabel = new GUIContent("High Freq. Attenuation (dB)",
            "Set high frequency attenuation level, default cut-off frequency is 5kHz");
        private GUIContent lowFreqAttenuationRatioTapLabel = new GUIContent("Low Freq. Attenuation Ratio",
            "Set low frequency attenuation ratio");
        private GUIContent highFreqAttenuationLabel = new GUIContent(" ",
            "Set high frequency attenuation level, default cut-off frequency is 5kHz");
        private GUIContent lowFreqAttenuationRatioLabel = new GUIContent(" ",
            "Set low frequency attenuation ratio");
        private GUIContent surfaceMaterialLabel = new GUIContent("Room Surface Material",
            "Set room surface materials for reverb effect");
        private GUIContent occlusionEngineLabel = new GUIContent("Occlusion Engine",
            "Set occlusion Engine");
        private GUIContent occlusionRadiusLabel = new GUIContent("Occlusion Radius",
            "Set sphere occlusion radius");
        private GUIContent occlusionSizeLabel = new GUIContent("Occlusion Size",
            "Set box occlusion size");
        private GUIContent occlusionCenterLabel = new GUIContent("Occlusion Center",
            "Set occlusion center");

        void OnEnable()
        {
            occlusionEffect = serializedObject.FindProperty("occlusionEffect");
            occlusionMaterial = serializedObject.FindProperty("occlusionMaterial");
            occlusionIntensity = serializedObject.FindProperty("occlusionIntensity");
            highFreqAttenuation = serializedObject.FindProperty("highFreqAttenuation");
            lowFreqAttenuationRatio = serializedObject.FindProperty("lowFreqAttenuationRatio");
            occlusionEngine = serializedObject.FindProperty("occlusionEngine");
            occlusionRadius = serializedObject.FindProperty("occlusionRadius");
            occlusionSize = serializedObject.FindProperty("occlusionSize");
            occlusionCenter = serializedObject.FindProperty("occlusionCenter");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(occlusionEffect, occlusionEffectLabel);
            EditorGUILayout.PropertyField(occlusionMaterial, occlusionMaterialLabel);
            if (occlusionMaterial.enumValueIndex == (int)Vive3DSPAudio.OccMaterial.UserDefine)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField(highFreqAttenuationTapLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.Slider(highFreqAttenuation, -50.0f, 0.0f, highFreqAttenuationLabel);
                --EditorGUI.indentLevel;
                EditorGUILayout.LabelField(lowFreqAttenuationRatioTapLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.Slider(lowFreqAttenuationRatio, 0.0f, 1.0f, lowFreqAttenuationRatioLabel);
                --EditorGUI.indentLevel;
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.Slider(occlusionIntensity, 1.0f, 2.0f, occlusionIntensityLabel);
            EditorGUILayout.PropertyField(occlusionEngine, occlusionEngineLabel);
            EditorGUILayout.PropertyField(occlusionCenter, occlusionCenterLabel);

            if (occlusionEngine.enumValueIndex == (int)Vive3DSPAudio.OccEngineMode.Sphere)
            {
                EditorGUILayout.PropertyField(occlusionRadius, occlusionRadiusLabel);   
            }

            if (occlusionEngine.enumValueIndex == (int)Vive3DSPAudio.OccEngineMode.Box)
            {
                EditorGUILayout.PropertyField(occlusionSize, occlusionSizeLabel);   
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSurfaceMaterial(SerializedProperty surfaceMaterial)
        {
            surfaceMaterialLabel.text = surfaceMaterial.displayName;
            EditorGUILayout.PropertyField(surfaceMaterial, surfaceMaterialLabel);
        }
    }
}


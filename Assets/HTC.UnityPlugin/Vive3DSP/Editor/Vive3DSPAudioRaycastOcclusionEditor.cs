//====================== Copyright 2016-2018, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using UnityEditor;

namespace HTC.UnityPlugin.Vive3DSP
{
    [CustomEditor(typeof(Vive3DSPAudioRaycastOcclusion))]
    [CanEditMultipleObjects]
    public class Vive3DSPAudioRaycastOcclusionEditor : Editor
    {
        private SerializedProperty rayNumber = null;
        private SerializedProperty occlusionEffect = null;
        private SerializedProperty occlusionMaterial = null;
        private SerializedProperty occlusionIntensity = null;
        private SerializedProperty highFreqAttenuation = null;
        private SerializedProperty lowFreqAttenuationRatio = null;

        private GUIContent rayNumberLabel = new GUIContent("Raycast Quantity",
            "Number of raycasts");
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

        void OnEnable()
        {
            rayNumber = serializedObject.FindProperty("rayNumber");
            occlusionEffect = serializedObject.FindProperty("occlusionEffect");
            occlusionMaterial = serializedObject.FindProperty("occlusionMaterial");
            occlusionIntensity = serializedObject.FindProperty("occlusionIntensity");
            highFreqAttenuation = serializedObject.FindProperty("highFreqAttenuation");
            lowFreqAttenuationRatio = serializedObject.FindProperty("lowFreqAttenuationRatio");
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
            EditorGUILayout.IntSlider(rayNumber, 1, 30, rayNumberLabel);
            serializedObject.ApplyModifiedProperties();
        }
    }
}


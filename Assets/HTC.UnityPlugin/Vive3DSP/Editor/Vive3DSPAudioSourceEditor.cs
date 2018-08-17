//====================== Copyright 2016-2018, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using UnityEditor;
using System;

namespace HTC.UnityPlugin.Vive3DSP
{
    [CustomEditor(typeof(Vive3DSPAudioSource))]
    [CanEditMultipleObjects]
    public class Vive3DSPAudioSourceEditor : Editor
    {
        private SerializedProperty gain = null;
        private SerializedProperty distanceMode = null;
        private SerializedProperty drc = null;
        private SerializedProperty spatializer = null;
        private SerializedProperty reverb = null;
        private SerializedProperty occlusion = null;
        private SerializedProperty minimumDecayVolumeDb = null;
        private SerializedProperty eqGain = null;
        private SerializedProperty[] eqGainArray = new SerializedProperty[20];

        private float[] prevEQGainArray = new float[20];

        private GUIContent gainLabel = new GUIContent("Gain (dB)",
            "Set the gain of the sound source");
        private GUIContent distanceModeSwitchLabel = new GUIContent("Overwrite Volume Rolloff",
            "Enable 3DSP sound decay effect to overwrite audio source volume rolloff");
        private GUIContent distanceModeLabel = new GUIContent("Sound Decay Effect",
            "Set sound decay mode");
        private GUIContent drcLabel = new GUIContent("DRC",
            "Set the DRC feature");
        private GUIContent spatializerLabel = new GUIContent("3D Sound Effect",
           "Set the 3D sound effect feature");
        private GUIContent reverbLabel = new GUIContent("Room Effect",
           "Set the reverb effect feature");
	    private GUIContent reverbModeLabel = new GUIContent("Room Reverb Mode", "Set the reverb mode");
        private GUIContent binauralEngineLabel = new GUIContent("Binaural Engine", "Set the binaural reverb engine");
        private GUIContent graphicEQLabel = new GUIContent("Graphic Equalizer",
           "Set the graphic equalizer feature");
        private GUIContent occlusionLabel = new GUIContent("Occlusion Effect",
           "Set the occlusion effect feature");
        private GUIContent minimumDecayVolumeTapDbLabel = new GUIContent("Minimum Decay Volume (dB)",
            "Set minimum decay volume");
        private GUIContent minimumDecayVolumeDbLabel = new GUIContent(" ",
            "Set minimum decay volume");
        private GUIContent[] eqGainArrayLabel = new GUIContent[20];
        private float[] frequencys = new float[] { 31.0f, 44.0f, 63.0f, 88.0f, 125.0f, 180.0f, 250.0f, 355.0f, 500.0f, 710.0f, 1000.0f, 1400.0f, 2000.0f, 2800.0f, 4000.0f, 5600.0f, 8000.0f, 11300.0f, 16000.0f, 22000.0f };

        void OnEnable()
        {
            Vive3DSPAudioSource model = target as Vive3DSPAudioSource;
            gain = serializedObject.FindProperty("gain");
            distanceMode = serializedObject.FindProperty("distanceMode");
            drc = serializedObject.FindProperty("drc");
            spatializer = serializedObject.FindProperty("spatializer_3d");
            reverb = serializedObject.FindProperty("reverb");
            occlusion = serializedObject.FindProperty("occlusion");
            minimumDecayVolumeDb = serializedObject.FindProperty("minimumDecayVolumeDb");
            eqGain = serializedObject.FindProperty("eqGain");
            for (int idx = 0; idx < 20; idx++)
            {
                eqGainArray[idx] = eqGain.GetArrayElementAtIndex(idx);
                prevEQGainArray[idx] = eqGainArray[idx].floatValue;
                if (frequencys[idx] < 1000.0f)
                    eqGainArrayLabel[idx] = new GUIContent((int)frequencys[idx] + " Hz", "");
                else
                    eqGainArrayLabel[idx] = new GUIContent((frequencys[idx] / 1000.0f) + " kHz", "");
                model.setEqGain(idx, eqGainArray[idx].floatValue);
            }
        }

        public override void OnInspectorGUI()
        {
            Vive3DSPAudioSource model = target as Vive3DSPAudioSource;
            AudioClip clip = model.audioSource.clip;
            if (clip != null)
            {
                var clipProperty = clip.GetType().GetProperty("ambisonic");
                if (clipProperty != null)
                {
                    if ((bool)clipProperty.GetValue(clip, null))
                    {
                        EditorGUILayout.HelpBox("The audio clip is ambisonic file. Please remove the Vive 3DSP Audio Source and disable spatialize checkbox in audio source.", MessageType.Error);
                        Debug.LogError("The audio clip is ambisonic file. Please remove the Vive 3DSP Audio Source and disable spatialize checkbox in audio source.");
                    }
                }
            }

            serializedObject.Update();

            EditorGUILayout.Slider(gain, -24.0f, 24.0f, gainLabel);
            EditorGUILayout.PropertyField(drc, drcLabel);
            EditorGUILayout.PropertyField(spatializer, spatializerLabel);
            EditorGUILayout.PropertyField(reverb, reverbLabel);
            Vive3DSPAudio.ReverbMode _rMode = (Vive3DSPAudio.ReverbMode)EditorGUILayout.EnumPopup(reverbModeLabel, model.ReverbMode);
            if (_rMode != model.ReverbMode)
                model.ReverbMode = _rMode;
            if(_rMode == Vive3DSPAudio.ReverbMode.Binaural)
            {
                ++EditorGUI.indentLevel;
                Vive3DSPAudio.BinauralEngine _bEngine = (Vive3DSPAudio.BinauralEngine)EditorGUILayout.EnumPopup(binauralEngineLabel, model.BinauralEngine);
                if (_bEngine != model.BinauralEngine)
                    model.BinauralEngine = _bEngine;
                 --EditorGUI.indentLevel;
            }
            EditorGUILayout.PropertyField(occlusion, occlusionLabel);
            bool _dist_mode = EditorGUILayout.Toggle(distanceModeSwitchLabel, model.distanceModeSwitch);
            if (_dist_mode != model.distanceModeSwitch)
            {
                model.distanceModeSwitch = _dist_mode;
            }
            if (_dist_mode == true)
            {
                EditorGUILayout.PropertyField(distanceMode, distanceModeLabel);
                if (distanceMode.enumValueIndex == (int)Vive3DSPAudio.Ambisonic3dDistanceMode.QuadraticDecay
                    || distanceMode.enumValueIndex == (int)Vive3DSPAudio.Ambisonic3dDistanceMode.LinearDecay)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.LabelField(minimumDecayVolumeTapDbLabel);
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(minimumDecayVolumeDb, -96.0f, 0.0f, minimumDecayVolumeDbLabel);
                    --EditorGUI.indentLevel;
                    --EditorGUI.indentLevel;
                }
            }
            else { 
                EditorGUILayout.HelpBox("To overwirte Audio Source volume rolloff will enable 3DSP Sound Decay Effect", MessageType.Info);
            }

            bool graphic_eq_switch = (model.GraphicEQ > 0.5 ? true : false);
            bool _graphic_EQ = EditorGUILayout.Toggle(graphicEQLabel, graphic_eq_switch);
            if (_graphic_EQ != graphic_eq_switch) {
                model.GraphicEQ = (_graphic_EQ == true ? 1.0f : 0.0f);
            }
            if (_graphic_EQ == true)
            {
                ++EditorGUI.indentLevel;
                for (int idx = 0; idx < frequencys.Length; idx++)
                {
                    EditorGUILayout.Slider(eqGainArray[idx], -12.0f, 12.0f, eqGainArrayLabel[idx]);
                    if (eqGainArray[idx].floatValue != prevEQGainArray[idx])
                    {
                        prevEQGainArray[idx] = eqGainArray[idx].floatValue;
                        model.setEqGain(idx, eqGainArray[idx].floatValue);
                    }
                }

                if (GUILayout.Button("Set to default", GUILayout.Width(100), GUILayout.Height(20)))
                {
                    model.setEQGainToDefault();
                }
                --EditorGUI.indentLevel;
            }
            
            EditorGUILayout.Separator();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}



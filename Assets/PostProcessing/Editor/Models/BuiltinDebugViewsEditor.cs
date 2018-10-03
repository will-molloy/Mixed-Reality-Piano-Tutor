using UnityEngine.PostProcessing;

namespace UnityEditor.PostProcessing
{
    [PostProcessingModelEditor(typeof(BuiltinDebugViewsModel), true)]
    public class BuiltinDebugViewsEditor : PostProcessingModelEditor
    {
        private DepthSettings m_Depth;

        private SerializedProperty m_Mode;
        private MotionVectorsSettings m_MotionVectors;

        public override void OnEnable()
        {
            m_Mode = FindSetting((BuiltinDebugViewsModel.Settings x) => x.mode);

            m_Depth = new DepthSettings
            {
                scale = FindSetting((BuiltinDebugViewsModel.Settings x) => x.depth.scale)
            };

            m_MotionVectors = new MotionVectorsSettings
            {
                sourceOpacity = FindSetting((BuiltinDebugViewsModel.Settings x) => x.motionVectors.sourceOpacity),
                motionImageOpacity =
                    FindSetting((BuiltinDebugViewsModel.Settings x) => x.motionVectors.motionImageOpacity),
                motionImageAmplitude =
                    FindSetting((BuiltinDebugViewsModel.Settings x) => x.motionVectors.motionImageAmplitude),
                motionVectorsOpacity =
                    FindSetting((BuiltinDebugViewsModel.Settings x) => x.motionVectors.motionVectorsOpacity),
                motionVectorsResolution = FindSetting((BuiltinDebugViewsModel.Settings x) =>
                    x.motionVectors.motionVectorsResolution),
                motionVectorsAmplitude = FindSetting((BuiltinDebugViewsModel.Settings x) =>
                    x.motionVectors.motionVectorsAmplitude)
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_Mode);

            var mode = m_Mode.intValue;

            if (mode == (int) BuiltinDebugViewsModel.Mode.Depth)
            {
                EditorGUILayout.PropertyField(m_Depth.scale);
            }
            else if (mode == (int) BuiltinDebugViewsModel.Mode.MotionVectors)
            {
                EditorGUILayout.HelpBox("Switch to play mode to see motion vectors.", MessageType.Info);

                EditorGUILayout.LabelField("Source Image", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_MotionVectors.sourceOpacity, EditorGUIHelper.GetContent("Opacity"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Motion Vectors (overlay)", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                if (m_MotionVectors.motionImageOpacity.floatValue > 0f)
                    EditorGUILayout.HelpBox("Please keep opacity to 0 if you're subject to motion sickness.",
                        MessageType.Warning);

                EditorGUILayout.PropertyField(m_MotionVectors.motionImageOpacity,
                    EditorGUIHelper.GetContent("Opacity"));
                EditorGUILayout.PropertyField(m_MotionVectors.motionImageAmplitude,
                    EditorGUIHelper.GetContent("Amplitude"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Motion Vectors (arrows)", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_MotionVectors.motionVectorsOpacity,
                    EditorGUIHelper.GetContent("Opacity"));
                EditorGUILayout.PropertyField(m_MotionVectors.motionVectorsResolution,
                    EditorGUIHelper.GetContent("Resolution"));
                EditorGUILayout.PropertyField(m_MotionVectors.motionVectorsAmplitude,
                    EditorGUIHelper.GetContent("Amplitude"));
                EditorGUI.indentLevel--;
            }
            else
            {
                CheckActiveEffect(
                    mode == (int) BuiltinDebugViewsModel.Mode.AmbientOcclusion && !profile.ambientOcclusion.enabled,
                    "Ambient Occlusion");
                CheckActiveEffect(mode == (int) BuiltinDebugViewsModel.Mode.FocusPlane && !profile.depthOfField.enabled,
                    "Depth Of Field");
                CheckActiveEffect(
                    mode == (int) BuiltinDebugViewsModel.Mode.EyeAdaptation && !profile.eyeAdaptation.enabled,
                    "Eye Adaptation");
                CheckActiveEffect(
                    (mode == (int) BuiltinDebugViewsModel.Mode.LogLut ||
                     mode == (int) BuiltinDebugViewsModel.Mode.PreGradingLog) && !profile.colorGrading.enabled,
                    "Color Grading");
                CheckActiveEffect(mode == (int) BuiltinDebugViewsModel.Mode.UserLut && !profile.userLut.enabled,
                    "User Lut");
            }
        }

        private void CheckActiveEffect(bool expr, string name)
        {
            if (expr)
                EditorGUILayout.HelpBox(string.Format("{0} isn't enabled, the debug view won't work.", name),
                    MessageType.Warning);
        }

        private struct DepthSettings
        {
            public SerializedProperty scale;
        }

        private struct MotionVectorsSettings
        {
            public SerializedProperty sourceOpacity;
            public SerializedProperty motionImageOpacity;
            public SerializedProperty motionImageAmplitude;
            public SerializedProperty motionVectorsOpacity;
            public SerializedProperty motionVectorsResolution;
            public SerializedProperty motionVectorsAmplitude;
        }
    }
}
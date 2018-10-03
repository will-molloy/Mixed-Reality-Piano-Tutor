using UnityEngine.PostProcessing;

namespace UnityEditor.PostProcessing
{
    [PostProcessingModelEditor(typeof(AmbientOcclusionModel))]
    public class AmbientOcclusionModelEditor : PostProcessingModelEditor
    {
        private SerializedProperty m_AmbientOnly;
        private SerializedProperty m_Downsampling;
        private SerializedProperty m_ForceForwardCompatibility;
        private SerializedProperty m_HighPrecision;
        private SerializedProperty m_Intensity;
        private SerializedProperty m_Radius;
        private SerializedProperty m_SampleCount;

        public override void OnEnable()
        {
            m_Intensity = FindSetting((AmbientOcclusionModel.Settings x) => x.intensity);
            m_Radius = FindSetting((AmbientOcclusionModel.Settings x) => x.radius);
            m_SampleCount = FindSetting((AmbientOcclusionModel.Settings x) => x.sampleCount);
            m_Downsampling = FindSetting((AmbientOcclusionModel.Settings x) => x.downsampling);
            m_ForceForwardCompatibility =
                FindSetting((AmbientOcclusionModel.Settings x) => x.forceForwardCompatibility);
            m_AmbientOnly = FindSetting((AmbientOcclusionModel.Settings x) => x.ambientOnly);
            m_HighPrecision = FindSetting((AmbientOcclusionModel.Settings x) => x.highPrecision);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_Intensity);
            EditorGUILayout.PropertyField(m_Radius);
            EditorGUILayout.PropertyField(m_SampleCount);
            EditorGUILayout.PropertyField(m_Downsampling);
            EditorGUILayout.PropertyField(m_ForceForwardCompatibility);
            EditorGUILayout.PropertyField(m_HighPrecision, EditorGUIHelper.GetContent("High Precision (Forward)"));

            using (new EditorGUI.DisabledGroupScope(m_ForceForwardCompatibility.boolValue))
            {
                EditorGUILayout.PropertyField(m_AmbientOnly,
                    EditorGUIHelper.GetContent("Ambient Only (Deferred + HDR)"));
            }
        }
    }
}
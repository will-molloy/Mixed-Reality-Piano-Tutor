using UnityEngine.PostProcessing;

namespace UnityEditor.PostProcessing
{
    [PostProcessingModelEditor(typeof(DepthOfFieldModel))]
    public class DepthOfFieldModelEditor : PostProcessingModelEditor
    {
        private SerializedProperty m_Aperture;
        private SerializedProperty m_FocalLength;
        private SerializedProperty m_FocusDistance;
        private SerializedProperty m_KernelSize;
        private SerializedProperty m_UseCameraFov;

        public override void OnEnable()
        {
            m_FocusDistance = FindSetting((DepthOfFieldModel.Settings x) => x.focusDistance);
            m_Aperture = FindSetting((DepthOfFieldModel.Settings x) => x.aperture);
            m_FocalLength = FindSetting((DepthOfFieldModel.Settings x) => x.focalLength);
            m_UseCameraFov = FindSetting((DepthOfFieldModel.Settings x) => x.useCameraFov);
            m_KernelSize = FindSetting((DepthOfFieldModel.Settings x) => x.kernelSize);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_FocusDistance);
            EditorGUILayout.PropertyField(m_Aperture, EditorGUIHelper.GetContent("Aperture (f-stop)"));

            EditorGUILayout.PropertyField(m_UseCameraFov, EditorGUIHelper.GetContent("Use Camera FOV"));
            if (!m_UseCameraFov.boolValue)
                EditorGUILayout.PropertyField(m_FocalLength, EditorGUIHelper.GetContent("Focal Length (mm)"));

            EditorGUILayout.PropertyField(m_KernelSize);
        }
    }
}
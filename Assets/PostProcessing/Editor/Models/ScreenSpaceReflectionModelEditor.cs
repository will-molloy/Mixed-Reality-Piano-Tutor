using UnityEngine.PostProcessing;

namespace UnityEditor.PostProcessing
{
    [PostProcessingModelEditor(typeof(ScreenSpaceReflectionModel))]
    public class ScreenSpaceReflectionModelEditor : PostProcessingModelEditor
    {
        private IntensitySettings m_Intensity;
        private ReflectionSettings m_Reflection;
        private ScreenEdgeMask m_ScreenEdgeMask;

        public override void OnEnable()
        {
            m_Intensity = new IntensitySettings
            {
                reflectionMultiplier =
                    FindSetting((ScreenSpaceReflectionModel.Settings x) => x.intensity.reflectionMultiplier),
                fadeDistance = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.intensity.fadeDistance),
                fresnelFade = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.intensity.fresnelFade),
                fresnelFadePower = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.intensity.fresnelFadePower)
            };

            m_Reflection = new ReflectionSettings
            {
                blendType = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.blendType),
                reflectionQuality =
                    FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.reflectionQuality),
                maxDistance = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.maxDistance),
                iterationCount = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.iterationCount),
                stepSize = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.stepSize),
                widthModifier = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.widthModifier),
                reflectionBlur = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.reflectionBlur),
                reflectBackfaces = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.reflection.reflectBackfaces)
            };

            m_ScreenEdgeMask = new ScreenEdgeMask
            {
                intensity = FindSetting((ScreenSpaceReflectionModel.Settings x) => x.screenEdgeMask.intensity)
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This effect only works with the deferred rendering path.", MessageType.Info);

            EditorGUILayout.LabelField("Reflection", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_Reflection.blendType);
            EditorGUILayout.PropertyField(m_Reflection.reflectionQuality);
            EditorGUILayout.PropertyField(m_Reflection.maxDistance);
            EditorGUILayout.PropertyField(m_Reflection.iterationCount);
            EditorGUILayout.PropertyField(m_Reflection.stepSize);
            EditorGUILayout.PropertyField(m_Reflection.widthModifier);
            EditorGUILayout.PropertyField(m_Reflection.reflectionBlur);
            EditorGUILayout.PropertyField(m_Reflection.reflectBackfaces);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Intensity", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_Intensity.reflectionMultiplier);
            EditorGUILayout.PropertyField(m_Intensity.fadeDistance);
            EditorGUILayout.PropertyField(m_Intensity.fresnelFade);
            EditorGUILayout.PropertyField(m_Intensity.fresnelFadePower);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Screen Edge Mask", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_ScreenEdgeMask.intensity);
            EditorGUI.indentLevel--;
        }

        private struct IntensitySettings
        {
            public SerializedProperty reflectionMultiplier;
            public SerializedProperty fadeDistance;
            public SerializedProperty fresnelFade;
            public SerializedProperty fresnelFadePower;
        }

        private struct ReflectionSettings
        {
            public SerializedProperty blendType;
            public SerializedProperty reflectionQuality;
            public SerializedProperty maxDistance;
            public SerializedProperty iterationCount;
            public SerializedProperty stepSize;
            public SerializedProperty widthModifier;
            public SerializedProperty reflectionBlur;
            public SerializedProperty reflectBackfaces;
        }

        private struct ScreenEdgeMask
        {
            public SerializedProperty intensity;
        }
    }
}
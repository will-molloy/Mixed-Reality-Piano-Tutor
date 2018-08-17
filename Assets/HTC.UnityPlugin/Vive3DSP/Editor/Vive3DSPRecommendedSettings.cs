using UnityEditor;
#if VIU_PLUGIN
using HTC.UnityPlugin.Vive;
#endif

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [InitializeOnLoad]
    public static class Vive3DSPRecommendedSettings
    {
#if VIU_PLUGIN
        static Vive3DSPRecommendedSettings()
        {
            VIUVersionCheck.AddRecommendedSetting(new VIUVersionCheck.RecommendedSetting<string>()
            {
                settingTitle = "Audio Spatializer Plugin",
                currentValueFunc = () =>
                {
                    var audioSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0]);
                    var spatializerProp = audioSettings.FindProperty("m_SpatializerPlugin");
                    var v = spatializerProp.stringValue;
                    return string.IsNullOrEmpty(v) ? "None" : v;
                },
                setValueFunc = (v) =>
                {
                    var audioSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0]);
                    var spatializerProp = audioSettings.FindProperty("m_SpatializerPlugin");
                    spatializerProp.stringValue = v;
                    audioSettings.ApplyModifiedProperties();
                },
                recommendedValue = "VIVE 3DSP Spatializer",
            });

#if UNITY_2017_1_OR_NEWER
            VIUVersionCheck.AddRecommendedSetting(new VIUVersionCheck.RecommendedSetting<string>()
            {
                settingTitle = "Ambisonic Decoder Plugin",
                currentValueFunc = () =>
                {
                    var audioSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0]);
                    var spatializerProp = audioSettings.FindProperty("m_AmbisonicDecoderPlugin");
                    var v = spatializerProp.stringValue;
                    return string.IsNullOrEmpty(v) ? "None" : v;
                },
                setValueFunc = (v) =>
                {
                    var audioSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0]);
                    var spatializerProp = audioSettings.FindProperty("m_AmbisonicDecoderPlugin");
                    spatializerProp.stringValue = v;
                    audioSettings.ApplyModifiedProperties();
                },
                recommendedValue = "VIVE 3DSP Spatializer",
            });
#endif
        }
#endif
    }
}
using System;

namespace UnityEngine.PostProcessing
{
    [Serializable]
    public class DitheringModel : PostProcessingModel
    {
        [SerializeField] private Settings m_Settings = Settings.defaultSettings;

        public Settings settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }

        public override void Reset()
        {
            m_Settings = Settings.defaultSettings;
        }

        [Serializable]
        public struct Settings
        {
            public static Settings defaultSettings => new Settings();
        }
    }
}
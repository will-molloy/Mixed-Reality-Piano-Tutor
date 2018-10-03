using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace UnityEditor.PostProcessing
{
    public abstract class PostProcessingMonitor : IDisposable
    {
        protected PostProcessingInspector m_BaseEditor;
        protected PostProcessingProfile.MonitorSettings m_MonitorSettings;

        public virtual void Dispose()
        {
        }

        public void Init(PostProcessingProfile.MonitorSettings monitorSettings, PostProcessingInspector baseEditor)
        {
            m_MonitorSettings = monitorSettings;
            m_BaseEditor = baseEditor;
        }

        public abstract bool IsSupported();

        public abstract GUIContent GetMonitorTitle();

        public virtual void OnMonitorSettings()
        {
        }

        public abstract void OnMonitorGUI(Rect r);

        public virtual void OnFrameData(RenderTexture source)
        {
        }
    }
}
using HTC.UnityPlugin.Vive3DSP;
using UnityEngine;

public class SelectBackgroundNoiseButtonGroup : MonoBehaviour
{
    [SerializeField]
    //private Vive3DSPAudioListener m_audioListener;
    private Vive3DSPAudioRoom m_audioRoom;
    [SerializeField]
    private SelectBackgroundNoiseButton m_onButton;
    //[SerializeField]
    //private SelectRoomButton m_onButton1;

    //public Vive3DSPAudioListener audioListener { get { return m_audioListener; } }
    public Vive3DSPAudioRoom audioRoom { get { return m_audioRoom; } }

    public void NotifyOn(SelectBackgroundNoiseButton button)
    {
        if (m_onButton == button) { return; }

        if (m_onButton != null)
        {
            m_onButton.isOn = false;
        }

        m_onButton = button;

        if (m_onButton != null)
        {
            m_onButton.isOn = true;
        }
    }
    //public void NotifyOn1(SelectRoomButton button)
    //{
    //    if (m_onButton1 == button) { return; }

    //    if (m_onButton1 != null)
    //    {
    //        m_onButton1.isOn = false;
    //    }

    //    m_onButton1 = button;

    //    if (m_onButton1 != null)
    //    {
    //        m_onButton1.isOn = true;
    //    }
    //}
}

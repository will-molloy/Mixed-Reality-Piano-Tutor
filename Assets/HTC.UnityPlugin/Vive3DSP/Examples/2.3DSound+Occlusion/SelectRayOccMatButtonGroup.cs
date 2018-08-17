using HTC.UnityPlugin.Vive3DSP;
using UnityEngine;

public class SelectRayOccMatButtonGroup : MonoBehaviour
{
    [SerializeField]
    private Vive3DSPAudioRaycastOcclusion m_audioRayOcclusion;
    //[SerializeField]
    //private Vive3DSPAudioRaycastOcclusion m_audioRayOcclusion;
    [SerializeField]
    private SelectRayOccMatButton m_onButton;

    public Vive3DSPAudioRaycastOcclusion audioRayOcclusion { get { return m_audioRayOcclusion; } }
    //public Vive3DSPAudioRaycastOcclusion audioRayOcclusion { get { return m_audioRayOcclusion; } }

    public void NotifyOn(SelectRayOccMatButton button)
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
}

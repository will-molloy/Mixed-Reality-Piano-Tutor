using HTC.UnityPlugin.Vive3DSP;
using UnityEngine;

public class SelectOccMatButtonGroup : MonoBehaviour
{
    [SerializeField]
    private Vive3DSPAudioGeometricOcclusion m_audioGemoOcclusion;
    [SerializeField]
    private Vive3DSPAudioRaycastOcclusion m_audioRayOcclusion;
    [SerializeField]
    private SelectOccMatButton m_onButton;

    public Vive3DSPAudioGeometricOcclusion audioGemoOcclusion { get { return m_audioGemoOcclusion; } }
    public Vive3DSPAudioRaycastOcclusion audioRayOcclusion { get { return m_audioRayOcclusion; } }

    public void NotifyOn(SelectOccMatButton button)
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

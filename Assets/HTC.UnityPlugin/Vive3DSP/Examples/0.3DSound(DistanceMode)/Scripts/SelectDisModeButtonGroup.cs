using HTC.UnityPlugin.Vive3DSP;
using UnityEngine;

public class SelectDisModeButtonGroup : MonoBehaviour
{
    [SerializeField]
    private Vive3DSPAudioSource m_audioSource;
    [SerializeField]
    private SelectDisModeButton m_onButton;//= new SelectDisModeButton();

    public Vive3DSPAudioSource audioSource { get { return m_audioSource; } }

    public void NotifyOn(SelectDisModeButton button)
    {
        if (m_onButton == button) { return; }

        if (m_onButton != null)
        {
            if(m_onButton.isOn) m_onButton.isOn = false;
        }

        m_onButton = button;

        if (m_onButton != null)
        {
            if (!m_onButton.isOn)m_onButton.isOn = true;
        }
    }
}

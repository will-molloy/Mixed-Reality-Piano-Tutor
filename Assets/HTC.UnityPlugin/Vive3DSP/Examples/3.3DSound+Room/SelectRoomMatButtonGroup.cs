using HTC.UnityPlugin.Vive3DSP;
using UnityEngine;

public class SelectRoomMatButtonGroup : MonoBehaviour
{
    [SerializeField]
    private Vive3DSPAudioRoom m_audioRoom;
    //[SerializeField]
    private SelectRoomMatButton m_onButton;
    [SerializeField]
    private SelectRoomButton m_onButton1;

    public Vive3DSPAudioRoom audioRoom { get { return m_audioRoom; } }
    //private void Awake()
    //{
    //    //m_label.text = m_roomPlaneMat.ToString();
    //    m_onButton.isOn = true;
    //    //m_label.text = m_roomReverbPreset.ToString();

    //    //if (m_roomProperty.material_front == m_roomPlaneMat)
    //    //{
    //    //    isOn = true;
    //    //}
    //}

    public void NotifyOn(SelectRoomMatButton button)
    {
        if (m_onButton == button) { return; }
        //if (m_onButton1.isOn)
        //{
        //    m_onButton.isOn = false;
        //}
        //else
        //{

        if (m_onButton != null)
            {
                m_onButton.isOn = false;
            }

            m_onButton = button;

            if (m_onButton != null)
            {
                m_onButton.isOn = true;
            m_onButton1.isOn = false;
            //SelectRoomButton._isReverbOn = true;
            }
        //}
    }
    public void NotifyOn1(SelectRoomButton button)
    {
        m_onButton1 = button;

        if (m_onButton1 != null & m_onButton != null)
        {
            if (m_onButton1.isOn)
            {
                m_onButton1.isOn = false;
            }
            else
            {
                m_onButton1.isOn = true;
                m_onButton.isOn = false;
                m_onButton = null;
            }
        }
    }
    private void Update()
    {

            if (m_onButton1.isOn)
            {
                m_audioRoom.RoomEffect = false;
            if (m_onButton != null)
            {
                m_onButton.isOn = false;
                //m_onButton = null;
            }
        }
    }
}

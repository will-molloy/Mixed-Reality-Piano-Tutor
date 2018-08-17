using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectRoomButton : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    [SerializeField]
    private SelectRoomMatButtonGroup m_buttonGroup;
    [SerializeField]
    private Transform m_enabledObj;
    [SerializeField]
    private Transform m_disabledObj;
    [SerializeField]
    private Text m_label;

    private HashSet<ColliderHoverEventData> m_hovers = new HashSet<ColliderHoverEventData>();
    private bool m_isOn;
    //static public bool _isReverbOn;

    public bool isOn
    {
        get { return m_isOn; }
        set
        {
            if (m_isOn != value)
            {
                m_isOn = value;
                if (value)
                {
                    m_enabledObj.gameObject.SetActive(true);
                    m_disabledObj.gameObject.SetActive(false);
                    
                    m_buttonGroup.audioRoom.RoomEffect = false;
                    //m_label.text = "Reverb Off";
                    //_isReverbOn = false;
                }
                else
                {
                    m_enabledObj.gameObject.SetActive(false);
                    m_disabledObj.gameObject.SetActive(true);
                    m_buttonGroup.audioRoom.RoomEffect = true;
                    //m_label.text = "Reverb On";
                    //_isReverbOn = true;
                }
            }
        }
    }

    private void Awake()
    {
        m_buttonGroup.audioRoom.RoomEffect = false;
        isOn = true;
        m_label.text = "Reverb Off";
        //_isReverbOn = false;
    }

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        if (m_hovers.Add(eventData) && m_hovers.Count == 1)
        {
            m_buttonGroup.NotifyOn1(this);
        }
    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        m_hovers.Remove(eventData);
    }
}

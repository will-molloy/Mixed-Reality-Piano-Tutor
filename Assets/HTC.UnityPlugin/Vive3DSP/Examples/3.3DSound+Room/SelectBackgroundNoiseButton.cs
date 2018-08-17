using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectBackgroundNoiseButton : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    //[SerializeField]
    //private Vive3DSPAudio.RoomPlane m_roomPlane;
    [SerializeField]
    private Vive3DSPAudio.RoomBackgroundAudioType m_roomBNType;
    [SerializeField]
    private SelectBackgroundNoiseButtonGroup m_buttonGroup;
    [SerializeField]
    private Transform m_enabledObj;
    [SerializeField]
    private Transform m_disabledObj;
    [SerializeField]
    private Text m_label;

    private HashSet<ColliderHoverEventData> m_hovers = new HashSet<ColliderHoverEventData>();
    private bool m_isOn;
    //private bool m_effectIsOff;

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
                    //if (m_roomPlaneMat != 0)
                    //{
                    //    m_buttonGroup.audioRoom.roomEffect = true;
                        m_buttonGroup.audioRoom.BackgroundType = m_roomBNType;
                        //m_buttonGroup.audioRoom.BackWall = m_roomPlaneMat;
                        //m_buttonGroup.audioRoom.LeftWall = m_roomPlaneMat;
                        //m_buttonGroup.audioRoom.RightWall = m_roomPlaneMat;
                        //m_buttonGroup.audioRoom.Floor = m_roomPlaneMat;
                        //m_buttonGroup.audioRoom.Ceiling = m_roomPlaneMat;
                    //}
                    //else
                    //{
                    //    m_buttonGroup.audioRoom.roomEffect = false;
                    //}
                }
                else
                {
                    m_enabledObj.gameObject.SetActive(false);
                    m_disabledObj.gameObject.SetActive(true);

                }
            }
        }
    }

    private void Awake()
    {
        //if (m_roomPlaneMat.ToString() == "None")
        //{
        //    m_label.text = "Off";
        //}
        //else
        //{
        //    m_label.text = m_roomPlaneMat.ToString();
        //}
        m_label.text = m_roomBNType.ToString();

        if (m_buttonGroup.audioRoom.BackgroundType == m_roomBNType)
        {
            isOn = true;
        }
    }

    private void Update()
    {
        if(m_buttonGroup.audioRoom.BackgroundType == m_roomBNType)
        {
            isOn = true;
        }
        else
        {
            isOn = false;
        }
    }
    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        if (m_hovers.Add(eventData) && m_hovers.Count == 1)
        {
            m_buttonGroup.NotifyOn(this);
        }
    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        m_hovers.Remove(eventData);
    }
}

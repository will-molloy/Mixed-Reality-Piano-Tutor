using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectRoomMatButton : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    [SerializeField]
    private Vive3DSPAudio.RoomPlateMaterial m_roomPlaneMat;
    //[SerializeField]
    //private Vive3DSPAudio.RoomReverbPreset m_roomReverbPreset;
    [SerializeField]
    private SelectRoomMatButtonGroup m_buttonGroup;
    [SerializeField]
    private Transform m_enabledObj;
    [SerializeField]
    private Transform m_disabledObj;
    [SerializeField]
    private Text m_label;

    //private Vive3DSPAudio.VIVE_3DSP_ROOM_PROPERTY m_roomProperty;
    private HashSet<ColliderHoverEventData> m_hovers = new HashSet<ColliderHoverEventData>();
    private bool m_isOn;


    public bool isOn
    {
        get { return m_isOn; }
        set
        {
            if (m_isOn != value)
            {
                m_isOn = value;
                if (value /*|| !SelectRoomButton._isReverbOn*/)
                {
                    m_enabledObj.gameObject.SetActive(true);
                    m_disabledObj.gameObject.SetActive(false);
                    //m_roomProperty.preset = m_roomReverbPreset;// Vive3DSPAudio.RoomReverbPreset.UserDefine;
                    //m_roomProperty.material_front = m_roomPlaneMat;
                    //m_roomProperty.material_back = m_roomPlaneMat;
                    //m_roomProperty.material_left = m_roomPlaneMat;
                    //m_roomProperty.material_right = m_roomPlaneMat;
                    //m_roomProperty.material_floor = m_roomPlaneMat;
                    //m_roomProperty.material_ceiling = m_roomPlaneMat;

                    //m_buttonGroup.audioRoom.ReverbPreset = m_roomReverbPreset;
                    // m_buttonGroup.audioRoom.
                    m_buttonGroup.audioRoom.FrontWall = m_roomPlaneMat;
                    m_buttonGroup.audioRoom.BackWall = m_roomPlaneMat;
                    m_buttonGroup.audioRoom.LeftWall = m_roomPlaneMat;
                    m_buttonGroup.audioRoom.RightWall = m_roomPlaneMat;
                    m_buttonGroup.audioRoom.Floor = m_roomPlaneMat;
                    m_buttonGroup.audioRoom.Ceiling = m_roomPlaneMat;
                    //SelectRoomButton._isReverbOn = true;

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
        m_label.text = m_roomPlaneMat.ToString();
        //m_label.text = m_roomReverbPreset.ToString();

        //if (m_roomProperty.material_front == m_roomPlaneMat)
        //{
        //    isOn = true;
        //}
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
    //private void Update()
    //{
    //    if (m_buttonGroup.audioRoom.FrontWall == m_roomPlaneMat)
    //    {
    //        isOn = true;
    //    }
    //    else
    //    {
    //        isOn = false;
    //    }
    //}
}

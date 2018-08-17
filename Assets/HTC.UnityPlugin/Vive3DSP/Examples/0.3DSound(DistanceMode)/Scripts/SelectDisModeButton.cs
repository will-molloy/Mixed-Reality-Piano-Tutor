using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectDisModeButton : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    [SerializeField]
    private Vive3DSPAudio.Ambisonic3dDistanceMode m_distanceMode;
    [SerializeField]
    private SelectDisModeButtonGroup m_buttonGroup;
    [SerializeField]
    private Transform m_enabledObj;
    [SerializeField]
    private Transform m_disabledObj;
    [SerializeField]
    private Text m_label;

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
                if (value)
                {
                    m_enabledObj.gameObject.SetActive(true);
                    m_disabledObj.gameObject.SetActive(false);
                    m_buttonGroup.audioSource.DistanceMode = m_distanceMode;
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
        m_label.text = m_distanceMode.ToString();

        //if (m_buttonGroup.audioSource.distanceMode == m_distanceMode)
        //{
        //    isOn = true;
        //}
    }
    private void Update()
    {
        if (m_buttonGroup.audioSource.distanceModeSwitch)
        {
            //Debug.Log("m_buttonGroup.audioSource.distanceMode:" + m_buttonGroup.audioSource.distanceMode);
            if (m_buttonGroup.audioSource.DistanceMode == m_distanceMode)
            {
                isOn = true;
            }
            else
            {
                isOn = false;
            }
        }
        else
        {
            if (isOn)
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

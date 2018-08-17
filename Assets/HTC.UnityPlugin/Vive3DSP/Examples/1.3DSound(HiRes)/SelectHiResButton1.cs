using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectHiResButton1 : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    [SerializeField]
    private AudioSource m_audioSource_HiRes;
	[SerializeField]
	private AudioSource m_audioSource_48k;
	//[SerializeField]
	//private AudioClip[] audios;
	[SerializeField]
    private SelectHiResButton1 m_onButton;
    [SerializeField]
    private Transform m_enabledObj;
    [SerializeField]
    private Transform m_disabledObj;
    [SerializeField]
    private Text m_label;

    private HashSet<ColliderHoverEventData> m_hovers = new HashSet<ColliderHoverEventData>();
    private bool m_isOn;
    public AudioSource audioSource { get { return m_isOn ? m_audioSource_HiRes : m_audioSource_48k; } }
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
					m_audioSource_HiRes.mute = false;
					m_audioSource_48k.mute = true;

                }
                else
                {
                    m_enabledObj.gameObject.SetActive(false);
                    m_disabledObj.gameObject.SetActive(true);
					m_audioSource_HiRes.mute = true;
					m_audioSource_48k.mute = false;
                }
            }
        }
    }

    private void Awake()
    {

        if (m_label.text == "Normal")
        {
            isOn = false;
        }
        else
        {
            isOn = true;
        }
    }

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        if (m_hovers.Add(eventData) && m_hovers.Count == 1)
        {
            NotifyOn(this);
        }
    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        m_hovers.Remove(eventData);
    }
    public void NotifyOn(SelectHiResButton1 button)
    {
        if (this != null)
        {
            if (!isOn)
            {
                isOn = true;
                m_label.text = "HiRes";
            }
            else
            {
                isOn = false;
                m_label.text = "Normal";
            }
        }

    }
}

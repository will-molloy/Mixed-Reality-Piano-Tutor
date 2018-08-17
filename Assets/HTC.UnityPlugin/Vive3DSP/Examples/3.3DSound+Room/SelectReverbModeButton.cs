using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectReverbModeButton : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{

	[SerializeField]
    private SelectReverbModeButton m_onButton;
    [SerializeField]
    private Vive3DSPAudioSource m_viveAudioSource;
    [SerializeField]
    private Transform m_enabledObj;
    [SerializeField]
    private Transform m_disabledObj;
    [SerializeField]
    private Text m_label;

    private HashSet<ColliderHoverEventData> m_hovers = new HashSet<ColliderHoverEventData>();
    private bool m_isOn;
    private GameObject obj;
    //public AudioSource audioSource { get { return m_isOn ? m_au : m_audioSource_48k; } }
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
                    m_viveAudioSource.ReverbMode = Vive3DSPAudio.ReverbMode.Binaural;
					//m_audioSource_HiRes.mute = false;
					//m_audioSource_48k.mute = true;

                }
                else
                {
                    m_enabledObj.gameObject.SetActive(false);
                    m_disabledObj.gameObject.SetActive(true);
                    m_viveAudioSource.ReverbMode = Vive3DSPAudio.ReverbMode.Mono;
                    //m_audioSource_HiRes.mute = true;
                    //m_audioSource_48k.mute = false;
                }
            }
        }
    }

    private void Awake()
    {
        //obj = GameObject.Find("Selection");
        //if(obj != null)
        //{
        //    obj.SetActive(false);
        //}

        m_viveAudioSource.ReverbMode = Vive3DSPAudio.ReverbMode.Mono;
        isOn = false;
        m_label.text = "Mono";
        //if (m_label.text == "Mono")
        //{
        //    isOn = false;
        //}
        //else
        //{
        //    isOn = true;
        //}
    }

    //private void Update()
    //{
    //    if(ViveInput.GetPress(HandRole.RightHand, ControllerButton.Menu))
    //    {
    //        if(obj != null)
    //        {
    //            if (!obj.activeInHierarchy)
    //            {
    //                obj.SetActive(true);
    //            }
    //            else { obj.SetActive(false); }
    //        }
    //    }
    //}
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
    public void NotifyOn(SelectReverbModeButton button)
    {
        if (this != null)
        {
            if (!isOn)
            {
                isOn = true;
                m_label.text = "Binaural";
            }
            else
            {
                isOn = false;
                m_label.text = "Mono";
            }
        }

    }
}

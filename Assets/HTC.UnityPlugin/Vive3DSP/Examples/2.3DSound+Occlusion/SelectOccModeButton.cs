using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectOccModeButton : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{

	[SerializeField]
    private SelectOccModeButton m_onButton;
    [SerializeField]
    private Vive3DSPAudioRaycastOcclusion m_rayCastOcc;
    [SerializeField]
    private Vive3DSPAudioGeometricOcclusion m_geometricOcc;
    [SerializeField]
    private Transform m_enabledObj;
    [SerializeField]
    private Transform m_disabledObj;
    [SerializeField]
    private Text m_label;
    //[SerializeField]
    private GameObject m_obj;
    private BoxCollider m_bc;
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
                    m_rayCastOcc.enabled = true;
                    m_geometricOcc.enabled = false;
                    m_bc.enabled = true;
                    //m_audioSource_HiRes.mute = false;
                    //m_audioSource_48k.mute = true;

                }
                else
                {
                    m_enabledObj.gameObject.SetActive(false);
                    m_disabledObj.gameObject.SetActive(true);
                    m_rayCastOcc.enabled = false;
                    m_geometricOcc.enabled = true;
                    m_bc.enabled = false;
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
        m_obj = GameObject.Find("Occlusion");
        m_bc = m_obj.GetComponent<BoxCollider>();
        m_bc.enabled = false;
        m_geometricOcc.enabled = true;
        m_rayCastOcc.enabled = false;

        isOn = false;
        m_label.text = "Geometric Occlusion";
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
    public void NotifyOn(SelectOccModeButton button)
    {
        if (this != null)
        {
            if (!isOn)
            {
                isOn = true;
                m_label.text = "RayCast Occlusion";
            }
            else
            {
                isOn = false;
                m_label.text = "Geometric Occlusion";
            }
        }

    }
}

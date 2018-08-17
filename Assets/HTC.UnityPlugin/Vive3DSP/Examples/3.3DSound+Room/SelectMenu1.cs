using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive3DSP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HTC.UnityPlugin.Vive;

public class SelectMenu1 : MonoBehaviour
{
    [SerializeField]
    private Vive3DSPAudioRoom m_audioRoom;
    private GameObject obj;
    //public AudioSource audioSource { get { return m_isOn ? m_au : m_audioSource_48k; } }
    

    private void Awake()
    {
        if (m_audioRoom != null)
            m_audioRoom.RoomEffect = false;
        obj = GameObject.Find("Selection");
        if(obj != null)
        {
            obj.SetActive(false);
        }
    }
    private void Update()
    {
        if(ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Menu))
        {
            if (obj != null)
            {
                if (!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                }
                else
            { obj.SetActive(false);
            }
        }
    }
        if (ViveInput.GetPressUp(HandRole.RightHand, ControllerButton.Menu))
        {
            if (obj != null)
            {
                if (!obj.activeInHierarchy)
                {
                    //obj.SetActive(false);
                }
                else
                {
                    //obj.SetActive(true);
                }
            }
        }
    }
}

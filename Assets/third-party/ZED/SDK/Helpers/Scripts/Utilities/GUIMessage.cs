//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using sl;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Controls the message displayed during the opening and disconnection of the ZED
/// </summary>
public class GUIMessage : MonoBehaviour
{
    /// <summary>
    ///     Reference to the spinner for the stereo rig's left eye
    /// </summary>
    private GameObject imageleft;

    /// <summary>
    ///     Reference to the spinner for the mono rig
    /// </summary>
    private GameObject imagemono;

    /// <summary>
    ///     Reference to the spinner for the stereo rig's right eye
    /// </summary>
    private GameObject imageright;

    /// <summary>
    ///     Stop the update of the script when the flag is set to true
    /// </summary>
    private bool init;

    /// <summary>
    ///     Previous opening error
    /// </summary>
    private ERROR_CODE oldInitStatus;

    /// <summary>
    ///     Event to say the ZED is ready, start the timer, to wait the textures to be initialized
    /// </summary>
    private bool ready;

    /// <summary>
    ///     Text under the loading sign for stereo rig's left eye
    /// </summary>
    private Text textleft;

    /// <summary>
    ///     Text under the loading sign for mono rig
    /// </summary>
    private Text textmono;

    /// <summary>
    ///     Text under the loading sign for stereo rig's right eye
    /// </summary>
    private Text textright;

    /// <summary>
    ///     Timer to stop the gif from rotating
    /// </summary>
    private float timer;

    /// <summary>
    ///     Add few time before stopping the ZED launching screen to let time to the other textures to be initialized
    /// </summary>
    private float timerWarning;

    /// <summary>
    ///     Warning container for stereo rig's left eye
    /// </summary>
    private GameObject warningleft;

    /// <summary>
    ///     Warning container for mono rig, contains the text, background and loading sign
    /// </summary>
    private GameObject warningmono;

    /// <summary>
    ///     Warning container for stereo rig's right eye
    /// </summary>
    private GameObject warningright;

    private void Awake()
    {
        oldInitStatus = ERROR_CODE.ERROR_CODE_LAST;
        if (!ZEDManager.IsStereoRig) //Without VR, we use a Screen Space - Overlay canvas. 
        {
            warningmono = Instantiate(Resources.Load("PrefabsUI/Warning") as GameObject, transform);
            warningmono.SetActive(true);
            textmono = warningmono.GetComponentInChildren<Text>();
            textmono.color = Color.white;

            if (!ZEDCamera.CheckPlugin())
                textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.SDK_NOT_INSTALLED);
            imagemono = warningmono.transform.GetChild(0).GetChild(1).gameObject;
            imagemono.transform.parent.gameObject.SetActive(true);
            ready = false;
        }
        else //In VR, we use two Screen Space - Camera canvases, one for each eye. 
        {
            //Setup the left warning prefab
            warningleft = Instantiate(Resources.Load("PrefabsUI/Warning_VR") as GameObject,
                ZEDManager.Instance.GetLeftCameraTransform());
            warningleft.SetActive(true);
            warningleft.GetComponent<Canvas>().worldCamera =
                ZEDManager.Instance.GetLeftCameraTransform().GetComponent<Camera>();
            warningleft.GetComponent<Canvas>().planeDistance = 1;
            textleft = warningleft.GetComponentInChildren<Text>();
            textleft.color = Color.white;
            imageleft = warningleft.transform.GetChild(0).GetChild(1).gameObject;
            imageleft.transform.parent.gameObject.SetActive(true);

            //Setup the right warning prefab
            warningright = Instantiate(Resources.Load("PrefabsUI/Warning_VR") as GameObject,
                ZEDManager.Instance.GetRightCameraTransform());
            warningright.SetActive(true);
            warningright.GetComponent<Canvas>().worldCamera =
                ZEDManager.Instance.GetRightCameraTransform().GetComponent<Camera>();
            warningright.GetComponent<Canvas>().planeDistance = 1;
            textright = warningright.GetComponentInChildren<Text>();
            textright.color = Color.white;
            imageright = warningright.transform.GetChild(0).GetChild(1).gameObject;
            imageright.transform.parent.gameObject.SetActive(true);

            if (!ZEDCamera.CheckPlugin())
            {
                textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.SDK_NOT_INSTALLED);
                textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.SDK_NOT_INSTALLED);
            }

            ready = false;
        }
    }

    private void OnEnable()
    {
        ZEDManager.OnZEDReady += Ready;
        ZEDManager.OnZEDDisconnected += ZEDDisconnected;
    }

    private void OnDisable()
    {
        ZEDManager.OnZEDReady -= Ready;
        ZEDManager.OnZEDDisconnected -= ZEDDisconnected;
    }

    /// <summary>
    ///     Event if ZED is disconnected
    /// </summary>
    private void ZEDDisconnected()
    {
        if (warningmono)
        {
            warningmono.SetActive(true);
            imagemono.SetActive(true);

            warningmono.transform.GetChild(0).gameObject.SetActive(true);
            textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.ZED_IS_DISCONNECETD);
            warningmono.layer = 30;

            ready = false;
        }

        if (warningleft)
        {
            warningleft.SetActive(true);
            imageleft.SetActive(true);
            warningleft.transform.GetChild(0).gameObject.SetActive(true);
            textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.ZED_IS_DISCONNECETD);
            warningleft.layer = 30;

            warningright.SetActive(true);
            imageright.SetActive(true);
            warningright.transform.GetChild(0).gameObject.SetActive(true);
            textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.ZED_IS_DISCONNECETD);
            warningright.layer = 30;

            ready = false;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!init)
        {
            var e = ZEDManager.LastInitStatus;

            if (e == ERROR_CODE.SUCCESS)
            {
                timer += Time.deltaTime;
                if (timer > 0.2f)
                {
                    if (textmono)
                    {
                        textmono.text = "";
                    }
                    else if (textleft)
                    {
                        textleft.text = "";
                        textright.text = "";
                    }
                }

                if (imagemono)
                {
                    imagemono.gameObject.SetActive(false);
                }
                else if (imageleft)
                {
                    imageleft.gameObject.SetActive(false);
                    imageright.gameObject.SetActive(false);
                }
            }
            else if (e == ERROR_CODE.ERROR_CODE_LAST)
            {
                if (textmono)
                {
                    textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_LOADING);
                    //textmono.color = Color.white;
                }
                else if (textleft)
                {
                    textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_LOADING);
                    //textleft.color = Color.white;

                    textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_LOADING);
                    //textright.color = Color.white;
                }
            }
            else if (e == ERROR_CODE.CAMERA_NOT_DETECTED && oldInitStatus == ERROR_CODE.CAMERA_NOT_DETECTED)
            {
                if (textmono)
                {
                    textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.UNABLE_TO_OPEN_CAMERA);
                }
                else if (textleft)
                {
                    textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.UNABLE_TO_OPEN_CAMERA);
                    textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.UNABLE_TO_OPEN_CAMERA);
                }
            }
            else if (e == ERROR_CODE.CAMERA_DETECTION_ISSUE && oldInitStatus == ERROR_CODE.CAMERA_DETECTION_ISSUE)
            {
                if (textmono)
                {
                    textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_DETECTION_ISSUE);
                }
                else if (textleft)
                {
                    textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_DETECTION_ISSUE);
                    textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_DETECTION_ISSUE);
                }
            }
            else if (e == ERROR_CODE.SENSOR_NOT_DETECTED && oldInitStatus == ERROR_CODE.SENSOR_NOT_DETECTED)
            {
                if (textmono)
                {
                    textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.SENSOR_NOT_DETECTED);
                }
                else if (textleft)
                {
                    textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.SENSOR_NOT_DETECTED);
                    textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.SENSOR_NOT_DETECTED);
                }
            }
            else if (e == ERROR_CODE.LOW_USB_BANDWIDTH && oldInitStatus == ERROR_CODE.LOW_USB_BANDWIDTH)
            {
                if (textmono)
                {
                    textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.LOW_USB_BANDWIDTH);
                }
                else if (textleft)
                {
                    textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.LOW_USB_BANDWIDTH);
                    textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.LOW_USB_BANDWIDTH);
                }
            }
            else if (e == oldInitStatus)
            {
                if (textmono)
                {
                    textmono.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_NOT_INITIALIZED);
                }
                else if (textleft)
                {
                    textleft.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_NOT_INITIALIZED);
                    textright.text = ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.CAMERA_NOT_INITIALIZED);
                }
            }

            oldInitStatus = e;
        }

        if (ready)
        {
            timerWarning += Time.deltaTime;
            if (timerWarning > 0.5f)
            {
                if (warningmono)
                {
                    warningmono.SetActive(false);
                }
                else if (warningleft)
                {
                    warningleft.SetActive(false);
                    warningright.SetActive(false);
                }
            }

            init = true;
            //timerWarning = 0;

            if (imagemono)
            {
                imagemono.gameObject.transform.parent.gameObject.SetActive(false);
            }
            else if (imageleft)
            {
                imageleft.gameObject.transform.parent.gameObject.SetActive(false);
                imageright.gameObject.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private void Ready()
    {
        ready = true;
    }
}
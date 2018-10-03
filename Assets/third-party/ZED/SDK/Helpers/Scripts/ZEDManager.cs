using System;
using System.Collections;
using System.IO;
using System.Threading;
using sl;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
#if UNITY_EDITOR
using UnityEditor;

#endif


public class ZEDManager : MonoBehaviour
{
    public delegate void onCamBrightnessChangeDelegate(float newVal);

    /// Event when ZED is disconnected
    public delegate void OnZEDManagerDisconnected();


    /////////////////////////////////////
    //////  ZED specific events    //////
    /////////////////////////////////////
    /// Event when ZED is Ready
    public delegate void OnZEDManagerReady();

    /// <summary>
    ///     Available Rendering path as ZEDRenderingMode
    /// </summary>
    public enum ZEDRenderingMode
    {
        FORWARD = RenderingPath.Forward,
        DEFERRED = RenderingPath.DeferredShading
    }


    ///////////////////////////////////////
    /////////// Static States /////////////
    ///////////////////////////////////////

    /// <summary>
    ///     Flag to check is AR mode is activated
    /// </summary>
    private static bool isStereoRig;

    /// <summary>
    ///     Opening Thread
    /// </summary>
    public static ERROR_CODE LastInitStatus = ERROR_CODE.ERROR_CODE_LAST; //Result State

    public static ERROR_CODE PreviousInitStatus = ERROR_CODE.ERROR_CODE_LAST;


    /// <summary>
    ///     ZED Manager Instance
    /// </summary>
    // Static singleton instance
    private static ZEDManager instance;

    [ReadOnly("Camera FPS")] [HideInInspector]
    public string cameraFPS = "-";


    ///////////////////////////////////////////
    //////  camera and player Transforms //////
    ///////////////////////////////////////////
    /// <summary>
    ///     Contains the transform of the camera left
    /// </summary>
    private Transform cameraLeft;

    /// <summary>
    ///     Contains the transform of the right camera if enabled
    /// </summary>
    private Transform cameraRight;


    /////////////////////////////////////
    //////  Timestamps             //////
    /////////////////////////////////////

    /// <summary>
    ///     Depth mode
    /// </summary>
    public DEPTH_MODE depthMode = DEPTH_MODE.PERFORMANCE;


    [Header("Rendering")]
    /// <summary>
    /// Activate/Deactivate depth comparison between Real and Virtual (depth occlusions)
    /// </summary>
    [Tooltip("Activate/Deactivate depth Real/Virtual comparison and occlusions")]
    public bool depthOcclusion = true;

    /// <summary>
    ///     Activate/Deactivate depth stabilizer
    /// </summary>
    private readonly bool depthStabilizer = true;

    /// <summary>
    ///     Enables the pose smoothing during drift correction (for MR experience, it is advised to leave it at true when
    ///     spatial memory is activated)
    /// </summary>
    private bool enablePoseSmoothing;

    /// <summary>
    ///     Enables the spatial memory. Will detect and correct tracking drift by remembering features and anchors in the
    ///     environment,
    ///     but may cause visible jumps when it happens.
    /// </summary>
    [Tooltip(
        "Will detect and correct tracking drift by remembering features and anchors in the environment, but may cause visible jumps when it happens")]
    public bool enableSpatialMemory = true;


    [Header("Motion Tracking")]
    /// <summary>
    /// Enables the tracking, if true, the tracking computed will be set to the gameObject.
    /// If false, the camera tracking will be done by HMD if connected and available
    /// </summary>
    public bool enableTracking = true;

    [ReadOnly("Engine FPS")] [HideInInspector]
    public string engineFPS = "-";

    /// <summary>
    ///     Targeted FPS
    /// </summary>
    private int FPS = 60;

    /// <summary>
    ///     The engine FPS.
    /// </summary>
    private float fps_engine = 90.0f;

    public object grabLock = new object(); // lock


    ///////////////////////////////////////////////////
    [HideInInspector] public Quaternion gravityRotation = Quaternion.identity;

    [ReadOnly("HMD Device")] [HideInInspector]
    public string HMDDevice = "-";

    [HideInInspector] public Vector3 HMDSyncPosition;
    [HideInInspector] public Quaternion HMDSyncRotation;

    /// <summary>
    ///     Starting Position (not used in Stereo AR)
    /// </summary>
    private Vector3 initialPosition;

    /// <summary>
    ///     Starting Orientation (not used in Stereo AR)
    /// </summary>
    private readonly Quaternion initialRotation = Quaternion.identity;


    ////////////////////////////
    //////// Private ///////////
    ////////////////////////////
    /// <summary>
    ///     Init parameters to the ZED (parameters of open())
    /// </summary>
    private InitParameters initParameters;

    /// <summary>
    ///     Checks if the camera tracked in any way (hmd, zed, ...)
    /// </summary>
    private bool isCameraTracked;

    private bool isDisconnected;

    /// <summary>
    ///     Checks if the tracking has been activated
    /// </summary>
    private bool isTrackingEnable;

    /// <summary>
    ///     Is camera moving
    /// </summary>
    private bool isZEDTracked;

    private readonly int layerLeftFinalScreen = 9;


    /////////////////////////////////////
    //////  Layers for ZED         //////
    /////////////////////////////////////
    private readonly int layerLeftScreen = 8;
    private readonly int layerRightFinalScreen = 11;
    private readonly int layerRightScreen = 10;

    /// <summary>
    ///     Camera or Image brightness
    /// </summary>
    [Range(0, 1)] public float m_cameraBrightness = 1.0f;

    private bool newFrameAvailable;
    private bool openingLaunched; // Init State

    /// <summary>
    ///     First position registered after enabling the tracking
    /// </summary>
    private Vector3 originPosition;

    /// <summary>
    ///     First rotation registered after enabling the tracking
    /// </summary>
    private Quaternion originRotation;

    /// <summary>
    ///     Area file path
    /// </summary>
    public string pathSpatialMemory = "ZED_spatial_memory";

    /// <summary>
    ///     Position offset used to retrieve the tracking with an offset of position
    /// </summary>
    private Vector3 positionOffset;

    /// <summary>
    ///     AR post processing: Real and Virtual blending
    /// </summary>
    [LabelOverride("AR Post-processing", "Adjust virtual objects rendering for a better fit on the real scene")]
    public bool postProcessing = true;


    private bool requestNewFrame;

    [Header("Camera")]
    /// <summary>
    /// Selected resolution
    /// </summary>
    public RESOLUTION resolution = RESOLUTION.HD720;

    /// <summary>
    ///     Rotation offset used to retrieve the tracking with an offset of rotation
    /// </summary>
    private Quaternion rotationOffset;

    private bool running; //state

    /// <summary>
    ///     Runtime parameters to the ZED (parameters of grab())
    /// </summary>
    private RuntimeParameters runtimeParameters;

    /// <summary>
    ///     Sensing mode
    ///     Always use the sensing mode FILL, since we need a depth without holes
    /// </summary>
    private readonly SENSING_MODE sensingMode = SENSING_MODE.FILL;


    /// <summary>
    ///     Image acquisition thread
    /// </summary>
    private Thread threadGrab; //Thread

    private Thread threadOpening; //Thread


    /// <summary>
    ///     Thread to init the tracking  (the tracking takes some time to Init)
    /// </summary>
    private Thread trackerThread;

    [ReadOnly("Tracking State")] [HideInInspector]
    public string trackingState = "-";


    [Header("Status")] [ReadOnly("Version")] [HideInInspector]
    public string versionZED = "-";

    // Set to true to activate dll wrapper verbose file (C:/ProgramData/STEREOLABS/SL_Unity_wrapper.txt)
    // Default: false
    // Warning: this can decrease performance. Use only for debugging. 
    private readonly bool wrapperVerbose = false;

    ////////////////////////////
    //////// Public ///////////
    ////////////////////////////
    /// <summary>
    ///     Current instance of the ZED Camera
    /// </summary>
    public ZEDCamera zedCamera;


    /// <summary>
    ///     Orientation returned by the tracker
    /// </summary>
    private Quaternion zedOrientation = Quaternion.identity;

    /// <summary>
    ///     Position returned by the tracker
    /// </summary>
    private Vector3 zedPosition;

    /// <summary>
    ///     Checks if the thread init is over
    /// </summary>
    private bool zedReady;

    /// <summary>
    ///     Contains the position of the player's head, different from ZED's position
    ///     But the position of the ZED regarding this transform does not change during use (rigid transform)
    ///     In ZED_Rig_Mono, this will be the root ZED_Rig_Mono object. In ZED_Rig_Stereo, this is Camera_eyes.
    /// </summary>
    private Transform zedRigRoot;

    /// <summary>
    ///     Manages the read and write of SVO
    /// </summary>
    private ZEDSVOManager zedSVOManager;

    [HideInInspector] public Vector3 ZEDSyncPosition;
    [HideInInspector] public Quaternion ZEDSyncRotation;

    /// <summary>
    ///     Tracking state used by anti-drift
    /// </summary>
    private TRACKING_STATE zedtrackingState = TRACKING_STATE.TRACKING_OFF;

    public float CameraBrightness
    {
        get { return m_cameraBrightness; }
        set
        {
            if (m_cameraBrightness == value) return;
            m_cameraBrightness = value;
            if (OnCamBrightnessChange != null)
                OnCamBrightnessChange(m_cameraBrightness);
        }
    }

    public bool IsCameraTracked => isCameraTracked;

    public static bool IsStereoRig => isStereoRig;

    public bool IsZEDReady => zedReady;

    public TRACKING_STATE ZEDTrackingState => zedtrackingState;

    public Vector3 OriginPosition => originPosition;

    public Quaternion OriginRotation => originRotation;

    public ulong CameraTimeStamp { get; private set; }

    public ulong ImageTimeStamp { get; private set; }

    // Static singleton property
    public static ZEDManager Instance =>
        instance ?? (instance = new GameObject("ZEDManager").AddComponent<ZEDManager>());

    public event onCamBrightnessChangeDelegate OnCamBrightnessChange;


    /// <summary>
    ///     Get the center transform, the only one moved by the tracker on AR
    /// </summary>
    /// <returns></returns>
    public Transform GetZedRootTansform()
    {
        return zedRigRoot;
    }

    /// <summary>
    ///     Get the left camera. It's best to use this one as it's available in all configurations
    /// </summary>
    /// <returns></returns>
    public Transform GetLeftCameraTransform()
    {
        return cameraLeft;
    }

    /// <summary>
    ///     Get the right camera, only available on AR
    /// </summary>
    /// <returns></returns>
    public Transform GetRightCameraTransform()
    {
        return cameraRight;
    }

    public static event OnZEDManagerReady OnZEDReady;
    public static event OnZEDManagerDisconnected OnZEDDisconnected;


    #region CHECK_AR

    /// <summary>
    ///     Check if there are two cameras, one for each eye as children
    /// </summary>
    private void CheckStereoMode()
    {
        zedRigRoot = gameObject.transform;

        var devicePresent = XRDevice.isPresent;
        if (gameObject.transform.childCount > 0 &&
            gameObject.transform.GetChild(0).gameObject.name.Contains("Camera_eyes"))
        {
            var cams = gameObject.transform.GetChild(0).GetComponentsInChildren(typeof(Camera));
            foreach (Camera cam in cams)
                if (cam.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    cameraLeft = cam.transform;
                    SetLayerRecursively(cameraLeft.gameObject, layerLeftScreen);

                    cam.cullingMask &= ~(1 << layerRightScreen);
                    cam.cullingMask &= ~(1 << layerRightFinalScreen);
                    cam.cullingMask &= ~(1 << layerLeftFinalScreen);
                    cam.cullingMask &= ~(1 << ZEDCamera.TagOneObject);
                }
                else if (cam.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    cameraRight = cam.transform;
                    SetLayerRecursively(cameraRight.gameObject, layerRightScreen);
                    cam.cullingMask &= ~(1 << layerLeftScreen);
                    cam.cullingMask &= ~(1 << layerLeftFinalScreen);
                    cam.cullingMask &= ~(1 << layerRightFinalScreen);
                    cam.cullingMask &= ~(1 << ZEDCamera.TagOneObject);
                }
        }
        else
        {
            var cams = gameObject.transform.GetComponentsInChildren(typeof(Camera));
            foreach (Camera cam in cams)
                if (cam.stereoTargetEye == StereoTargetEyeMask.None)
                {
                    cameraLeft = cam.transform;
                    cam.cullingMask = -1;
                    cam.cullingMask &= ~(1 << ZEDCamera.TagOneObject);
                }
        }


        if (cameraLeft && cameraRight)
        {
            isStereoRig = true;
            if (cameraLeft.transform.parent != null)
                zedRigRoot = cameraLeft.transform.parent;
        }
        else
        {
            isStereoRig = false;
            var temp = cameraLeft.gameObject.GetComponent<Camera>();

            if (cameraLeft.transform.parent != null)
                zedRigRoot = cameraLeft.transform.parent;

            foreach (var c in Camera.allCameras)
                if (c != temp)
                {
                    c.cullingMask &= ~(1 << layerLeftScreen);
                    c.cullingMask &= ~(1 << ZEDCamera.Tag);
                }

            if (cameraLeft.gameObject.transform.childCount > 0)
                cameraLeft.transform.GetChild(0).gameObject.layer = layerLeftScreen;
        }
    }

    #endregion


    /// <summary>
    ///     Set the layer number to the game object layer
    /// </summary>
    public static void SetLayerRecursively(GameObject go, int layerNumber)
    {
        if (go == null) return;
        foreach (var trans in go.GetComponentsInChildren<Transform>(true)) trans.gameObject.layer = layerNumber;
    }


    /// <summary>
    ///     Stops the current thread
    /// </summary>
    public void Destroy()
    {
        running = false;

        // In case the opening thread is still running
        if (threadOpening != null)
        {
            threadOpening.Join();
            threadOpening = null;
        }

        // Shutdown grabbing thread
        if (threadGrab != null)
        {
            threadGrab.Join();
            threadGrab = null;
        }

        Thread.Sleep(10);
    }

    /// <summary>
    ///     Raises the application quit event
    /// </summary>
    private void OnApplicationQuit()
    {
        zedReady = false;
        OnCamBrightnessChange -= CameraBrightnessChangeHandler;
        Destroy();

        if (zedCamera != null)
        {
            if (zedSVOManager != null)
                if (zedSVOManager.record)
                    zedCamera.DisableRecording();
            zedCamera.Destroy();
            zedCamera = null;
        }
    }


    private void Awake()
    {
        instance = this;
        zedReady = false;
        //If you want the ZEDRig not to be destroyed
        DontDestroyOnLoad(transform.root);

        //Init the first parameters
        initParameters = new InitParameters();
        initParameters.resolution = resolution;
        initParameters.depthMode = depthMode;
        initParameters.depthStabilization = depthStabilizer;

        //Check if the AR is needed and if possible to add
        CheckStereoMode();

        //Init the other options
        isZEDTracked = enableTracking;
        initialPosition = zedRigRoot.transform.localPosition;
        zedPosition = initialPosition;
        zedOrientation = initialRotation;


        //Create a camera and return an error message if the dependencies are not detected
        zedCamera = ZEDCamera.GetInstance();
        LastInitStatus = ERROR_CODE.ERROR_CODE_LAST;

        zedSVOManager = GetComponent<ZEDSVOManager>();
        zedCamera.CreateCamera(wrapperVerbose);

        if (zedSVOManager != null)
        {
            //Create a camera
            if ((zedSVOManager.read || zedSVOManager.record) && zedSVOManager.videoFile.Length == 0)
            {
                zedSVOManager.record = false;
                zedSVOManager.read = false;
            }

            if (zedSVOManager.read)
            {
                zedSVOManager.record = false;
                initParameters.pathSVO = zedSVOManager.videoFile;
                initParameters.svoRealTimeMode = zedSVOManager.realtimePlayback;
                initParameters.depthStabilization = depthStabilizer;
            }
        }

        versionZED = "[SDK]: " + ZEDCamera.GetSDKVersion() + " [Plugin]: " + ZEDCamera.PluginVersion;


        //Set the ZED Tracking frame as Left eye
        if (isStereoRig)
        {
            //Creates a CameraRig (the 2 last cameras)
            var o = CreateZEDRigDisplayer();
            o.hideFlags = HideFlags.HideAndDontSave;
            o.transform.parent = transform;

            //Force some initParameters that are required for MR experience
            initParameters.enableRightSideMeasure = isStereoRig;
            initParameters.depthMinimumDistance = 0.1f;
            initParameters.depthMode = DEPTH_MODE.PERFORMANCE;
            initParameters.depthStabilization = depthStabilizer;

            //Create the mirror, the texture from the firsts cameras is rendered to avoid a black border
            CreateMirror();
        }

        //Start the co routine to initialize the ZED and avoid to block the user
        LastInitStatus = ERROR_CODE.ERROR_CODE_LAST;
        openingLaunched = false;
        StartCoroutine("InitZED");

        OnCamBrightnessChange += CameraBrightnessChangeHandler;
    }


    /// <summary>
    ///     Init the SVO, and launch the thread to enable the tracking
    /// </summary>
    private void ZEDReady()
    {
        FPS = (int) zedCamera.GetRequestedCameraFPS();
        if (enableTracking)
        {
            trackerThread = new Thread(EnableTrackingThreaded);
            trackerThread.Start();
        }

        if (zedSVOManager != null)
        {
            if (zedSVOManager.record)
                if (zedCamera.EnableRecording(zedSVOManager.videoFile, zedSVOManager.compressionMode) !=
                    ERROR_CODE.SUCCESS)
                    zedSVOManager.record = false;

            if (zedSVOManager.read) zedSVOManager.NumberFrameMax = zedCamera.GetSVONumberOfFrames();
        }


        if (enableTracking)
            trackerThread.Join();

        if (isStereoRig && XRDevice.isPresent)
        {
            var pose = ar.InitTrackingAR();
            originPosition = pose.translation;
            originRotation = pose.rotation;
            zedRigRoot.localPosition = originPosition;
            zedRigRoot.localRotation = originRotation;

            if (!zedCamera.IsHmdCompatible && zedCamera.IsCameraReady)
                Debug.LogWarning(
                    "WARNING : AR Passtrough with a ZED is not recommended. You may consider using the ZED-M, designed for that purpose");
        }
        else
        {
            originPosition = initialPosition;
            originRotation = initialRotation;
        }

#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged = HandleOnPlayModeChanged;
#endif
    }

    /// <summary>
    ///     Enables the thread to get the trackingr.up
    /// </summary>
    private void EnableTrackingThreaded()
    {
        enablePoseSmoothing = enableSpatialMemory;
        lock (grabLock)
        {
            //Make sure we have "grabbed" on frame first
            var e = zedCamera.Grab(ref runtimeParameters);
            var timeOut_grab = 0;
            while (e != ERROR_CODE.SUCCESS && timeOut_grab < 100)
            {
                e = zedCamera.Grab(ref runtimeParameters);
                Thread.Sleep(10);
                timeOut_grab++;
            }

            //Make sure the .area path is valid
            if (pathSpatialMemory != "" && !File.Exists(pathSpatialMemory))
            {
                Debug.Log("Specified path to .area file '" + pathSpatialMemory + "' does not exist. Ignoring.");
                pathSpatialMemory = "";
            }

            //Now enable the tracking with the proper parameters
            //if (!(enableTracking = (zedCamera.EnableTracking(ref initialRotation, ref initialPosition, enableSpatialMemory, enablePoseSmoothing, pathSpatialMemory) == sl.ERROR_CODE.SUCCESS)))
            if (!(enableTracking = zedCamera.EnableTracking(ref zedOrientation, ref zedPosition, enableSpatialMemory,
                                       enablePoseSmoothing, pathSpatialMemory) == ERROR_CODE.SUCCESS))
                throw new Exception(ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.TRACKING_NOT_INITIALIZED));
            isTrackingEnable = true;
        }
    }

#if UNITY_EDITOR
    private void HandleOnPlayModeChanged()
    {
        if (zedCamera == null) return;

#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged = HandleOnPlayModeChanged;
#endif
    }
#endif
    /// <summary>
    ///     Event called when camera is disconnected
    /// </summary>
    private void ZEDDisconnected()
    {
        cameraFPS = "Disconnected";

        isDisconnected = true;

        if (zedReady) Reset();
    }

    private void OnDestroy()
    {
        OnApplicationQuit();
    }

    /// <summary>
    ///     Closes out the current stream, then starts it up again.
    ///     Used when the zed becomes unplugged, or you want to change a setting at runtime that
    ///     requires re-initializing the camera.
    /// </summary>
    public void Reset()
    {
        //Save tracking
        if (enableTracking && isTrackingEnable) zedCamera.GetPosition(ref zedOrientation, ref zedPosition);

        OnApplicationQuit();

        openingLaunched = false;
        running = false;

        Awake();
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (zedCamera != null)
        {
            if (!isTrackingEnable && enableTracking)
            {
                //Enables the tracking and initializes the first position of the camera
                enablePoseSmoothing = enableSpatialMemory;
                if (!(enableTracking = zedCamera.EnableTracking(ref zedOrientation, ref zedPosition,
                                           enableSpatialMemory, enablePoseSmoothing, pathSpatialMemory) ==
                                       ERROR_CODE.SUCCESS))
                {
                    isZEDTracked = false;
                    throw new Exception(ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.TRACKING_NOT_INITIALIZED));
                }

                isZEDTracked = true;
                isTrackingEnable = true;
            }


            if (isTrackingEnable && !enableTracking)
            {
                isZEDTracked = false;
                lock (grabLock)
                {
                    zedCamera.DisableTracking();
                }

                isTrackingEnable = false;
            }


            setRenderingSettings();
        }
    }
#endif


    #region INITIALIZATION

    /// <summary>
    ///     ZED opening function (should be called in the thread)
    /// </summary>
    private void OpenZEDInBackground()
    {
        openingLaunched = true;
        LastInitStatus = zedCamera.Init(ref initParameters);
        openingLaunched = false;
    }


    /// <summary>
    ///     Initialization routine
    /// </summary>
    private uint numberTriesOpening;

    /// Counter of tries to open the ZED
    private const int MAX_OPENING_TRIES = 50;

    private IEnumerator InitZED()
    {
        zedReady = false;
        while (LastInitStatus != ERROR_CODE.SUCCESS)
        {
            //Initialize the camera
            if (!openingLaunched)
            {
                threadOpening = new Thread(OpenZEDInBackground);

                if (LastInitStatus != ERROR_CODE.SUCCESS)
                {
#if UNITY_EDITOR
                    numberTriesOpening++;
                    if (numberTriesOpening % 2 == 0 && LastInitStatus == PreviousInitStatus)
                        Debug.LogWarning("[ZEDPlugin]: " + LastInitStatus);

                    if (numberTriesOpening > MAX_OPENING_TRIES)
                    {
                        Debug.Log("[ZEDPlugin]: Stops initialization");
                        yield break;
                    }
#endif


                    PreviousInitStatus = LastInitStatus;
                }


                threadOpening.Start();
            }

            yield return new WaitForSeconds(0.3f);
        }


        //ZED has opened
        if (LastInitStatus == ERROR_CODE.SUCCESS)
        {
            threadOpening.Join();

            //Initialize the threading mode, the positions with the AR and the SVO if needed
            //Launch the threading to enable the tracking
            ZEDReady();

            //Wait until the ZED of the init of the tracking
            while (enableTracking && !isTrackingEnable) yield return new WaitForSeconds(0.5f);

            //Calls all the observers, the ZED is ready :)
            if (OnZEDReady != null) OnZEDReady();

            var ratio = Screen.width / (float) Screen.height;
            var target = 16.0f / 9.0f;
            if (Mathf.Abs(ratio - target) > 0.01) ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.SCREEN_RESOLUTION);


            //If not already launched, launch the grabbing thread
            if (!running)
            {
                running = true;
                requestNewFrame = true;

                threadGrab = new Thread(ThreadedZEDGrab);
                threadGrab.Start();
            }

            zedReady = true;
            isDisconnected = false; //In case we just regained connection

            setRenderingSettings();
            AdjustZEDRigCameraPosition();
        }
    }


    /// <summary>
    ///     Adjust camera(s) and render plane position regarding zedRigRoot (player) transform
    ///     The ZED Rig will then only be moved using zedRigRoot transform (each camera will keep its local position regarding
    ///     zedRigRoot)
    /// </summary>
    private void AdjustZEDRigCameraPosition()
    {
        var rightCameraOffset = new Vector3(zedCamera.Baseline, 0.0f, 0.0f);
        if (isStereoRig && XRDevice.isPresent)
        {
            // zedRigRoot transform (origin of the global camera) is placed on the HMD headset. Therefore we move the camera in front of it ( offsetHmdZEDPosition)
            // as when the camera is mount on the HMD. Values are provided by default. This can be done with a calibration as well
            // to know the exact position of the HMD regarding the camera
            cameraLeft.localPosition = ar.HmdToZEDCalibration.translation;
            cameraLeft.localRotation = ar.HmdToZEDCalibration.rotation;
            if (cameraRight)
                cameraRight.localPosition = cameraLeft.localPosition + new Vector3(zedCamera.Baseline, 0.0f, 0.0f);
            if (cameraRight) cameraRight.localRotation = cameraLeft.localRotation;
        }
        else if (isStereoRig && !XRDevice.isPresent)
        {
            // When no Hmd is available, simply put the origin at the left camera.
            cameraLeft.localPosition = Vector3.zero;
            cameraLeft.localRotation = Quaternion.identity;
            if (cameraRight) cameraRight.localPosition = rightCameraOffset;
            if (cameraRight) cameraRight.localRotation = Quaternion.identity;
        }
        else
        {
            cameraLeft.localPosition = Vector3.zero;
            cameraLeft.localRotation = Quaternion.identity;
        }
    }

    public void setRenderingSettingsPublic()
    {
        setRenderingSettings();
    }


    /// <summary>
    ///     Set the rendering settings (rendering path, shaders values) for camera Left and right
    ///     Activate/Deactivate depth occlusions, Change rendering path...
    /// </summary>
    private void setRenderingSettings()
    {
        var textureLeftOverlay = GetLeftCameraTransform().GetComponent<ZEDRenderingPlane>();
        textureLeftOverlay.SetPostProcess(postProcessing);
        GetLeftCameraTransform().GetComponent<Camera>().renderingPath = RenderingPath.UsePlayerSettings;
        Shader.SetGlobalFloat("_ZEDFactorAffectReal", m_cameraBrightness);

        ZEDRenderingPlane textureRightOverlay = null;

        if (IsStereoRig)
        {
            textureRightOverlay = GetRightCameraTransform().GetComponent<ZEDRenderingPlane>();
            textureRightOverlay.SetPostProcess(postProcessing);
        }

        var renderingPath = (ZEDRenderingMode) GetLeftCameraTransform().GetComponent<Camera>().actualRenderingPath;

        //Check that we are in forward or deffered
        if (renderingPath != ZEDRenderingMode.FORWARD && renderingPath != ZEDRenderingMode.DEFERRED)
        {
            Debug.LogError("[ZED Plugin] Only Forward and Deferred Shading rendering path are supported");
            GetLeftCameraTransform().GetComponent<Camera>().renderingPath = RenderingPath.Forward;
            if (IsStereoRig)
                GetRightCameraTransform().GetComponent<Camera>().renderingPath = RenderingPath.Forward;
        }

        //Set Depth Occ 
        if (renderingPath == ZEDRenderingMode.FORWARD)
        {
            textureLeftOverlay.ManageKeyWordPipe(!depthOcclusion, "NO_DEPTH_OCC");
            if (textureRightOverlay)
                textureRightOverlay.ManageKeyWordPipe(!depthOcclusion, "NO_DEPTH_OCC");
        }
        else if (renderingPath == ZEDRenderingMode.DEFERRED)
        {
            textureLeftOverlay.ManageKeyWordDefferedMat(!depthOcclusion, "NO_DEPTH_OCC");
            if (textureRightOverlay)
                textureRightOverlay.ManageKeyWordDefferedMat(!depthOcclusion, "NO_DEPTH_OCC");
        }
    }

    #endregion


    #region IMAGE_ACQUIZ

    private void ThreadedZEDGrab()
    {
        runtimeParameters = new RuntimeParameters();
        runtimeParameters.sensingMode = sensingMode;
        runtimeParameters.enableDepth = true;
        // Don't change this ReferenceFrame. If we need normals in world frame, then we will do the convertion ourselves.
        runtimeParameters.measure3DReferenceFrame = REFERENCE_FRAME.CAMERA;

        while (running)
        {
            if (zedCamera == null)
                return;

            AcquireImages();
        }
    }

    private void AcquireImages()
    {
        if (requestNewFrame && zedReady)
        {
            var e = ERROR_CODE.NOT_A_NEW_FRAME;

            // Live or SVO ? if SVO is in pause, don't need to call grab again since image will not change
            if (zedSVOManager == null)
            {
                e = zedCamera.Grab(ref runtimeParameters);
            }
            else
            {
                if (!zedSVOManager.pause)
                {
                    e = zedCamera.Grab(ref runtimeParameters);
                }
                else
                {
                    if (zedSVOManager.NeedNewFrameGrab)
                    {
                        e = zedCamera.Grab(ref runtimeParameters);
                        zedSVOManager.NeedNewFrameGrab = false;
                    }
                    else
                    {
                        e = ERROR_CODE.SUCCESS;
                    }
                }
            }


            lock (grabLock)
            {
                if (e == ERROR_CODE.CAMERA_NOT_DETECTED)
                {
                    Debug.Log("Camera not detected or disconnected.");
                    isDisconnected = true;
                    Thread.Sleep(10);
                    requestNewFrame = false;
                }
                else if (e == ERROR_CODE.SUCCESS)
                {
                    //Save the timestamp
                    CameraTimeStamp = zedCamera.GetCameraTimeStamp();

#if UNITY_EDITOR
                    var camera_fps = zedCamera.GetCameraFPS();
                    cameraFPS = camera_fps + "Fps";

                    if (camera_fps <= FPS * 0.8)
                        cameraFPS += " WARNING: Low USB bandwidth detected";
#endif

                    //Get position of camera
                    if (isTrackingEnable)
                        zedtrackingState =
                            zedCamera.GetPosition(ref zedOrientation, ref zedPosition, TRACKING_FRAME.LEFT_EYE);
                    else
                        zedtrackingState = TRACKING_STATE.TRACKING_OFF;


                    // Indicate that a new frame is available and pause the thread until a new request is called
                    newFrameAvailable = true;
                    requestNewFrame = false;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
        else
        {
            //to avoid "overheat"
            Thread.Sleep(1);
        }
    }

    #endregion


    #region ENGINE_UPDATE

    /// <summary>
    ///     If a new frame is available, this function retrieve the Images and update the texture at each engine tick
    ///     Called in Update()
    /// </summary>
    public void UpdateImages()
    {
        if (zedCamera == null)
            return;

        if (newFrameAvailable)
        {
            lock (grabLock)
            {
                zedCamera.RetrieveTextures();
                zedCamera.UpdateTextures();
                ImageTimeStamp = zedCamera.GetImagesTimeStamp();
            }

            requestNewFrame = true;
            newFrameAvailable = false;
        }

        #region SVO Manager

        if (zedSVOManager != null)
        {
            if (!zedSVOManager.pause)
            {
                if (zedSVOManager.record)
                {
                    zedCamera.Record();
                }
                else if (zedSVOManager.read)
                {
                    zedSVOManager.CurrentFrame = zedCamera.GetSVOPosition();
                    if (zedSVOManager.loop && zedSVOManager.CurrentFrame >= zedCamera.GetSVONumberOfFrames() - 1)
                    {
                        zedCamera.SetSVOPosition(0);

                        if (enableTracking)
                        {
                            if (!(enableTracking = zedCamera.ResetTracking(initialRotation, initialPosition) ==
                                                   ERROR_CODE.SUCCESS))
                                throw new Exception("Error, tracking not available");

                            zedRigRoot.localPosition = initialPosition;
                            zedRigRoot.localRotation = initialRotation;
                        }
                    }
                }
            }
            else if (zedSVOManager.read)
            {
                zedCamera.SetSVOPosition(zedSVOManager
                    .CurrentFrame); //As this wasn't updated, it's effectively the last frame. 
            }
        }

        #endregion
    }


    /// <summary>
    ///     Get the tracking position from the ZED and update the manager's position. If enable, update the AR Tracking
    ///     Only called in LIVE mode
    ///     Called in Update()
    /// </summary>
    private void UpdateTracking()
    {
        if (!zedReady)
            return;

        if (isZEDTracked)
        {
            Quaternion r;
            Vector3 v;

            isCameraTracked = true;

            if (XRDevice.isPresent && isStereoRig)
            {
                if (calibrationHasChanged)
                {
                    AdjustZEDRigCameraPosition();
                    calibrationHasChanged = false;
                }

                ar.ExtractLatencyPose(ImageTimeStamp);
                ar.AdjustTrackingAR(zedPosition, zedOrientation, out r, out v);
                zedRigRoot.localRotation = r;
                zedRigRoot.localPosition = v;

                ZEDSyncPosition = v;
                ZEDSyncRotation = r;
                HMDSyncPosition = ar.LatencyPose().translation;
                HMDSyncRotation = ar.LatencyPose().rotation;
            }
            else
            {
                zedRigRoot.localRotation = zedOrientation;
                zedRigRoot.localPosition = zedPosition;
            }
        }
        else if (XRDevice.isPresent && isStereoRig)
        {
            isCameraTracked = true;
            ar.ExtractLatencyPose(ImageTimeStamp);
            zedRigRoot.localRotation = ar.LatencyPose().rotation;
            zedRigRoot.localPosition = ar.LatencyPose().translation;
        }
        else
        {
            isCameraTracked = false;
        }
    }

    /// <summary>
    ///     Updates the collection of hmd pose (AR only)
    /// </summary>
    private void updateHmdPose()
    {
        if (IsStereoRig && XRDevice.isPresent)
            ar.CollectPose();
    }

    /// <summary>
    ///     Update this instance. Called at each frame
    /// </summary>
    private void Update()
    {
        // Update Image first, then collect HMD pose at the image time.
        // Then update the tracking
        UpdateImages();
        updateHmdPose();
        UpdateTracking();

        //If the ZED is disconnected, to easily look at the message
        if (isDisconnected)
        {
            if (OnZEDDisconnected != null)
                OnZEDDisconnected();

            ZEDDisconnected();
        }

#if UNITY_EDITOR
        if (zedCamera != null)
        {
            var frame_drop_count = zedCamera.GetFrameDroppedPercent();
            var CurrentTickFPS = 1.0f / Time.deltaTime;
            fps_engine = (fps_engine + CurrentTickFPS) / 2.0f;
            engineFPS = fps_engine.ToString("F1") + " FPS";
            if (frame_drop_count > 30 && fps_engine < 45)
                engineFPS += "WARNING : engine low framerate detected";

            if (isZEDTracked)
                trackingState = ZEDTrackingState.ToString();
            else if (XRDevice.isPresent && isStereoRig)
                trackingState = "HMD Tracking";
            else
                trackingState = "Camera Not Tracked";
        }
#endif
    }

    public void LateUpdate()
    {
        if (IsStereoRig && XRDevice.isPresent) ar.LateUpdateHmdRendering();
    }

    #endregion


    #region AR_CAMERAS

    private GameObject zedRigDisplayer;
    private ZEDMixedRealityPlugin ar;

    /// <summary>
    ///     Create a GameObject to display the ZED in an headset (ZED-M Only)
    /// </summary>
    /// <returns></returns>
    private GameObject CreateZEDRigDisplayer()
    {
        //Make sure we don't already have one, such as if the camera disconnected and reconnected. 
        if (zedRigDisplayer != null) return zedRigDisplayer;

        zedRigDisplayer = new GameObject("ZEDRigDisplayer");
        ar = zedRigDisplayer.AddComponent<ZEDMixedRealityPlugin>();


        /*Screens : Left and right */
        var leftScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var meshLeftScreen = leftScreen.GetComponent<MeshRenderer>();
        meshLeftScreen.lightProbeUsage = LightProbeUsage.Off;
        meshLeftScreen.reflectionProbeUsage = ReflectionProbeUsage.Off;
        meshLeftScreen.receiveShadows = false;
        meshLeftScreen.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        meshLeftScreen.shadowCastingMode = ShadowCastingMode.Off;
        meshLeftScreen.sharedMaterial = Resources.Load("Materials/Unlit/Mat_ZED_Unlit") as Material;
        leftScreen.layer = layerLeftFinalScreen;
        Destroy(leftScreen.GetComponent<MeshCollider>());

        var rightScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var meshRightScreen = rightScreen.GetComponent<MeshRenderer>();
        meshRightScreen.lightProbeUsage = LightProbeUsage.Off;
        meshRightScreen.reflectionProbeUsage = ReflectionProbeUsage.Off;
        meshRightScreen.receiveShadows = false;
        meshRightScreen.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        meshRightScreen.shadowCastingMode = ShadowCastingMode.Off;
        Destroy(rightScreen.GetComponent<MeshCollider>());
        meshRightScreen.sharedMaterial = Resources.Load("Materials/Unlit/Mat_ZED_Unlit") as Material;
        rightScreen.layer = layerRightFinalScreen;

        /*Camera left and right*/
        var camLeft = new GameObject("cameraLeft");
        camLeft.transform.SetParent(zedRigDisplayer.transform);
        var camL = camLeft.AddComponent<Camera>();
        camL.stereoTargetEye =
            StereoTargetEyeMask.Both; //Temporary setting to fix loading screen issue - will be set to Left once ready
        camL.renderingPath = RenderingPath.Forward; //Minimal overhead
        camL.clearFlags = CameraClearFlags.Color;
        camL.backgroundColor = Color.black;
        camL.cullingMask = 1 << layerLeftFinalScreen;
        camL.allowHDR = false;
        camL.allowMSAA = false;

        var camRight = new GameObject("cameraRight");
        camRight.transform.SetParent(zedRigDisplayer.transform);
        var camR = camRight.AddComponent<Camera>();
        camR.renderingPath = RenderingPath.Forward; //Minimal overhead
        camR.clearFlags = CameraClearFlags.Color;
        camR.backgroundColor = Color.black;
        camR.stereoTargetEye =
            StereoTargetEyeMask.Both; //Temporary setting to fix loading screen issue - will be set to Left once ready
        camR.cullingMask = 1 << layerRightFinalScreen;
        camR.allowHDR = false;
        camR.allowMSAA = false;

        SetLayerRecursively(camRight, layerRightFinalScreen);
        SetLayerRecursively(camLeft, layerLeftFinalScreen);

        //Hide camera in editor
#if UNITY_EDITOR
        LayerMask
            layerNumberBinary = 1 << layerRightFinalScreen; // This turns the layer number into the right binary number
        layerNumberBinary |= 1 << layerLeftFinalScreen;
        LayerMask flippedVisibleLayers = ~Tools.visibleLayers;
        Tools.visibleLayers = ~(flippedVisibleLayers | layerNumberBinary);
#endif
        leftScreen.transform.SetParent(zedRigDisplayer.transform);
        rightScreen.transform.SetParent(zedRigDisplayer.transform);


        ar.finalCameraLeft = camLeft;
        ar.finalCameraRight = camRight;
        ar.ZEDEyeLeft = cameraLeft.gameObject;
        ar.ZEDEyeRight = cameraRight.gameObject;
        ar.quadLeft = leftScreen.transform;
        ar.quadRight = rightScreen.transform;


        ZEDMixedRealityPlugin.OnHdmCalibChanged += CalibrationHasChanged;
        if (XRDevice.isPresent)
            HMDDevice = XRDevice.model;

        return zedRigDisplayer;
    }

    #endregion

    #region MIRROR

    private ZEDMirror mirror;
    private GameObject mirrorContainer;

    private void CreateMirror()
    {
        GameObject camLeft;
        Camera camL;
        if (mirrorContainer == null)
        {
            mirrorContainer = new GameObject("Mirror");
            mirrorContainer.hideFlags = HideFlags.HideAndDontSave;

            camLeft = new GameObject("MirrorCamera");
            camLeft.hideFlags = HideFlags.HideAndDontSave;
            mirror = camLeft.AddComponent<ZEDMirror>();
            mirror.manager = this;
            camL = camLeft.AddComponent<Camera>();
        }
        else
        {
            camLeft = mirror.gameObject;
            camL = camLeft.GetComponent<Camera>();
        }

        camLeft.transform.parent = mirrorContainer.transform;
        camL.gameObject.layer = 8;
        camL.stereoTargetEye = StereoTargetEyeMask.None;
        camL.renderingPath = RenderingPath.Forward; //Minimal overhead
        camL.clearFlags = CameraClearFlags.Color;
        camL.backgroundColor = Color.black;
        camL.cullingMask = 0;
        camL.allowHDR = false;
        camL.allowMSAA = false;
        camL.useOcclusionCulling = false;
    }

    #endregion


    #region EventHandler

    /// <summary>
    ///     Set the overall real world brightness by setting the value triggered in the shaders
    /// </summary>
    /// <param name="newVal">New value trigged by the event </param>
    private void CameraBrightnessChangeHandler(float newVal)
    {
        Shader.SetGlobalFloat("_ZEDFactorAffectReal", m_cameraBrightness);
    }


    /// <summary>
    ///     Hmd To ZED calibratio has changed ? --> Need to re-adjust Camera Left and Camera Right local position regarding
    ///     ZEDRigRoot
    /// </summary>
    private bool calibrationHasChanged;

    private void CalibrationHasChanged()
    {
        calibrationHasChanged = true;
    }

    #endregion
}
//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using sl;
using UnityEditor;
using UnityEngine;

/// <summary>
///     Custom window editor : Displays the information about the ZED.
/// </summary>
public class ZEDCameraSettingsEditor : EditorWindow
{
    /// <summary>
    ///     Brightness default value
    /// </summary>
    private const int cbrightness = 4;

    /// <summary>
    ///     Contrast default value
    /// </summary>
    private const int ccontrast = 4;

    /// <summary>
    ///     Hue default value
    /// </summary>
    private const int chue = 0;

    /// <summary>
    ///     Saturation default value
    /// </summary>
    private const int csaturation = 4;

    /// <summary>
    ///     White balance default value
    /// </summary>
    private const int cwhiteBalance = 2600;

    /// <summary>
    ///     Path to save data
    /// </summary>
    private const string ZEDSettingsPath = "ZED_Settings.conf";

    /// <summary>
    ///     Refresh rate of the values of gain and exposure when auto mode is activated
    /// </summary>
    private const int refreshRate = 60;

    /// <summary>
    ///     Current brightness value
    /// </summary>
    private static int brightness = 4;

    /// <summary>
    ///     Current contrast value
    /// </summary>
    private static int contrast = 4;

    /// <summary>
    ///     Current hue value
    /// </summary>
    private static int hue;

    /// <summary>
    ///     Current saturation value
    /// </summary>
    private static int saturation = 4;

    private static int whiteBalance = cwhiteBalance;

    /// <summary>
    ///     Is the exposure and gain is in auto mode
    /// </summary>
    [SerializeField] public static bool groupAuto = true;

    /// <summary>
    ///     Is the whiteBalance is in auto mode
    /// </summary>
    [SerializeField] public static bool whiteBalanceAuto = true;

    /// <summary>
    ///     Is data is loaded from the file
    /// </summary>
    [SerializeField] public static bool loaded;

    private static int refreshCount;

    /// <summary>
    ///     Sat manual value
    /// </summary>
    private static bool setManualValue = true;

    private static bool setManualWhiteBalance = true;

    /// <summary>
    ///     default gui
    /// </summary>
    private static Color defaultColor;

    private static readonly GUIStyle style = new GUIStyle();
    private static readonly GUILayoutOption[] optionsButton = {GUILayout.MaxWidth(100)};


    private static ZEDCamera zedCamera;

    private static int tab;

    private static bool isInit;

    private static CalibrationParameters parameters;
    private static int zed_serial_number;
    private static int zed_fw_version;
    private static MODEL zed_model = MODEL.ZED;

    /// <summary>
    ///     Current exposure value
    /// </summary>
    [SerializeField] public int exposure;

    /// <summary>
    ///     Current gain value
    /// </summary>
    [SerializeField] public int gain;

    private bool launched;

    /// <summary>
    ///     Is a reset is wanted
    /// </summary>
    [SerializeField] public bool resetWanted;

    private void Draw()
    {
        if (zedCamera != null && Application.isPlaying)
        {
            parameters = zedCamera.GetCalibrationParameters(false);
            zed_serial_number = zedCamera.GetZEDSerialNumber();
            zed_fw_version = zedCamera.GetZEDFirmwareVersion();
            zed_model = zedCamera.GetCameraModel();
        }

        Repaint();
    }

    [MenuItem("Window/ZED Camera")]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        var window = (ZEDCameraSettingsEditor) GetWindow(typeof(ZEDCameraSettingsEditor), false, "ZED Camera");
        window.position = new Rect(window.position.x, window.position.y, 400, 400);
        style.normal.textColor = Color.red;
        style.fontSize = 15;
        style.margin.left = 5;

        parameters = new CalibrationParameters();
        window.Show();

        Debug.Log("Camera S/N : " + zed_serial_number);
        Debug.Log("Camera Model : " + zed_model);
        Debug.Log("Camera FW : " + zed_fw_version);
    }

    /// <summary>
    ///     Refresh data
    /// </summary>
    private void OnFocus()
    {
        if (zedCamera != null && zedCamera.IsCameraReady)
        {
            parameters = zedCamera.GetCalibrationParameters(false);
            zed_serial_number = zedCamera.GetZEDSerialNumber();
            zed_fw_version = zedCamera.GetZEDFirmwareVersion();
            zed_model = zedCamera.GetCameraModel();

            if (!loaded)
            {
                zedCamera.RetrieveCameraSettings();
                UpdateValuesCameraSettings();
            }
        }
    }

    /// <summary>
    ///     Init all the first values, and gets values from the ZED
    /// </summary>
    private void FirstInit()
    {
        if (!isInit)
        {
            zedCamera = ZEDCamera.GetInstance();
            EditorApplication.playmodeStateChanged += Draw;

            if (zedCamera != null && zedCamera.IsCameraReady)
            {
                isInit = true;

                if (!loaded)
                {
                    if (resetWanted)
                    {
                        ResetValues(groupAuto);
                        resetWanted = false;
                    }

                    zedCamera.RetrieveCameraSettings();
                    var settings = zedCamera.GetCameraSettings();
                    groupAuto = zedCamera.GetExposureUpdateType();
                    whiteBalanceAuto = zedCamera.GetWhiteBalanceUpdateType();

                    hue = settings.Hue;
                    brightness = settings.Brightness;
                    contrast = settings.Contrast;
                    saturation = settings.Saturation;

                    exposure = settings.Exposure;
                    gain = settings.Gain;

                    zedCamera.SetCameraSettings(CAMERA_SETTINGS.GAIN, gain, groupAuto);
                    zedCamera.SetCameraSettings(CAMERA_SETTINGS.EXPOSURE, exposure, groupAuto);
                    zedCamera.SetCameraSettings(CAMERA_SETTINGS.WHITEBALANCE, whiteBalance, whiteBalanceAuto);
                }
                else
                {
                    LoadCameraSettings();
                }

                parameters = zedCamera.GetCalibrationParameters(false);
                zed_serial_number = zedCamera.GetZEDSerialNumber();
                zed_fw_version = zedCamera.GetZEDFirmwareVersion();
                zed_model = zedCamera.GetCameraModel();
            }
        }
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    /// <summary>
    ///     View for the camera settings
    /// </summary>
    private void CameraSettingsView()
    {
        GUILayout.Label("Video Mode", EditorStyles.boldLabel);

        if (zedCamera != null && zedCamera.IsCameraReady)
        {
            EditorGUILayout.LabelField("Resolution ", zedCamera.ImageWidth + " x " + zedCamera.ImageHeight);
            EditorGUILayout.LabelField("FPS ", zedCamera.GetCameraFPS().ToString());
            launched = true;
        }
        else
        {
            EditorGUILayout.LabelField("Resolution ", 0 + " x " + 0);
            EditorGUILayout.LabelField("FPS ", "0");
        }

        EditorGUI.indentLevel = 0;
        GUILayout.Space(20);
        GUILayout.Label("Settings", EditorStyles.boldLabel);


        EditorGUI.BeginChangeCheck();
        brightness = EditorGUILayout.IntSlider("Brightness", brightness, 0, 8);
        if (EditorGUI.EndChangeCheck()) zedCamera.SetCameraSettings(CAMERA_SETTINGS.BRIGHTNESS, brightness, false);

        EditorGUI.BeginChangeCheck();
        contrast = EditorGUILayout.IntSlider("Contrast", contrast, 0, 8);
        if (EditorGUI.EndChangeCheck()) zedCamera.SetCameraSettings(CAMERA_SETTINGS.CONTRAST, contrast, false);

        EditorGUI.BeginChangeCheck();
        hue = EditorGUILayout.IntSlider("Hue", hue, 0, 11);
        if (EditorGUI.EndChangeCheck()) zedCamera.SetCameraSettings(CAMERA_SETTINGS.HUE, hue, false);

        EditorGUI.BeginChangeCheck();
        saturation = EditorGUILayout.IntSlider("Saturation", saturation, 0, 8);
        if (EditorGUI.EndChangeCheck()) zedCamera.SetCameraSettings(CAMERA_SETTINGS.SATURATION, saturation, false);
        EditorGUI.BeginChangeCheck();
        var origFontStyle = EditorStyles.label.fontStyle;
        EditorStyles.label.fontStyle = FontStyle.Bold;
        GUILayout.Space(20);

        whiteBalanceAuto = EditorGUILayout.Toggle("Automatic ", whiteBalanceAuto, EditorStyles.toggle);
        if (!whiteBalanceAuto && setManualWhiteBalance && EditorGUI.EndChangeCheck())
        {
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.WHITEBALANCE, whiteBalance / 100, false);
            setManualWhiteBalance = false;
        }

        if (whiteBalanceAuto && EditorGUI.EndChangeCheck())
        {
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.WHITEBALANCE, whiteBalance / 100, true);
            setManualWhiteBalance = true;
        }

        EditorGUI.BeginChangeCheck();
        GUI.enabled = !whiteBalanceAuto;
        whiteBalance = 100 * EditorGUILayout.IntSlider("White balance", whiteBalance / 100, 26, 65);
        if (!whiteBalanceAuto && EditorGUI.EndChangeCheck())
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.WHITEBALANCE, whiteBalance, false);

        GUI.enabled = true;
        EditorGUI.BeginChangeCheck();
        groupAuto = EditorGUILayout.Toggle("Automatic", groupAuto, EditorStyles.toggle);
        if (!groupAuto && setManualValue && EditorGUI.EndChangeCheck())
        {
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.GAIN, gain, false);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.EXPOSURE, exposure, false);
            setManualValue = false;
        }

        if (groupAuto && zedCamera.IsCameraReady && EditorGUI.EndChangeCheck())
        {
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.GAIN, gain, true);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.EXPOSURE, exposure, true);
            setManualValue = true;
        }

        EditorStyles.label.fontStyle = origFontStyle;

        GUI.enabled = !groupAuto;
        EditorGUI.BeginChangeCheck();
        gain = EditorGUILayout.IntSlider("Gain", gain, 0, 100);

        if (EditorGUI.EndChangeCheck())
            if (!groupAuto)
                zedCamera.SetCameraSettings(CAMERA_SETTINGS.GAIN, gain, false);
        EditorGUI.BeginChangeCheck();
        exposure = EditorGUILayout.IntSlider("Exposure", exposure, 0, 100);
        if (EditorGUI.EndChangeCheck())
            if (!groupAuto)
                zedCamera.SetCameraSettings(CAMERA_SETTINGS.EXPOSURE, exposure, false);

        refreshCount++;
        if (refreshCount >= refreshRate)
            if (zedCamera != null && zedCamera.IsCameraReady)
            {
                exposure = zedCamera.GetCameraSettings(CAMERA_SETTINGS.EXPOSURE);
                gain = zedCamera.GetCameraSettings(CAMERA_SETTINGS.GAIN);
                refreshCount = 0;
            }

        GUI.enabled = true;
        EditorGUI.indentLevel = 0;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset", optionsButton))
        {
            brightness = cbrightness;
            contrast = ccontrast;
            hue = chue;
            saturation = csaturation;

            groupAuto = true;

            whiteBalanceAuto = true;

            ResetValues(groupAuto);
            zedCamera.RetrieveCameraSettings();
            loaded = false;
            if (zedCamera != null) resetWanted = true;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save")) SaveCameraSettings();


        if (GUILayout.Button("Load")) LoadCameraSettings();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    /// <summary>
    ///     Reset all the values
    /// </summary>
    /// <param name="auto"></param>
    private void ResetValues(bool auto)
    {
        zedCamera.SetCameraSettings(CAMERA_SETTINGS.BRIGHTNESS, cbrightness, false);
        zedCamera.SetCameraSettings(CAMERA_SETTINGS.CONTRAST, ccontrast, false);
        zedCamera.SetCameraSettings(CAMERA_SETTINGS.HUE, 0, false);
        zedCamera.SetCameraSettings(CAMERA_SETTINGS.SATURATION, csaturation, false);

        if (auto)
        {
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.WHITEBALANCE, cwhiteBalance, true);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.GAIN, gain, true);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.EXPOSURE, exposure, true);
        }
    }

    /// <summary>
    ///     Save the camera settings in a file
    /// </summary>
    private void SaveCameraSettings()
    {
        zedCamera.SaveCameraSettings(ZEDSettingsPath);
    }

    /// <summary>
    ///     Get the values registered and update the interface
    /// </summary>
    private void UpdateValuesCameraSettings()
    {
        var settings = zedCamera.GetCameraSettings();
        hue = settings.Hue;

        brightness = settings.Brightness;
        contrast = settings.Contrast;
        exposure = settings.Exposure;
        saturation = settings.Saturation;
        gain = settings.Gain;
        whiteBalance = settings.WhiteBalance;
    }

    /// <summary>
    ///     Load the data from the file and update the current settings
    /// </summary>
    private void LoadCameraSettings()
    {
        zedCamera.LoadCameraSettings(ZEDSettingsPath);
        UpdateValuesCameraSettings();
        groupAuto = zedCamera.GetExposureUpdateType();
        whiteBalanceAuto = zedCamera.GetWhiteBalanceUpdateType();
        setManualWhiteBalance = true;
        loaded = true;
    }

    private void LabelHorizontal(string name, float value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name);
        GUILayout.Box(value.ToString());
        GUILayout.EndHorizontal();
    }

    /// <summary>
    ///     Calibration settings view
    /// </summary>
    private void CalibrationSettingsView()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Left camera", EditorStyles.boldLabel);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);

        LabelHorizontal("fx", parameters.leftCam.fx);
        LabelHorizontal("fy", parameters.leftCam.fy);
        LabelHorizontal("cx", parameters.leftCam.cx);
        LabelHorizontal("cy", parameters.leftCam.cy);

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        if (parameters.leftCam.disto != null)
        {
            LabelHorizontal("k1", (float) parameters.leftCam.disto[0]);
            LabelHorizontal("k2", (float) parameters.leftCam.disto[1]);
        }

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        LabelHorizontal("vFOV", parameters.leftCam.vFOV);
        LabelHorizontal("hFOV", parameters.leftCam.hFOV);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Right camera", EditorStyles.boldLabel);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);

        LabelHorizontal("fx", parameters.rightCam.fx);
        LabelHorizontal("fy", parameters.rightCam.fy);
        LabelHorizontal("cx", parameters.rightCam.cx);
        LabelHorizontal("cy", parameters.rightCam.cy);

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        if (parameters.rightCam.disto != null)
        {
            LabelHorizontal("k1", (float) parameters.rightCam.disto[0]);
            LabelHorizontal("k2", (float) parameters.rightCam.disto[1]);
        }

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        LabelHorizontal("vFOV", parameters.rightCam.vFOV);
        LabelHorizontal("hFOV", parameters.rightCam.hFOV);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Stereo", EditorStyles.boldLabel);

        GUILayout.BeginVertical(EditorStyles.helpBox);
        LabelHorizontal("Baseline", parameters.Trans[0]);
        LabelHorizontal("Convergence", parameters.Rot[1]);
        GUILayout.EndVertical();

        GUILayout.Label("Optional", EditorStyles.boldLabel);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        LabelHorizontal("Rx", parameters.Rot[0]);
        LabelHorizontal("Rz", parameters.Rot[2]);
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void OnGUI()
    {
        FirstInit();
        defaultColor = GUI.color;
        if (zedCamera != null && zedCamera.IsCameraReady)
            GUI.color = Color.green;
        else GUI.color = Color.red;
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.FlexibleSpace();
        if (zedCamera != null && zedCamera.IsCameraReady)
        {
            style.normal.textColor = Color.black;
            GUILayout.Label("Online", style);
        }
        else
        {
            style.normal.textColor = Color.black;
            if (!launched)
                GUILayout.Label("To access information, please launch your scene once", style);
            else
                GUILayout.Label("Offline", style);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.color = defaultColor;
        EditorGUI.BeginChangeCheck();
        tab = GUILayout.Toolbar(tab, new[] {"Camera Control", "Calibration"});
        if (EditorGUI.EndChangeCheck())
            if (zedCamera != null && zedCamera.IsCameraReady)
                parameters = zedCamera.GetCalibrationParameters(false);
        switch (tab)
        {
            case 0:
                CameraSettingsView();
                break;

            case 1:
                CalibrationSettingsView();
                break;

            default:
                CameraSettingsView();
                break;
        }
    }
}
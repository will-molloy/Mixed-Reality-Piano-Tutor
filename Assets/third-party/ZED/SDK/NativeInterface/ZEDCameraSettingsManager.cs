//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using System;
using System.IO;
using System.Runtime.InteropServices;
using sl;

/// <summary>
///     Stores the camera settings and is used as an interface with the ZED
/// </summary>
public class ZEDCameraSettingsManager
{
    private const string nameDll = "sl_unitywrapper";

    /// <summary>
    ///     Is in auto mode
    /// </summary>
    public bool auto = true;

    /// <summary>
    ///     Reference to the container
    /// </summary>
    private readonly CameraSettings settings_;

    public bool whiteBalanceAuto = true;


    public ZEDCameraSettingsManager()
    {
        settings_ = new CameraSettings();
    }

    public CameraSettings Settings => settings_.Clone();

    [DllImport(nameDll, EntryPoint = "dllz_set_camera_settings")]
    private static extern void dllz_set_camera_settings(int mode, int value, int usedefault);

    [DllImport(nameDll, EntryPoint = "dllz_get_camera_settings")]
    private static extern int dllz_get_camera_settings(int mode);

    /// <summary>
    ///     Set settings from the container to the camera
    /// </summary>
    /// <param name="zedCamera"></param>
    public void SetSettings(ZEDCamera zedCamera)
    {
        if (zedCamera != null)
        {
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.BRIGHTNESS, settings_.Brightness, false);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.CONTRAST, settings_.Contrast, false);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.HUE, settings_.Hue, false);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.SATURATION, settings_.Saturation, false);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.GAIN, settings_.Gain, false);
            zedCamera.SetCameraSettings(CAMERA_SETTINGS.EXPOSURE, settings_.Exposure, false);
            if (settings_.WhiteBalance != -1)
                zedCamera.SetCameraSettings(CAMERA_SETTINGS.WHITEBALANCE, settings_.WhiteBalance, false);
        }
    }

    /// <summary>
    ///     Load camera settings from a file, and set them to the container and camera
    /// </summary>
    /// <param name="zedCamera"></param>
    /// <param name="path"></param>
    public void LoadCameraSettings(ZEDCamera zedCamera, string path = "ZED_Settings.conf")
    {
        string[] lines = null;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception)
        {
        }

        if (lines == null) return;

        foreach (var line in lines)
        {
            var splittedLine = line.Split('=');
            if (splittedLine.Length == 2)
            {
                var key = splittedLine[0];
                var field = splittedLine[1];

                if (key == "brightness")
                    settings_.Brightness = int.Parse(field);
                else if (key == "contrast")
                    settings_.Contrast = int.Parse(field);
                else if (key == "hue")
                    settings_.Hue = int.Parse(field);
                else if (key == "saturation")
                    settings_.Saturation = int.Parse(field);
                else if (key == "whiteBalance")
                    settings_.WhiteBalance = int.Parse(field);
                else if (key == "gain")
                    settings_.Gain = int.Parse(field);
                else if (key == "exposure") settings_.Exposure = int.Parse(field);
            }
        }

        SetSettings(zedCamera);
        auto = settings_.Exposure == -1;
        whiteBalanceAuto = settings_.WhiteBalance == -1;
    }


    /// <summary>
    ///     Retrieves settings from the camera
    /// </summary>
    /// <param name="zedCamera"></param>
    public void RetrieveSettingsCamera(ZEDCamera zedCamera)
    {
        if (zedCamera != null)
        {
            settings_.Brightness = zedCamera.GetCameraSettings(CAMERA_SETTINGS.BRIGHTNESS);
            settings_.Contrast = zedCamera.GetCameraSettings(CAMERA_SETTINGS.CONTRAST);
            settings_.Hue = zedCamera.GetCameraSettings(CAMERA_SETTINGS.HUE);
            settings_.Saturation = zedCamera.GetCameraSettings(CAMERA_SETTINGS.SATURATION);
            settings_.Gain = zedCamera.GetCameraSettings(CAMERA_SETTINGS.GAIN);
            settings_.Exposure = zedCamera.GetCameraSettings(CAMERA_SETTINGS.EXPOSURE);
            settings_.WhiteBalance = zedCamera.GetCameraSettings(CAMERA_SETTINGS.WHITEBALANCE);
        }
    }

    /// <summary>
    ///     Set settings of the camera
    /// </summary>
    /// <param name="settings">The setting which will be changed</param>
    /// <param name="value">The value</param>
    /// <param name="usedefault">
    ///     will set default (or automatic) value if set to true (value (int) will not be taken into
    ///     account)
    /// </param>
    public void SetCameraSettings(CAMERA_SETTINGS settings, int value, bool usedefault = false)
    {
        settings_.settings[(int) settings] = !usedefault && value != -1 ? value : -1;
        dllz_set_camera_settings((int) settings, value, Convert.ToInt32(usedefault));
    }

    /// <summary>
    ///     Get the value from a setting of the camera
    /// </summary>
    /// <param name="settings"></param>
    public int GetCameraSettings(CAMERA_SETTINGS settings)
    {
        settings_.settings[(int) settings] = dllz_get_camera_settings((int) settings);
        return settings_.settings[(int) settings];
    }

    /// <summary>
    ///     Save the camera settings into a file
    /// </summary>
    /// <param name="path"></param>
    public void SaveCameraSettings(string path)
    {
        using (var file = new StreamWriter(path))
        {
            file.WriteLine("brightness=" + settings_.Brightness);
            file.WriteLine("contrast=" + settings_.Contrast);
            file.WriteLine("hue=" + settings_.Hue);
            file.WriteLine("saturation=" + settings_.Saturation);
            file.WriteLine("whiteBalance=" + settings_.WhiteBalance);
            file.WriteLine("gain=" + settings_.Gain);
            file.WriteLine("exposure=" + settings_.Exposure);
            file.Close();
        }
    }

    /// <summary>
    ///     Container of the camera settings
    /// </summary>
    public class CameraSettings
    {
        public int[] settings = new int[Enum.GetNames(typeof(CAMERA_SETTINGS)).Length];

        public CameraSettings(int brightness = 4, int contrast = 4, int hue = 0, int saturation = 4,
            int whiteBalance = -1, int gain = -1, int exposure = -1)
        {
            settings = new int[Enum.GetNames(typeof(CAMERA_SETTINGS)).Length];
            settings[(int) CAMERA_SETTINGS.BRIGHTNESS] = brightness;
            settings[(int) CAMERA_SETTINGS.CONTRAST] = contrast;
            settings[(int) CAMERA_SETTINGS.SATURATION] = saturation;
            settings[(int) CAMERA_SETTINGS.HUE] = hue;
            settings[(int) CAMERA_SETTINGS.WHITEBALANCE] = whiteBalance;
            settings[(int) CAMERA_SETTINGS.GAIN] = gain;
            settings[(int) CAMERA_SETTINGS.EXPOSURE] = exposure;
        }

        public CameraSettings(CameraSettings other)
        {
            settings = new int[Enum.GetNames(typeof(CAMERA_SETTINGS)).Length];
            settings[(int) CAMERA_SETTINGS.BRIGHTNESS] = other.settings[(int) CAMERA_SETTINGS.BRIGHTNESS];
            settings[(int) CAMERA_SETTINGS.CONTRAST] = other.settings[(int) CAMERA_SETTINGS.CONTRAST];
            settings[(int) CAMERA_SETTINGS.SATURATION] = other.settings[(int) CAMERA_SETTINGS.SATURATION];
            settings[(int) CAMERA_SETTINGS.HUE] = other.settings[(int) CAMERA_SETTINGS.HUE];
            settings[(int) CAMERA_SETTINGS.WHITEBALANCE] = other.settings[(int) CAMERA_SETTINGS.WHITEBALANCE];
            settings[(int) CAMERA_SETTINGS.GAIN] = other.settings[(int) CAMERA_SETTINGS.GAIN];
            settings[(int) CAMERA_SETTINGS.EXPOSURE] = other.settings[(int) CAMERA_SETTINGS.EXPOSURE];
        }


        public int Brightness
        {
            get { return settings[(int) CAMERA_SETTINGS.BRIGHTNESS]; }

            set { settings[(int) CAMERA_SETTINGS.BRIGHTNESS] = value; }
        }

        public int Saturation
        {
            get { return settings[(int) CAMERA_SETTINGS.SATURATION]; }

            set { settings[(int) CAMERA_SETTINGS.SATURATION] = value; }
        }

        public int Hue
        {
            get { return settings[(int) CAMERA_SETTINGS.HUE]; }

            set { settings[(int) CAMERA_SETTINGS.HUE] = value; }
        }

        public int Contrast
        {
            get { return settings[(int) CAMERA_SETTINGS.CONTRAST]; }

            set { settings[(int) CAMERA_SETTINGS.CONTRAST] = value; }
        }

        public int Gain
        {
            get { return settings[(int) CAMERA_SETTINGS.GAIN]; }

            set { settings[(int) CAMERA_SETTINGS.GAIN] = value; }
        }

        public int Exposure
        {
            get { return settings[(int) CAMERA_SETTINGS.EXPOSURE]; }

            set { settings[(int) CAMERA_SETTINGS.EXPOSURE] = value; }
        }

        public int WhiteBalance
        {
            get { return settings[(int) CAMERA_SETTINGS.WHITEBALANCE]; }

            set { settings[(int) CAMERA_SETTINGS.WHITEBALANCE] = value; }
        }

        public CameraSettings Clone()
        {
            return new CameraSettings(this);
        }
    }
}
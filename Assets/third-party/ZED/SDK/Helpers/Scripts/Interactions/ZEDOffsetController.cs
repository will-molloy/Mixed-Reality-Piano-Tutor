//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============


using System;
using System.Globalization;
using System.IO;
using sl;
using UnityEngine;
using Valve.VR;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
///     Enables to save/load the position of the ZED, is useful especially for the greenScreen
/// </summary>
public class ZEDOffsetController : MonoBehaviour
{
    /// <summary>
    ///     ZED pose file name
    /// </summary>
    [SerializeField] public static string ZEDOffsetFile = "ZED_Position_Offset.conf";

    public bool isReady;

    public ZEDControllerTracker padManager;

    private string path = @"Stereolabs\steamvr";

    /// <summary>
    ///     Save the position of the ZED
    /// </summary>
    public void SaveZEDPos()
    {
        using (var file = new StreamWriter(path))
        {
            var tx = "x=" + transform.localPosition.x + " //Translation x";
            var ty = "y=" + transform.localPosition.y + " //Translation y";
            var tz = "z=" + transform.localPosition.z + " //Translation z";
            var rx = "rx=" + transform.localRotation.eulerAngles.x + " //Rotation x";
            var ry = "ry=" + transform.localRotation.eulerAngles.y + " //Rotation y";
            var rz = "rz=" + transform.localRotation.eulerAngles.z + " //Rotation z";


            file.WriteLine(tx);
            file.WriteLine(ty);
            file.WriteLine(tz);
            file.WriteLine(rx);
            file.WriteLine(ry);
            file.WriteLine(rz);
            if (ZEDCamera.GetInstance().IsCameraReady)
            {
                var fov = "fov=" + ZEDCamera.GetInstance().GetFOV() * Mathf.Rad2Deg;
                file.WriteLine(fov);
            }


#if ZED_STEAM_VR
            if (PadComponentExist())
            {
                var i = "indexController=" +
                        (padManager.index > 0
                            ? SteamVR.instance.GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String,
                                (uint) padManager.index)
                            : "NONE") + " //SN of the pad attached to the camera (NONE to set no pad on it)";
                file.WriteLine(i);
            }
#endif


            file.Close();
        }
    }

    public bool PadComponentExist()
    {
        if (padManager != null)
            return true;
        return false;
    }

    private void OnEnable()
    {
        LoadComponentPad();
    }

    private void LoadComponentPad()
    {
        var pad = GetComponent<ZEDControllerTracker>();
        if (pad == null)
            pad = GetComponentInParent<ZEDControllerTracker>();
        if (pad == null)
            pad = GetComponentInChildren<ZEDControllerTracker>();
        if (pad != null)
            padManager = pad;
    }

    private void Awake()
    {
        LoadComponentPad();

        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var specificFolder = Path.Combine(folder, @"Stereolabs\steamvr");
        path = Path.Combine(specificFolder, ZEDOffsetFile);

        // Check if folder exists and if not, create it
        if (!Directory.Exists(specificFolder))
            Directory.CreateDirectory(specificFolder);


        LoadZEDPos();
        CreateFileWatcher(specificFolder);

        isReady = true;
    }

    private void Update()
    {
        if (ZEDManager.Instance.IsZEDReady)
            LoadZEDPos();
    }

    /// <summary>
    ///     Load the position of the ZED from a file
    /// </summary>
    public void LoadZEDPos()
    {
        if (!File.Exists(path)) return;

        string[] lines = null;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception)
        {
            padManager.SNHolder = "NONE";
        }

        if (lines == null)
        {
            padManager.SNHolder = "NONE";
            return;
        }

        if (lines == null) return;
        var position = new Vector3(0, 0, 0);
        var eulerRotation = new Vector3(0, 0, 0);
        foreach (var line in lines)
        {
            var splittedLine = line.Split('=');
            if (splittedLine != null && splittedLine.Length >= 2)
            {
                var key = splittedLine[0];
                var field = splittedLine[1].Split(' ')[0];

                if (key == "x")
                {
                    position.x = float.Parse(field, CultureInfo.InvariantCulture);
                }
                else if (key == "y")
                {
                    position.y = float.Parse(field, CultureInfo.InvariantCulture);
                }
                else if (key == "z")
                {
                    position.z = float.Parse(field, CultureInfo.InvariantCulture);
                }
                else if (key == "rx")
                {
                    eulerRotation.x = float.Parse(field, CultureInfo.InvariantCulture);
                }
                else if (key == "ry")
                {
                    eulerRotation.y = float.Parse(field, CultureInfo.InvariantCulture);
                }
                else if (key == "rz")
                {
                    eulerRotation.z = float.Parse(field, CultureInfo.InvariantCulture);
                }
                else if (key == "indexController")
                {
                    LoadComponentPad();

                    if (PadComponentExist()) padManager.SNHolder = field;
                }
            }
        }

        transform.localPosition = position;
        transform.localRotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, eulerRotation.z);
    }

    public void CreateFileWatcher(string path)
    {
        // Create a new FileSystemWatcher and set its properties.
        var watcher = new FileSystemWatcher();
        watcher.Path = path;
        /* Watch for changes in LastAccess and LastWrite times, and 
           the renaming of files or directories. */
        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                                        | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        // Only watch text files.
        watcher.Filter = ZEDOffsetFile;

        // Add event handlers.
        watcher.Changed += OnChanged;

        // Begin watching.
        watcher.EnableRaisingEvents = true;
    }

    // Define the event handlers.
    private void OnChanged(object source, FileSystemEventArgs e)
    {
        if (PadComponentExist()) LoadZEDPos();
    }
}

#if UNITY_EDITOR


[CustomEditor(typeof(ZEDOffsetController))]
public class ZEDPositionEditor : Editor
{
    private ZEDOffsetController positionManager;

    public void OnEnable()
    {
        positionManager = (ZEDOffsetController) target;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = positionManager.isReady;
        if (GUILayout.Button("Save Camera Offset")) positionManager.SaveZEDPos();
        if (GUILayout.Button("Load Camera Offset")) positionManager.LoadZEDPos();
        EditorGUILayout.EndHorizontal();
    }
}

#endif
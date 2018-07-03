using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Aruco;
using Emgu.CV.Util;
using Emgu.CV.Structure;


public class EmguCvVideoTest : MonoBehaviour
{
    [SerializeField]
    private RawImage rawImage;
    [SerializeField]
    private AspectRatioFitter ratio;
    private WebCamTexture cam; // should be able to use ZED right ??
    private Texture defaultImage;
    private Texture2D resultTexture;
    private static readonly Dictionary dictionary = new Dictionary(6, 7);
    private static readonly MCvScalar borderColor = new MCvScalar(0, 255, 0);
    private static readonly DetectorParameters parameters = DetectorParameters.GetDefault();

    void Start()
    {
        // Init camera
        defaultImage = rawImage.texture;
        var devices = WebCamTexture.devices;
        var cameraCount = devices.Length;
        devices.ToList().ForEach(x => Debug.Log("Found: " + x.name));
        if (devices.Length == 0)
        {
            Debug.LogError("No device found.");
            return;
        }
        cam = new WebCamTexture(devices[0].name);
        cam.Play();

        // Init other stuff
        CvInvoke.CheckLibraryLoaded();
        rawImage.texture = cam;
        ratio.aspectRatio = (float)cam.width / (float)cam.height;
        rawImage.rectTransform.localScale = Vector3.one;
    }

    void Update()
    {
        if (cam == null || !cam.didUpdateThisFrame)
        {
            return;
        }
        // Process frame
        var data = new Color32[cam.width * cam.height];
        var bytes = new byte[data.Length * 3];

        cam.GetPixels32(data);
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var resultHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        using (Mat bgra = new Mat(new Size(cam.width, cam.height), DepthType.Cv8U, 4, handle.AddrOfPinnedObject(), cam.width * 4))
        using (Mat bgr = new Mat(cam.height, cam.width, DepthType.Cv8U, 3, resultHandle.AddrOfPinnedObject(), cam.width * 3))
        {
            CvInvoke.CvtColor(bgra, bgr, ColorConversion.Bgra2Bgr);

            #region image processing

            bgr.Save("Assets/Scenes/test/emgucv/Resources/bgr-frame.png");
            var corners = new VectorOfVectorOfPointF();
            var ids = new VectorOfInt();
            ArucoInvoke.DetectMarkers(bgr, dictionary, corners, ids, parameters);
            Debug.Log("Markers found: " + ids.Size);
            ArucoInvoke.DrawDetectedMarkers(bgr, corners, ids, borderColor);

            #endregion
        }
        handle.Free();
        resultHandle.Free();

        if (resultTexture == null || resultTexture.width != cam.width || resultTexture.height != cam.height)
        {
            resultTexture = new Texture2D(cam.width, cam.height, TextureFormat.RGB24, false);
        }

        // Update frame
        resultTexture.LoadRawTextureData(bytes);
        resultTexture.Apply();
        rawImage.texture = resultTexture;
    }

}

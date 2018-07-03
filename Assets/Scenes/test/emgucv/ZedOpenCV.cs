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
using sl;

public class ZedOpenCV : MonoBehaviour
{

    private ZEDCamera zedCam;

    void Start()
    {
        zedCam = ZEDCamera.GetInstance();
        var zedMat = new ZEDMat(new sl.Resolution((uint)zedCam.ImageWidth, (uint)zedCam.ImageHeight), ZEDMat.MAT_TYPE.MAT_32F_C1);
        zedCam.RetrieveImage(zedMat, VIEW.LEFT);
		var cvMat = new Mat();
		// https://github.com/stereolabs/zed-unity/issues/15
		// Marshal.Copy(zedMat.MatPtr, cvMat.Ptr, 0, zedMat.GetWidth() * zedMat.GetHeight() * 4);
        var bgra = new Mat(new Size(zedMat.GetWidth(), zedMat.GetHeight()), DepthType.Cv8U, 3, zedMat.MatPtr, zedMat.GetWidth() * 3);
        bgra.Save("Assets/Scenes/test/emgucv/Resources/zed-frame-converted.png");

    }

    void Update()
    {

    }
}

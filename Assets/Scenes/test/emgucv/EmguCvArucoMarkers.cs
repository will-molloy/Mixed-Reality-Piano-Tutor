using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.Structure;
using Emgu.CV.Util;

public class EmguCvArucoMarkers : MonoBehaviour
{

    void Start()
    {
        CreateMarkers();
        DetectAndDrawMarkers();
    }

    void CreateMarkers()
    {
        var image = new Mat();
        var dict = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_250);
        Enumerable.Range(0, 10).ToList().ForEach(x =>
        {
            ArucoInvoke.DrawMarker(dict, x, 200, image);
            image.Save("Assets/Scenes/test/emgucv/Resources/marker-" + x + ".png");
        });
    }

    void DetectAndDrawMarkers()
    {
        var image = new Image<Bgr, byte>("Assets/Scenes/test/emgucv/Resources/somemarkers.png");
        var dict = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_250);
        var corners = new VectorOfVectorOfPointF();
        var ids = new VectorOfInt();
        var parameters = DetectorParameters.GetDefault();
        ArucoInvoke.DetectMarkers(image, dict, corners, ids, parameters);

        Debug.Log("Corners: " + corners.Size);
        for (var i = 0; i < corners.Size; i++)
        {
            Debug.Log("Corners[" + i + "] (should be 4): " + corners[i].Size);
        }
        Debug.Log("Ids (should match corners size): " + ids.Size);

        var borderColor = new MCvScalar(0, 255, 0);
        ArucoInvoke.DrawDetectedMarkers(image, corners, ids, borderColor);
        image.Save("Assets/Scenes/test/emgucv/Resources/detected-markers.png");
    }

}

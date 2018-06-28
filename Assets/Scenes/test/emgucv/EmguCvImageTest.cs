using UnityEngine;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

public class EmguCvImageTest : MonoBehaviour
{

    private const int numMarkers = 6;
    private Dictionary dict = new Dictionary(numMarkers, 7);

    void Start()
    {
        CreateMarkers();
        DetectAndDrawMarkers();
    }

    void CreateMarkers()
    {
        var image = new Mat();
        Enumerable.Range(0, numMarkers).ToList().ForEach(x =>
        {
            ArucoInvoke.DrawMarker(dict, x, 200, image);
            image.Save("Assets/Scenes/test/emgucv/Resources/marker-" + x + ".png");
        });
    }

    void DetectAndDrawMarkers()
    {
        var image = new Image<Bgr, byte>("Assets/Scenes/test/emgucv/Resources/somemarkers.png");
        var corners = new VectorOfVectorOfPointF();
        var ids = new VectorOfInt();
        var parameters = DetectorParameters.GetDefault();
        ArucoInvoke.DetectMarkers(image, dict, corners, ids, parameters);

        // Log(corners, ids);

        var borderColor = new MCvScalar(0, 255, 0);
        ArucoInvoke.DrawDetectedMarkers(image, corners, ids, borderColor);
        image.Save("Assets/Scenes/test/emgucv/Resources/detected-markers-bgr.png");
    }

    public static void Log(VectorOfVectorOfPointF corners, VectorOfInt ids)
    {
        Debug.Log("Corners: " + corners.Size);
        for (var i = 0; i < corners.Size; i++)
        {
            Debug.Log("Corners[" + i + "] (should be 4): " + corners[i].Size);
        }
        Debug.Log("Ids (should match corners size): " + ids.Size);
    }

}

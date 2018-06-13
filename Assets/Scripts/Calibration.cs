using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>  
/// - Place cube markers to represent real-virtual piano mapping
/// </summary>  
public class Calibration : MonoBehaviour
{

    private static CalibrationState caliState = CalibrationState.None;
    private bool lastButtonState = false;
    public GameObject leftMarker;
    public GameObject rightMarker;
    private List<GameObject> cubes;
    private static GameObject fixedObject;
    private enum CalibrationState
    {
        None, LeftMarked, BothMarked
    }

    void Start()
    {
        fixedObject = new GameObject();
        cubes = new List<GameObject>(new GameObject[] { leftMarker, rightMarker });
    }

    void Update()
    {
        var a = OVRInput.Get(OVRInput.Button.One);
        if (a && !lastButtonState)
        {
            switch (caliState)
            {
                case CalibrationState.None:
                    {
                        leftMarker.transform.SetParent(fixedObject.transform);
                        caliState++;
                        break;
                    }
                case CalibrationState.LeftMarked:
                    {
                        rightMarker.transform.SetParent(fixedObject.transform);
                        caliState++;
                        Piano.instance.BuildPiano(leftMarker, rightMarker);
                        break;
                    }
                default:
                    {
                        cubes.ForEach(cube =>
                        {
                            cube.transform.SetParent(this.transform);
                            cube.transform.localPosition = new Vector3(0, 0, 0.2f);
                            cube.transform.localRotation = new Quaternion(0, 0, 0, 1);
                        });
                        caliState = CalibrationState.None;
                        Piano.instance.clearPiano();
                        break;
                    }
            }
        }
        lastButtonState = a;
    }
}

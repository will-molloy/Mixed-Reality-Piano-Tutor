using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using System;
using System.Drawing;

public class EmguCvTestInstall : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        var picture = new Image<Bgr, byte>("Assets/Scenes/test/emgucv/picture1.jpg");
        var color = new Bgr(255, 255, 255);
        for (int i = 0; i < Math.Min(picture.Size.Height, picture.Size.Width); i++)
        {
            picture[i, i] = color;
        }
        picture.Save("Assets/Scenes/test/emgucv/picture2.jpg");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

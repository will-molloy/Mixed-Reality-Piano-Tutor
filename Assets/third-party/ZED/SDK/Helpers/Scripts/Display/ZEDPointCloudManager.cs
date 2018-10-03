//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using sl;
using UnityEngine;

/// <summary>
///     Displays the point cloud in front of the camera
/// </summary>
public class ZEDPointCloudManager : MonoBehaviour
{
    /// <summary>
    ///     Color of each point
    /// </summary>
    private Texture2D colorTexture;

    /// <summary>
    ///     Temporary copy of the ColorTexture to stop the point cloud to a defined moment
    /// </summary>
    private RenderTexture ColorTextureCopy;

    /// <summary>
    ///     It's the current state of display. If set to false the point cloud will be hidden
    /// </summary>
    public bool display = true;

    /// <summary>
    ///     This camera will not see the point cloud
    /// </summary>
    public Camera hiddenObjectFromCamera;

    /// <summary>
    ///     Material used to display the point cloud
    /// </summary>
    private Material mat;

    /// <summary>
    ///     Number of points displayed usually, the WIDTH*HEIGHT of the ZED
    /// </summary>
    private int numberPoints;

    /// <summary>
    ///     Falg to check if the update has changed state
    /// </summary>
    private bool previousUpdate = true;

    /// <summary>
    ///     It's the cuurent state of updating.
    ///     If set to false, the point cloud will not be updated and will be drawn with the content of the temp textures
    /// </summary>
    public bool update = true;

    /// <summary>
    ///     3D position of the points
    /// </summary>
    private Texture2D XYZTexture;

    /// <summary>
    ///     Temporary copy of the XYZTexture to stop the point cloud to a defined moment
    /// </summary>
    private RenderTexture XYZTextureCopy;

    /// <summary>
    ///     Instance of the ZED
    /// </summary>
    private ZEDCamera zed;


    private void Start()
    {
        zed = ZEDCamera.GetInstance();
    }

    // Update is called once per frame
    private void Update()
    {
        if (zed.IsCameraReady)
        {
            if (numberPoints == 0)
            {
                //Creations of the textures
                XYZTexture = zed.CreateTextureMeasureType(MEASURE.XYZ);
                colorTexture = zed.CreateTextureImageType(VIEW.LEFT);
                numberPoints = zed.ImageWidth * zed.ImageHeight;

                //Load and set the materials properties
                mat = Resources.Load("Materials/PointCloud/Mat_ZED_PointCloud") as Material;
                if (mat != null)
                {
                    mat.SetTexture("_XYZTex", XYZTexture);
                    mat.SetTexture("_ColorTex", colorTexture);
                }
            }

            //If stop updated, create new render texture and fill them with the textures from the ZED.
            // These textures will be displayed as they are not updated
            if (!update && previousUpdate != update)
            {
                if (XYZTextureCopy == null)
                    XYZTextureCopy = new RenderTexture(XYZTexture.width, XYZTexture.height, 0,
                        RenderTextureFormat.ARGBFloat);

                if (ColorTextureCopy == null)
                    ColorTextureCopy = new RenderTexture(colorTexture.width, colorTexture.height, 0,
                        RenderTextureFormat.ARGB32);


                Graphics.Blit(XYZTexture, XYZTextureCopy);
                Graphics.Blit(colorTexture, ColorTextureCopy);

                if (mat != null)
                {
                    mat.SetTexture("_XYZTex", XYZTextureCopy);
                    mat.SetTexture("_ColorTex", ColorTextureCopy);
                }
            }

            //Display the right textures
            if (update && previousUpdate != update && mat != null)
            {
                mat.SetTexture("_XYZTex", XYZTexture);
                mat.SetTexture("_ColorTex", colorTexture);
            }

            previousUpdate = update;
        }
    }


    private void OnApplicationQuit()
    {
        mat = null;
        if (XYZTextureCopy != null) XYZTextureCopy.Release();

        if (ColorTextureCopy != null) ColorTextureCopy.Release();
    }

    private void OnRenderObject()
    {
        if (mat != null)
        {
            if (hiddenObjectFromCamera == Camera.current) return;

            if (!display) return;
            mat.SetMatrix("_Position", transform.localToWorldMatrix);
            mat.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, 1, numberPoints);
        }
    }
}
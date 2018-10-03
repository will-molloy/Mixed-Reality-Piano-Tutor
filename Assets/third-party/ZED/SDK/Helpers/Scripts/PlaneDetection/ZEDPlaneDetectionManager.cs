//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using sl;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
///     Contols the ZEDSpatialMapping and hides its implementation
/// </summary>
[DisallowMultipleComponent]
public class ZEDPlaneDetectionManager : MonoBehaviour
{
    public static bool isDisplay;

    public bool addPhysicsOption = true;
    private float estimatedPlayerHeight;

    private GameObject floorPlaneGO;

    public List<ZEDPlaneGameObject> hitPlaneList;
    private GameObject holder; //Object all planes are parented to, called [ZED Planes] in Hierarchy
    public bool isVisibleInGameOption = true;
    public bool isVisibleInSceneOption = true;
    private Camera LeftCamera;
    private ZEDManager manager;

    private readonly ZEDPlaneRenderer[] meshRenderer = new ZEDPlaneRenderer[2];

    public Material overrideMaterial; //If null, shows wireframe. Otherwise, displays your custom material. 

    private int planeHitCount;
    private int[] planeMeshTriangles; //Buffer for triangle data from SDK 

    private Vector3[] planeMeshVertices; //Buffer for vertex data from SDK
    private ZEDCamera zedCam;

    public bool IsReady { get; private set; }

    public bool HasDetectedFloor { get; private set; }

    public ZEDPlaneGameObject getFloorPlane { get; private set; }


    public float GetEstimatedPlayerHeight => estimatedPlayerHeight;

    public ZEDPlaneGameObject getHitPlane(int i)
    {
        if (i < hitPlaneList.Count)
            return hitPlaneList[i];
        return null;
    }

	/// <summary>
	///     Start this instance.
	/// </summary>
	private void Start()
    {
        manager = FindObjectOfType(typeof(ZEDManager)) as ZEDManager;

        if (manager && manager.GetLeftCameraTransform())
            LeftCamera = manager.GetLeftCameraTransform().gameObject.GetComponent<Camera>();

        zedCam = ZEDCamera.GetInstance();
        IsReady = false;

        //Create a holder for all the planes
        holder = new GameObject();
        holder.name = "[ZED Planes]";
        holder.transform.parent = transform;
        holder.transform.position = Vector3.zero;
        holder.transform.rotation = Quaternion.identity;
        StaticBatchingUtility.Combine(holder);

        //initialize Vertices/Triangles with enough length
        planeMeshVertices = new Vector3[65000];
        planeMeshTriangles = new int[65000];

        //floorPlaneGO = holder.AddComponent<ZEDPlaneGameObject> ();
        hitPlaneList = new List<ZEDPlaneGameObject>();

        SetPlaneRenderer();
    }


	/// <summary>
	///     Event When ZED is ready
	/// </summary>
	private void ZEDReady()
    {
        if (LeftCamera)
        {
            IsReady = true;
            isDisplay = isVisibleInGameOption;
            SetPlaneRenderer();
        }
    }


	/// <summary>
	///     Raises the enable event.
	/// </summary>
	public void OnEnable()
    {
        ZEDManager.OnZEDReady += ZEDReady;
    }


	/// <summary>
	///     Raises the disable event.
	/// </summary>
	public void OnDisable()
    {
        if (IsReady)
        {
            foreach (Transform child in holder.transform) Destroy(child.gameObject);
            Destroy(holder);
        }

        ZEDManager.OnZEDReady -= ZEDReady;
    }


	/// <summary>
	///     Set the plane renderer to the cameras. Is necessary to see the planes
	/// </summary>
	public void SetPlaneRenderer()
    {
        if (manager != null)
        {
            var left = manager.GetLeftCameraTransform();
            if (left != null)
            {
                meshRenderer[0] = left.gameObject.GetComponent<ZEDPlaneRenderer>();
                if (!meshRenderer[0]) meshRenderer[0] = left.gameObject.AddComponent<ZEDPlaneRenderer>();
            }

            var right = manager.GetRightCameraTransform();
            if (right != null)
            {
                meshRenderer[1] = right.gameObject.GetComponent<ZEDPlaneRenderer>();
                if (!meshRenderer[1]) meshRenderer[1] = right.gameObject.AddComponent<ZEDPlaneRenderer>();
            }
        }
    }

	/// <summary>
	///     Transforms the plane mesh from Camera frame to local frame, where each vertex is relative to the plane's center.
	/// </summary>
	/// <param name="camera">Camera transform.</param>
	/// <param name="srcVertices">Source vertices (in camera space).</param>
	/// <param name="srcTriangles">Source triangles (in camera space).</param>
	/// <param name="dstVertices">Dst vertices (in world space).</param>
	/// <param name="dstTriangles">Dst triangles (in world space).</param>
	/// <param name="numVertices">Number of vertices.</param>
	/// <param name="numTriangles">Number of triangles.</param>
	private void TransformCameraToLocalMesh(Transform camera, Vector3[] srcVertices, int[] srcTriangles,
        Vector3[] dstVertices, int[] dstTriangles, int numVertices, int numTriangles, Vector3 centerpos)
    {
        //Since we are in Camera
        if (numVertices == 0 || numTriangles == 0)
            return;

        Array.Copy(srcVertices, dstVertices, numVertices);
        Buffer.BlockCopy(srcTriangles, 0, dstTriangles, 0, numTriangles * sizeof(int));

        for (var i = 0; i < numVertices; i++)
        {
            dstVertices[i] -= centerpos;
            dstVertices[i] = camera.transform.rotation * dstVertices[i];
        }
    }

	/// <summary>
	///     Detects the floor plane. Replaces the current floor plane, if there is one, unlike DetectPlaneAtHit.
	/// </summary>
	/// <returns><c>true</c>, if floor plane was detected, <c>false</c> otherwise.</returns>
	public bool DetectFloorPlane(bool auto)
    {
        if (!IsReady)
            return false;

        var plane = new ZEDPlaneGameObject.PlaneData();
        if (zedCam.findFloorPlane(ref plane, out estimatedPlayerHeight, Quaternion.identity, Vector3.zero) ==
            ERROR_CODE.SUCCESS)
        {
            int numVertices, numTriangles = 0;
            zedCam.convertFloorPlaneToMesh(planeMeshVertices, planeMeshTriangles, out numVertices, out numTriangles);
            if (numVertices > 0 && numTriangles > 0)
            {
                var worldPlaneVertices = new Vector3[numVertices];
                var worldPlaneTriangles = new int[numTriangles];
                TransformCameraToLocalMesh(LeftCamera.transform, planeMeshVertices, planeMeshTriangles,
                    worldPlaneVertices, worldPlaneTriangles, numVertices, numTriangles, plane.PlaneCenter);

                HasDetectedFloor = true;

                if (!floorPlaneGO)
                {
                    floorPlaneGO = new GameObject("Floor Plane");
                    floorPlaneGO.transform.SetParent(holder.transform);
                }

                //Move the gameobject to the center of the plane. Note that the plane data's center is relative to the camera. 
                floorPlaneGO.transform.position = LeftCamera.transform.position; //Add the camera's world position 
                floorPlaneGO.transform.position +=
                    LeftCamera.transform.rotation * plane.PlaneCenter; //Add the center of the plane

                if (!getFloorPlane) getFloorPlane = floorPlaneGO.AddComponent<ZEDPlaneGameObject>();

                if (!getFloorPlane.IsCreated)
                {
                    if (overrideMaterial != null)
                        getFloorPlane.Create(plane, worldPlaneVertices, worldPlaneTriangles, 0, overrideMaterial);
                    else getFloorPlane.Create(plane, worldPlaneVertices, worldPlaneTriangles, 0);
                    getFloorPlane.SetPhysics(addPhysicsOption);
                }
                else
                {
                    getFloorPlane.UpdateFloorPlane(!auto, plane, worldPlaneVertices, worldPlaneTriangles,
                        overrideMaterial);
                    getFloorPlane.SetPhysics(addPhysicsOption);
                }

                return true;
            }
        }

        return false;
    }


	/// <summary>
	///     Detects the plane around screen-space coordinates specified.
	/// </summary>
	/// <returns><c>true</c>, if plane at hit was detected, <c>false</c> otherwise.</returns>
	/// <param name="screenPos">position of the pixel in screen space (2D)</param>
	public bool DetectPlaneAtHit(Vector2 screenPos)
    {
        if (!IsReady)
            return false;

        var plane = new ZEDPlaneGameObject.PlaneData();
        if (zedCam.findPlaneAtHit(ref plane, screenPos) == ERROR_CODE.SUCCESS)
        {
            int numVertices, numTriangles = 0;
            zedCam.convertHitPlaneToMesh(planeMeshVertices, planeMeshTriangles, out numVertices, out numTriangles);
            if (numVertices > 0 && numTriangles > 0)
            {
                var newhitGO =
                    new GameObject(); //TODO: Move to proper location. (Need to rework how the mesh works first, though)
                newhitGO.transform.SetParent(holder.transform);

                var worldPlaneVertices = new Vector3[numVertices];
                var worldPlaneTriangles = new int[numTriangles];
                TransformCameraToLocalMesh(LeftCamera.transform, planeMeshVertices, planeMeshTriangles,
                    worldPlaneVertices, worldPlaneTriangles, numVertices, numTriangles, plane.PlaneCenter);

                //Move the gameobject to the center of the plane. Note that the plane data's center is relative to the camera. 
                newhitGO.transform.position = LeftCamera.transform.position; //Add the camera's world position 
                newhitGO.transform.position +=
                    LeftCamera.transform.rotation * plane.PlaneCenter; //Add the center of the plane

                var hitPlane = newhitGO.AddComponent<ZEDPlaneGameObject>();

                if (overrideMaterial != null)
                    hitPlane.Create(plane, worldPlaneVertices, worldPlaneTriangles, planeHitCount + 1,
                        overrideMaterial);
                else hitPlane.Create(plane, worldPlaneVertices, worldPlaneTriangles, planeHitCount + 1);

                hitPlane.SetPhysics(addPhysicsOption);
                hitPlane.SetVisible(isVisibleInSceneOption);
                hitPlaneList.Add(hitPlane);
                planeHitCount++;
                return true;
            }
        }

        return false;
    }


	/// <summary>
	///     Update this instance.
	/// </summary>
	private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 ScreenPosition = Input.mousePosition;
            DetectPlaneAtHit(ScreenPosition);
        }
    }

	/// <summary>
	///     Switchs the display. Set the static variable for rendering
	/// </summary>
	public void SwitchDisplay()
    {
        if (IsReady)
            isDisplay = isVisibleInGameOption;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (getFloorPlane != null && getFloorPlane.IsCreated)
        {
            getFloorPlane.SetPhysics(addPhysicsOption);
            getFloorPlane.SetVisible(isVisibleInSceneOption);
        }

        if (hitPlaneList != null)
            foreach (var c in hitPlaneList)
            {
                if (c.IsCreated)
                    c.SetPhysics(addPhysicsOption);

                c.SetVisible(isVisibleInSceneOption);
            }
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(ZEDPlaneDetectionManager))]
public class ZEDPlaneDetectionEditor : Editor
{
    // private GUILayoutOption[] optionsButtonBrowse = { GUILayout.MaxWidth(30) };

    private SerializedProperty addPhysicsOption;
    private SerializedProperty isVisibleInGameOption;
    private SerializedProperty isVisibleInSceneOption;
    private SerializedProperty overrideMaterialOption;
    private ZEDPlaneDetectionManager planeDetector;


    private ZEDPlaneDetectionManager Target => (ZEDPlaneDetectionManager) target;

    public void OnEnable()
    {
        planeDetector = (ZEDPlaneDetectionManager) target;
        addPhysicsOption = serializedObject.FindProperty("addPhysicsOption");
        isVisibleInSceneOption = serializedObject.FindProperty("isVisibleInSceneOption");
        isVisibleInGameOption = serializedObject.FindProperty("isVisibleInGameOption");
        overrideMaterialOption = serializedObject.FindProperty("overrideMaterial");
    }


    public override void OnInspectorGUI()
    {
        var cameraIsReady = ZEDCamera.GetInstance().IsCameraReady;


        serializedObject.Update();

        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Detection Parameters", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUI.enabled = cameraIsReady;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Single-shot Floor Detection");
        GUILayout.Space(20);
        if (GUILayout.Button("Detect"))
            if (planeDetector.IsReady)
                planeDetector.DetectFloorPlane(false);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Visualization", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        isVisibleInSceneOption.boolValue = EditorGUILayout.Toggle("Visible in Scene", isVisibleInSceneOption.boolValue);
        isVisibleInGameOption.boolValue = EditorGUILayout.Toggle("Visible in Game",
            isVisibleInGameOption.boolValue && isVisibleInSceneOption.boolValue);

        var overridematlabel = new GUIContent("Override Material: ",
            "Material applied to all planes if visible. If left empty, default materials will be applied depending on the plane type.");
        planeDetector.overrideMaterial = (Material) EditorGUILayout.ObjectField(overridematlabel,
            planeDetector.overrideMaterial, typeof(Material), false);


        planeDetector.SwitchDisplay();
        GUILayout.Space(20);
        GUI.enabled = true;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Physics", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        addPhysicsOption.boolValue = EditorGUILayout.Toggle("Add Collider", addPhysicsOption.boolValue);


        serializedObject.ApplyModifiedProperties();

        if (!cameraIsReady) Repaint();
    }
}


#endif
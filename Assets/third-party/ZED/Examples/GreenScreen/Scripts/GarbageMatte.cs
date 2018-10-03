//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using System.IO;
using sl;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
#if UNITY_EDITOR

#endif

/// <summary>
///     Creates a mask from position and apply it on the pipeline
/// </summary>
[RequireComponent(typeof(GreenScreenManager))]
[RequireComponent(typeof(Camera))]
public class GarbageMatte
{
    //Position in queue to make transparent object, used to render the mesh transparent
    private const int QUEUE_TRANSPARENT_VALUE = 3000;

    // Apply the garbageMatte is per default on the button "return"
    private readonly string applyButton = "return";

    // The current camera looking the scene, used to transform ScreenPosition to worldPosition
    private readonly Camera cam;

    private readonly CommandBuffer commandBuffer;
    private readonly List<GameObject> currentGOSelected = new List<GameObject>();

    private int currentPlaneIndex;

    [SerializeField] [HideInInspector] public bool editMode = true;

    [SerializeField] [HideInInspector] public string garbageMattePath = "garbageMatte.cfg";

    //List of the different gameObjects used by the mesh
    private readonly List<GameObject> go;


    private readonly List<int> indexSelected = new List<int>();

    /*** OPTIONS TO CHANGE ****/
    //At launch, if no file are found, the GarbageMatte controls are activated if true. Button Fire1 || Fire2
    private readonly bool isAbleToEdit = true;
    private bool isClosed;

    [SerializeField] [HideInInspector] public bool loadAtStart = false;

    //List of the meshes
    private readonly List<MeshFilter> meshFilters;
    private readonly List<MeshFilter> meshFilterSelected = new List<MeshFilter>();
    private int numberSpheresSelected = -1;
    private readonly Material outlineMaterial;
    private readonly List<int> planeSelectedIndex = new List<int>();

    //List of points to make the mesh
    private List<Vector3> points = new List<Vector3>();

    private readonly Material shader_greenScreen;
    private readonly int sphereLayer = 21;
    private readonly List<GameObject> spheresBorder = new List<GameObject>();
    private readonly Transform target;

    //Triangles of the current mesh
    private readonly List<int> triangles = new List<int>();

    // Reference to the ZED
    private readonly ZEDCamera zed;

    /// <summary>
    ///     Create the GarbageMatte class
    /// </summary>
    /// <param name="cam"></param>
    /// <param name="greenScreenMaterial"></param>
    /// <param name="target"></param>
    /// <param name="matte"></param>
    public GarbageMatte(Camera cam, Material greenScreenMaterial, Transform target, GarbageMatte matte)
    {
        this.target = target;
        currentPlaneIndex = 0;
        zed = ZEDCamera.GetInstance();
        this.cam = cam;
        points.Clear();

        outlineMaterial = Resources.Load("Materials/Mat_ZED_Outlined") as Material;

        go = new List<GameObject>();
        meshFilters = new List<MeshFilter>();

        shader_greenScreen = greenScreenMaterial;
        ResetPoints(false);
        if (matte != null) editMode = matte.editMode;
        if (commandBuffer == null)
        {
            //Create a command buffer to clear the depth and stencil
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "GarbageMatte";
            commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.Depth);

            //Remove the previous command buffer to set the garbage matte first
            var cmd = cam.GetCommandBuffers(CameraEvent.BeforeDepthTexture);
            cam.RemoveCommandBuffers(CameraEvent.BeforeDepthTexture);
            if (cmd.Length > 0)
            {
                cam.AddCommandBuffer(CameraEvent.BeforeDepthTexture, commandBuffer);
                for (var i = 0; i < cmd.Length; ++i) cam.AddCommandBuffer(CameraEvent.BeforeDepthTexture, cmd[i]);
            }
        }

        if (loadAtStart && Load())
        {
            Debug.Log("Config garbage matte found, and loaded ( " + garbageMattePath + " )");
            ApplyGarbageMatte();
            editMode = false;
        }

        IsInit = true;
    }

    /// <summary>
    ///     Create a dummy garbage matte, do nothing. Should be used only as a cache in memory
    /// </summary>
    public GarbageMatte()
    {
        IsInit = false;
    }

    public bool IsInit { get; }


    /// <summary>
    ///     Check if pad are ready to set the garbage matte points
    /// </summary>
    private void PadReady()
    {
        ResetPoints(false);
        if (Load())
        {
            Debug.Log("Config garbage matte found, and loaded ( " + garbageMattePath + " )");
            ApplyGarbageMatte();
            editMode = false;
        }
        else
        {
            if (isAbleToEdit) editMode = true;
        }
    }

    private void OnEnable()
    {
        ZEDSteamVRControllerManager.ZEDOnPadIndexSet += PadReady;
    }

    private void OnDisable()
    {
        ZEDSteamVRControllerManager.ZEDOnPadIndexSet -= PadReady;
    }

    /// <summary>
    ///     Update the garbage matte and manage the movement of the spheres
    /// </summary>
    public void Update()
    {
        if (editMode)
        {
            // if at least a sphere is selected
            if (numberSpheresSelected != -1)
                if (zed.IsCameraReady)
                {
                    var vec = cam.ScreenToWorldPoint(new Vector4(Input.mousePosition.x, Input.mousePosition.y,
                        zed.GetDepthValue(Input.mousePosition)));
                    // For each sphere selected move their position with the mouse
                    for (var i = 0; i < currentGOSelected.Count; ++i) currentGOSelected[i].transform.position = vec;
                }


            if (zed != null && zed.IsCameraReady)
            {
                //If left click, add a sphere
                if (Input.GetMouseButtonDown(0))
                {
                    //Add a new plan if needed
                    if (go.Count - 1 < currentPlaneIndex)
                    {
                        go.Add(CreateGameObject());

                        go[currentPlaneIndex].GetComponent<MeshRenderer>().material.renderQueue =
                            QUEUE_TRANSPARENT_VALUE + 5;
                        meshFilters[currentPlaneIndex] = go[currentPlaneIndex].GetComponent<MeshFilter>();
                        meshFilters[currentPlaneIndex].sharedMesh = CreateMesh();
                        meshFilters[currentPlaneIndex].sharedMesh.MarkDynamic();
                    }


                    if (numberSpheresSelected != -1)
                    {
                        //Remove outline from the sphere cause a sphere was selected
                        //Clear the meshes and spheres selected
                        for (var i = 0; i < currentGOSelected.Count; ++i)
                        {
                            currentGOSelected[i].GetComponent<MeshRenderer>().material.SetFloat("_Outline", 0.00f);
                            currentGOSelected[i] = null;
                        }

                        currentGOSelected.Clear();

                        for (var i = 0; i < meshFilterSelected.Count; ++i) meshFilterSelected[i].mesh.Clear();
                        meshFilterSelected.Clear();

                        //Create the planes if needed
                        for (var i = 0; i < planeSelectedIndex.Count; ++i)
                        {
                            if (spheresBorder.Count - planeSelectedIndex[i] * 4 < 4)
                            {
                                numberSpheresSelected = -1;
                                planeSelectedIndex.Clear();
                                return;
                            }

                            var triangles = new List<int>();
                            points = new List<Vector3>();
                            for (var j = planeSelectedIndex[i] * 4; j < (planeSelectedIndex[i] + 1) * 4; j++)
                                points.Add(spheresBorder[j].transform.position);

                            CloseShape(triangles, points, planeSelectedIndex[i]);
                        }

                        numberSpheresSelected = -1;
                        return;
                    }
                    // Add a sphere

                    if (points.Count < 100 && !isClosed)
                    {
                        var vec = cam.ScreenToWorldPoint(new Vector4(Input.mousePosition.x, Input.mousePosition.y,
                            zed.GetDepthValue(Input.mousePosition)));
                        RaycastHit hit;
                        if (Physics.Raycast(target.position, vec - target.position, out hit, 10, 1 << sphereLayer))
                        {
                            var hitIndex = spheresBorder.IndexOf(hit.transform.gameObject);
                            vec = spheresBorder[hitIndex].transform.position;
                        }

                        points.Add(vec);

                        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        sphere.hideFlags = HideFlags.HideInHierarchy;
                        sphere.tag = "HelpObject";
                        sphere.GetComponent<MeshRenderer>().material = outlineMaterial;
                        sphere.GetComponent<MeshRenderer>().material.SetFloat("_Outline", 0.02f);

                        sphere.transform.position = points[points.Count - 1];
                        sphere.layer = sphereLayer;
                        spheresBorder.Add(sphere);
                        if (spheresBorder.Count >= 2)
                            spheresBorder[spheresBorder.Count - 2].GetComponent<MeshRenderer>().material
                                .SetFloat("_Outline", 0.00f);

                        if (spheresBorder.Count % 4 == 0)
                        {
                            points = new List<Vector3>();
                            for (var i = currentPlaneIndex * 4; i < (currentPlaneIndex + 1) * 4; i++)
                                points.Add(spheresBorder[i].transform.position);
                            CloseShape(triangles, points, currentPlaneIndex);
                            EndPlane();
                        }
                    }
                }

                //Select the sphere, to move them
                if (Input.GetMouseButtonDown(1))
                {
                    if (numberSpheresSelected != -1) return;
                    var vec = cam.ScreenToWorldPoint(new Vector4(Input.mousePosition.x, Input.mousePosition.y,
                        zed.GetDepthValue(Input.mousePosition)));
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(target.position, vec - target.position, 10, 1 << sphereLayer);
                    if (hits.Length > 0)
                    {
                        indexSelected.Clear();
                        currentGOSelected.Clear();
                        planeSelectedIndex.Clear();
                        meshFilterSelected.Clear();

                        for (var i = 0; i < hits.Length; ++i)
                        {
                            var hitIndex = spheresBorder.IndexOf(hits[i].transform.gameObject);

                            indexSelected.Add(hitIndex);
                            currentGOSelected.Add(spheresBorder[hitIndex]);
                            planeSelectedIndex.Add(hitIndex / 4);
                            meshFilterSelected.Add(meshFilters[planeSelectedIndex[planeSelectedIndex.Count - 1]]);

                            spheresBorder[hitIndex].GetComponent<MeshRenderer>().material.SetFloat("_Outline", 0.02f);
                        }

                        numberSpheresSelected = hits.Length;
                        spheresBorder[spheresBorder.Count - 1].GetComponent<MeshRenderer>().material
                            .SetFloat("_Outline", 0.00f);
                    }
                    else
                    {
                        numberSpheresSelected = -1;
                    }
                }
            }

            //Apply the garbage matte
            if (Input.GetKeyDown(applyButton)) ApplyGarbageMatte();
        }
    }


    /// <summary>
    ///     End the current plane and increase the index of plane
    /// </summary>
    public void EndPlane()
    {
        currentPlaneIndex++;
        ResetDataCurrentPlane();
    }

    /// <summary>
    /// </summary>
    public void EnterEditMode()
    {
        if (isClosed)
        {
            foreach (var s in spheresBorder) s.SetActive(true);
            if (shader_greenScreen != null) Shader.SetGlobalInt("_ZEDStencilComp", 0);
            for (var i = 0; i < go.Count; i++)
            {
                if (go[i] == null) continue;
                go[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("alpha", 0.5f);
                go[i].GetComponent<MeshRenderer>().sharedMaterial.renderQueue = QUEUE_TRANSPARENT_VALUE + 5;
            }

            isClosed = false;
        }
    }

    public void RemoveLastPoint()
    {
        //Prevent to remove and move a sphere at the same time
        if (numberSpheresSelected != -1) return;
        if (isClosed)
        {
            foreach (var s in spheresBorder) s.SetActive(true);
            if (shader_greenScreen != null) Shader.SetGlobalInt("_ZEDStencilComp", 0);
            for (var i = 0; i < go.Count; i++)
            {
                if (go[i] == null) continue;
                go[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("alpha", 0.5f);
                go[i].GetComponent<MeshRenderer>().sharedMaterial.renderQueue = QUEUE_TRANSPARENT_VALUE + 5;
            }

            isClosed = false;
        }

        if (spheresBorder.Count % 4 == 0 && currentPlaneIndex > 0)
        {
            Object.Destroy(go[currentPlaneIndex - 1]);
            go.RemoveAll(item => item == null);
            meshFilters.RemoveAll(item => item == null);
            meshFilters[currentPlaneIndex - 1].sharedMesh.Clear();

            currentPlaneIndex--;
        }

        if (spheresBorder != null && spheresBorder.Count > 0)
        {
            Object.DestroyImmediate(spheresBorder[spheresBorder.Count - 1]);
            spheresBorder.RemoveAt(spheresBorder.Count - 1);
            if (spheresBorder.Count % 4 == 0 && spheresBorder.Count > 0)
                spheresBorder[spheresBorder.Count - 1].GetComponent<MeshRenderer>().material
                    .SetFloat("_Outline", 0.02f);
        }
    }

    private void ResetDataCurrentPlane()
    {
        points.Clear();
        triangles.Clear();
    }

    public void CleanSpheres()
    {
        var remain_sphere2 = GameObject.FindGameObjectsWithTag("HelpObject");
        if (remain_sphere2.Length > 0)
            foreach (var sph in remain_sphere2)
                Object.DestroyImmediate(sph);
    }

    public void ResetPoints(bool cleansphere)
    {
        if (cleansphere)
        {
            var remain_sphere2 = GameObject.FindGameObjectsWithTag("HelpObject");
            if (remain_sphere2.Length > 0)
                foreach (var sph in remain_sphere2)
                    Object.Destroy(sph);
        }

        Shader.SetGlobalInt("_ZEDStencilComp", 0);

        if (go == null) return;
        isClosed = false;

        currentPlaneIndex = 0;
        for (var i = 0; i < go.Count; i++) Object.DestroyImmediate(go[i]);
        go.Clear();
        meshFilters.Clear();
        ResetDataCurrentPlane();
        if (spheresBorder != null)
            foreach (var s in spheresBorder)
                Object.DestroyImmediate(s);
        spheresBorder.Clear();
        if (commandBuffer != null) commandBuffer.Clear();

        points.Clear();
    }

    /// <summary>
    ///     Get orientation
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p"></param>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <returns></returns>
    private static int Orientation(Vector3 p1, Vector3 p2, Vector3 p, Vector3 X, Vector3 Y)
    {
        return (Vector3.Dot(p2, X) - Vector3.Dot(p1, X)) * (Vector3.Dot(p, Y) - Vector3.Dot(p1, Y)) -
               (Vector3.Dot(p, X) - Vector3.Dot(p1, X)) * (Vector3.Dot(p2, Y) - Vector3.Dot(p1, Y)) > 0
            ? 1
            : 0;
    }

    /// <summary>
    ///     Ordering the points to draw a mesh
    /// </summary>
    /// <returns></returns>
    private List<int> OrderPoints(List<Vector3> points)
    {
        var normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]);
        normal.Normalize();
        var X = new Vector3(-normal.y, normal.x, 0);
        X.Normalize();
        var Y = Vector3.Cross(X, normal);
        Y.Normalize();

        var ordoredIndex = new List<int>();

        var convexHull = new List<Vector3>();
        var minX = Vector3.Dot(points[0], X);
        var p = points[0];

        for (var i = 0; i < points.Count; i++)
            if (Vector3.Dot(points[i], X) < minX)
            {
                minX = Vector3.Dot(points[i], X);
                p = points[i];
            }

        Vector3 currentTestPoint;

        for (var i = 0; i < 4; i++)
        {
            convexHull.Add(p);
            ordoredIndex.Add(points.IndexOf(p));
            currentTestPoint = points[0];
            for (var j = 0; j < points.Count; j++)
                if (currentTestPoint == p || Orientation(p, currentTestPoint, points[j], X, Y) == 1)
                    currentTestPoint = points[j];
            p = currentTestPoint;
        }

        return ordoredIndex;
    }

    /// <summary>
    ///     Draw the last quad
    /// </summary>
    public void CloseShape(List<int> triangles, List<Vector3> points, int currentPlaneIndex)
    {
        triangles.Clear();
        var indexOrder = OrderPoints(points);

        triangles.Add(indexOrder[0]);
        triangles.Add(indexOrder[1]);
        triangles.Add(indexOrder[2]);
        triangles.Add(indexOrder[0]);
        triangles.Add(indexOrder[2]);
        triangles.Add(indexOrder[3]);

        if (go[currentPlaneIndex] == null)
        {
            go[currentPlaneIndex] = CreateGameObject();
            meshFilters[currentPlaneIndex] = go[currentPlaneIndex].GetComponent<MeshFilter>();
            meshFilters[currentPlaneIndex].sharedMesh = CreateMesh();
            meshFilters[currentPlaneIndex].sharedMesh.MarkDynamic();
        }

        go[currentPlaneIndex].GetComponent<MeshFilter>().sharedMesh.Clear();
        go[currentPlaneIndex].GetComponent<MeshFilter>().sharedMesh.vertices = points.ToArray();
        go[currentPlaneIndex].GetComponent<MeshFilter>().sharedMesh.triangles = triangles.ToArray();

        spheresBorder[spheresBorder.Count - 1].GetComponent<MeshRenderer>().material.SetFloat("_Outline", 0.00f);
    }

    /// <summary>
    ///     Apply the garbage matte by rendering into the stencil buffer
    /// </summary>
    public void ApplyGarbageMatte()
    {
        if (currentPlaneIndex <= 0)
        {
            editMode = false;
            ResetPoints(false);
            return;
        }

        if (shader_greenScreen != null)
        {
            isClosed = true;
            foreach (var s in spheresBorder) s.SetActive(false);
            Shader.SetGlobalInt("_ZEDStencilComp", 3);
            for (var i = 0; i < go.Count; i++)
            {
                if (go[i] == null) continue;
                go[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("alpha", 0.0f);
                go[i].GetComponent<MeshRenderer>().sharedMaterial.renderQueue = QUEUE_TRANSPARENT_VALUE - 5;
            }
        }

        commandBuffer.Clear();
        for (var i = 0; i < go.Count; ++i)
            if (go[i] != null)
                commandBuffer.DrawMesh(go[i].GetComponent<MeshFilter>().mesh, go[i].transform.localToWorldMatrix,
                    go[i].GetComponent<Renderer>().material);
        editMode = false;
    }

    private void OnApplicationQuit()
    {
        ResetPoints(true);
    }

    private GameObject CreateGameObject()
    {
        var plane = new GameObject("PlaneTest");
        plane.hideFlags = HideFlags.HideInHierarchy;
        meshFilters.Add((MeshFilter) plane.AddComponent(typeof(MeshFilter)));

        var renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.sharedMaterial = new Material(Resources.Load("Materials/Mat_ZED_Mask_Quad") as Material);
        renderer.sharedMaterial.SetFloat("alpha", 0.5f);
        return plane;
    }

    private Mesh CreateMesh()
    {
        var m = new Mesh();
        m.name = "ScriptedMesh";

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        return m;
    }

    public GarbageMatteData RegisterData()
    {
        var garbageMatteData = new GarbageMatteData();

        if (meshFilters == null) return garbageMatteData;
        garbageMatteData.numberMeshes = meshFilters.Count;
        garbageMatteData.planes = new List<Plane>();
        for (var i = 0; i < meshFilters.Count; i++)
        {
            var vertices = meshFilters[i].mesh.vertices;
            var p = new Plane();
            p.numberVertices = vertices.Length;
            p.vertices = new List<Vector3>(vertices);
            garbageMatteData.planes.Add(p);
            //garbageMatteData.plane.ad
        }

        return garbageMatteData;
    }


    public bool LoadData(GarbageMatteData garbageMatteData)
    {
        var nbMesh = garbageMatteData.numberMeshes;
        if (nbMesh < 0) return false;
        currentPlaneIndex = 0;
        ResetPoints(false);

        for (var i = 0; i < nbMesh; i++)
        {
            points.Clear();
            triangles.Clear();
            go.Add(CreateGameObject());
            go[currentPlaneIndex].GetComponent<MeshRenderer>().material.renderQueue = QUEUE_TRANSPARENT_VALUE + 5;
            meshFilters[currentPlaneIndex] = go[currentPlaneIndex].GetComponent<MeshFilter>();
            meshFilters[currentPlaneIndex].sharedMesh = CreateMesh();
            meshFilters[currentPlaneIndex].sharedMesh.MarkDynamic();
            var p = garbageMatteData.planes[i];
            for (var j = 0; j < p.numberVertices; j++)
            {
                points.Add(p.vertices[j]);
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                sphere.tag = "HelpObject";
                sphere.hideFlags = HideFlags.HideInHierarchy;
                outlineMaterial.SetFloat("_Outline", 0.00f);
                sphere.GetComponent<MeshRenderer>().material = outlineMaterial;

                sphere.transform.position = points[points.Count - 1];
                sphere.layer = sphereLayer;
                spheresBorder.Add(sphere);
            }

            if (go.Count == 0) return false;

            CloseShape(triangles, points, currentPlaneIndex);
            EndPlane();
        }

        return true;
    }

    /// <summary>
    ///     Save the points into a file
    /// </summary>
    public void Save()
    {
        var meshes = new List<string>();
        meshes.Add((meshFilters.Count - 1).ToString());

        for (var i = 0; i < meshFilters.Count - 1; i++)
        {
            var vertices = meshFilters[i].mesh.vertices;
            var tri = meshFilters[i].mesh.triangles;
            meshes.Add("v#" + vertices.Length);
            for (var j = 0; j < vertices.Length; j++)
                meshes.Add(vertices[j].x + " " + vertices[j].y + " " + vertices[j].z);
        }

        File.WriteAllLines(garbageMattePath, meshes.ToArray());
    }

    /// <summary>
    ///     Load the current shape
    /// </summary>
    public bool Load()
    {
        if (!File.Exists(garbageMattePath)) return false;
        var meshes = File.ReadAllLines(garbageMattePath);
        if (meshes == null) return false;
        var nbMesh = int.Parse(meshes[0]);
        if (nbMesh < 0) return false;
        currentPlaneIndex = 0;
        ResetPoints(false);
        var lineCount = 1;
        string[] splittedLine;
        for (var i = 0; i < nbMesh; i++)
        {
            points.Clear();
            triangles.Clear();
            go.Add(CreateGameObject());
            go[currentPlaneIndex].GetComponent<MeshRenderer>().material.renderQueue = QUEUE_TRANSPARENT_VALUE + 5;
            meshFilters[currentPlaneIndex] = go[currentPlaneIndex].GetComponent<MeshFilter>();
            meshFilters[currentPlaneIndex].sharedMesh = CreateMesh();
            meshFilters[currentPlaneIndex].sharedMesh.MarkDynamic();
            splittedLine = meshes[lineCount].Split('#');
            lineCount++;
            var nbVertices = int.Parse(splittedLine[1]);
            for (var j = 0; j < nbVertices; j++)
            {
                splittedLine = meshes[lineCount].Split(' ');
                lineCount++;
                float x = float.Parse(splittedLine[0]),
                    y = float.Parse(splittedLine[1]),
                    z = float.Parse(splittedLine[2]);
                points.Add(new Vector3(x, y, z));
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                sphere.hideFlags = HideFlags.HideInHierarchy;
                sphere.tag = "HelpObject";
                sphere.transform.position = points[points.Count - 1];
                sphere.layer = sphereLayer;
                spheresBorder.Add(sphere);
            }

            if (go.Count == 0) return false;

            CloseShape(triangles, points, currentPlaneIndex);
            EndPlane();
        }

        return true;
    }

    [Serializable]
    public struct Plane
    {
        public int numberVertices;
        public List<Vector3> vertices;
    }

    [Serializable]
    public struct GarbageMatteData
    {
        public int numberMeshes;
        public List<Plane> planes;
    }
}
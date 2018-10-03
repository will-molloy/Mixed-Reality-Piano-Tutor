//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using sl;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

/// <summary>
///     Process the mesh taken from the ZED
/// </summary>
public class ZEDSpatialMapping
{
    /// <summary>
    ///     Eevents called when a new mesh had been processed. Is called many times.
    /// </summary>
    public delegate void OnNewMesh();

    /// <summary>
    ///     Event called at the end of the spatial mapping
    /// </summary>
    public delegate void OnSpatialMappingEnded();

    /// <summary>
    ///     Event called when the spatial mapping has started
    /// </summary>
    public delegate void OnSpatialMappingStarted();

    /// <summary>
    ///     List the spatial mapping depth range presets.
    /// </summary>
    public enum RANGE
    {
        /// <summary>
        ///     Only depth close to the camera will be used by the spatial mapping.
        /// </summary>
        NEAR,

        /// <summary>
        ///     Medium depth range.
        /// </summary>
        MEDIUM,

        /// <summary>
        ///     Takes into account objects that are far, useful outdoor.
        /// </summary>
        FAR
    }


    /// <summary>
    ///     List the spatial mapping depth resolution presets.
    /// </summary>
    public enum RESOLUTION
    {
        /// <summary>
        ///     Create a detail geometry, requires lots of memory.
        /// </summary>
        HIGH,

        /// <summary>
        ///     Smalls variations in the geometry will disappear, useful for big object.
        /// </summary>
        MEDIUM,

        /// <summary>
        ///     Keeps only huge variations of the geometry , useful outdoor.
        /// </summary>
        LOW
    }

    /// <summary>
    ///     Max time in ms during the process of the mesh can be done, if the time is over,  the flag remainMeshes is set to
    ///     true
    /// </summary>
    private const int MAX_TIME = 5;

    /// <summary>
    ///     Checks if the display of the mesh is wanted
    /// </summary>
    public static bool display;

    /// <summary>
    ///     List final of meshes
    /// </summary>
    public List<Chunk> ChunkList = new List<Chunk>();
#if UNITY_EDITOR
    private readonly Color colorMesh = new Color(0.35f, 0.65f, 0.95f);
#endif


    public FILTER filterParameters = FILTER.MEDIUM;

    /// <summary>
    ///     Thread to filter
    /// </summary>
    private Thread filterThread;

    /// <summary>
    ///     Gravity estimation, set after the spatial mapping
    /// </summary>
    public Vector3 gravityEstimation;

    /// <summary>
    ///     Are colliders wanted
    /// </summary>
    private readonly bool hasColliders = true;

    /// <summary>
    ///     Chunks holder
    /// </summary>
    private readonly GameObject holder;

    /// <summary>
    ///     Index of the mesh during last frame
    /// </summary>
    private int indexLastFrame;

    /// <summary>
    ///     The mesh is in the state of filtering
    /// </summary>
    private bool isFiltering;

    /// <summary>
    ///     Flag checking if the filtering is over
    /// </summary>
    private bool isFilteringOver;

    /// <summary>
    ///     Checks if the mesh is textured
    /// </summary>
    private bool isTextured;

    private readonly object lockScanning = new object();

    private readonly ZEDMeshRenderer[] meshRenderer = new ZEDMeshRenderer[2];

    /**** Threading Variables ****/
    /// <summary>
    ///     The mesh has been updated, and needs to be processed
    /// </summary>
    private bool meshUpdated;

    /// <summary>
    ///     Flag to set a pause
    /// </summary>
    private bool pause;

    /// <summary>
    ///     Flag if remains meshes to process (due to lack of time)
    /// </summary>
    private bool remainMeshes;

    /// <summary>
    ///     Checks if the thread is running, and if the spatial mapping is running
    /// </summary>
    private bool running;

    /// <summary>
    ///     State of the scanning during it's init
    /// </summary>
    private ERROR_CODE scanningInitState;

    /// <summary>
    ///     Thread retrieving the size of the meshes
    /// </summary>
    private Thread scanningThread;

    // Use this for initialization
    private bool setMeshRenderer;

    /// <summary>
    ///     The spatial mapping should start
    /// </summary>
    private bool spatialMappingRequested;

    /// <summary>
    ///     The update of the thread is over
    /// </summary>
    private bool stopRunning;

    /// <summary>
    ///     Stop has been wanted by the user
    /// </summary>
    private bool stopWanted;

    /// <summary>
    ///     Offset for the triangles buffer
    /// </summary>
    private int trianglesOffsetLastFrame;

    /// <summary>
    ///     Thetexture had been udpated
    /// </summary>
    private bool updatedTexture;

    /// <summary>
    ///     Needs an update of a texture
    /// </summary>
    private bool updateTexture;

    /// <summary>
    ///     The thread is running
    /// </summary>
    private bool updateThreadRunning;

    /// <summary>
    ///     Offset for the uvs buffer
    /// </summary>
    private int uvsOffsetLastFrame;

    /// <summary>
    ///     Offset for the vertices buffer
    /// </summary>
    private int verticesOffsetLastFrame;


    /// <summary>
    ///     Current instance of the ZED Camera
    /// </summary>
    private readonly ZEDCamera zedCamera;

    private readonly ZEDManager zedManager;

    /// <summary>
    ///     Internal class used to help
    /// </summary>
    private readonly ZEDSpatialMappingHelper zedSpatialMapping;


    public ZEDSpatialMapping(Transform transform, ZEDCamera zedCamera, ZEDManager zedManager)
    {
        zedSpatialMapping = new ZEDSpatialMappingHelper(
            Resources.Load("Materials/SpatialMapping/Mat_ZED_Texture") as Material,
            Resources.Load("Materials/SpatialMapping/Mat_ZED_Geometry_Wireframe") as Material);

        this.zedCamera = zedCamera;
        this.zedManager = zedManager;
        scanningInitState = ERROR_CODE.FAILURE;

        holder = new GameObject();
        holder.name = "[ZED Mesh Holder]";
        //holder.hideFlags = HideFlags.HideInInspector;
        holder.transform.position = Vector3.zero;
        holder.transform.rotation = Quaternion.identity;
        StaticBatchingUtility.Combine(holder);
    }

    /// <summary>
    ///     Returns the state of the pause
    /// </summary>
    public bool IsPaused => pause;

    public bool IsUpdateThreadRunning => updateThreadRunning;

    /// <summary>
    ///     Is tetxuring is running
    /// </summary>
    public bool IsTexturingRunning { get; private set; }

    /// <summary>
    ///     The meshes with their indices. This dictionnay is updated only when running the spatial Mapping. Prefer using
    ///     SubMeshesList to get the complete list of meshes if spatial mapping is stopped
    /// </summary>
    public Dictionary<int, Chunk> Chunks => zedSpatialMapping.chunks;

    public static event OnNewMesh OnMeshUpdate;
    public static event OnSpatialMappingEnded OnMeshReady;
    public static event OnSpatialMappingStarted OnMeshStarted;


    /// <summary>
    ///     Starts the spatial mapping
    /// </summary>
    /// <param name="resolutionPreset"></param>
    /// <param name="rangePreset"></param>
    /// <param name="isTextured"></param>
    public void StartStatialMapping(RESOLUTION resolutionPreset, RANGE rangePreset, bool isTextured)
    {
        holder.transform.position = Vector3.zero;
        holder.transform.rotation = Quaternion.identity;
        spatialMappingRequested = true;
        if (spatialMappingRequested && scanningInitState != ERROR_CODE.SUCCESS)
            scanningInitState = EnableSpatialMapping(resolutionPreset, rangePreset, isTextured);

        zedManager.gravityRotation = Quaternion.identity;
    }

    /// <summary>
    ///     Enable the spatial mapping
    /// </summary>
    /// <param name="resolutionPreset"> A preset resolution</param>
    /// <param name="rangePreset">A preset range</param>
    /// <param name="isTextured">if true, images will be collected during the process</param>
    /// <returns></returns>
    private ERROR_CODE EnableSpatialMapping(RESOLUTION resolutionPreset, RANGE rangePreset, bool isTextured)
    {
        ERROR_CODE error;
        this.isTextured = isTextured;
        error = zedSpatialMapping.EnableSpatialMapping(
            ZEDSpatialMappingHelper.ConvertResolutionPreset(resolutionPreset),
            ZEDSpatialMappingHelper.ConvertRangePreset(rangePreset), isTextured);
        ZEDMeshRenderer.isTextured = false;
        stopWanted = false;
        running = true;

        if (error == ERROR_CODE.SUCCESS)
        {
            display = true;
            meshUpdated = false;
            spatialMappingRequested = false;
            updateTexture = false;
            updatedTexture = false;

            // clear all prevous meshes
            ClearMeshes();

            //start the requesting meshes
            zedCamera.RequestMesh();

            //Launch the thread to retrieve the chunks and their sizes
            scanningThread = new Thread(UpdateMesh);
            updateThreadRunning = true;
            if (OnMeshStarted != null) OnMeshStarted();
            scanningThread.Start();
        }

        return error;
    }

    /// <summary>
    ///     Set the mesh renderer to the cameras. Is necessary to see the mesh
    /// </summary>
    public void SetMeshRenderer()
    {
        if (!setMeshRenderer)
            if (zedManager != null)
            {
                var left = zedManager.GetLeftCameraTransform();

                if (left != null)
                {
                    meshRenderer[0] = left.gameObject.GetComponent<ZEDMeshRenderer>();
                    if (!meshRenderer[0]) meshRenderer[0] = left.gameObject.AddComponent<ZEDMeshRenderer>();
                }

                var right = zedManager.GetRightCameraTransform();
                if (right != null)
                {
                    meshRenderer[1] = right.gameObject.GetComponent<ZEDMeshRenderer>();
                    if (!meshRenderer[1]) meshRenderer[1] = right.gameObject.AddComponent<ZEDMeshRenderer>();
                }

                setMeshRenderer = true;
            }
    }

    /// <summary>
    ///     Updates the current mesh and manages the start and stop states
    /// </summary>
    public void Update()
    {
        SetMeshRenderer();

        if (meshUpdated || remainMeshes)
        {
            UpdateMeshMainthread();
            meshUpdated = false;
        }

        if (stopWanted && !remainMeshes)
        {
            stopRunning = true;
            stopWanted = false;
            Stop();
        }

        //Disable the spatial mapping and rotate the parent of the ZEDManager to apply the gravity estimation
        if (stopRunning && !isFiltering && isFilteringOver)
        {
            isFilteringOver = false;
            UpdateMeshMainthread(false);
            var disabling = new Thread(DisableSpatialMapping);
            disabling.Start();
            if (hasColliders)
            {
                if (!ZEDManager.IsStereoRig && gravityEstimation != Vector3.zero && zedManager.transform.parent != null)
                {
                    var rotationToApplyForGravity =
                        Quaternion.Inverse(Quaternion.FromToRotation(Vector3.up, -gravityEstimation.normalized));
                    holder.transform.localRotation = rotationToApplyForGravity;

                    zedManager.gravityRotation = rotationToApplyForGravity;
                }

                UpdateMeshCollider();
            }
            else
            {
                running = false;
            }

            stopRunning = false;
        }
    }


    /// <summary>
    ///     Get the mesh data from the ZED camera and store it for later update in unity mesh.
    /// </summary>
    private void UpdateMesh()
    {
        while (updateThreadRunning)
            //If all the index are not over
            if (!remainMeshes)
            {
                lock (lockScanning)
                {
                    if (meshUpdated == false && updateTexture)
                    {
                        //Get the last size of mesh and get the texture Size
                        zedSpatialMapping.ApplyTexture();
                        meshUpdated = true;
                        updateTexture = false;
                        updatedTexture = true;
                        updateThreadRunning = false;
                    }
                    else if (zedCamera.GetMeshRequestStatus() == ERROR_CODE.SUCCESS && !pause && meshUpdated == false)
                    {
                        zedSpatialMapping.UpdateMesh();
                        zedSpatialMapping.RetrieveMesh();
                        meshUpdated = true;
                    }
                }

                //Time to process all the meshes spread on multiple frames
                Thread.Sleep(5);
            }
            else
            {
                //If remain meshes it checks every 5ms
                Thread.Sleep(5);
            }
    }

    /// <summary>
    ///     Destroy all submesh.
    /// </summary>
    private void ClearMeshes()
    {
        foreach (Transform child in holder.transform) Object.Destroy(child.gameObject);
        zedSpatialMapping.Clear();
    }

    /// <summary>
    ///     Measure time. If the computational time is over the limit, all the meshes not processed will be done in the next
    ///     frame
    /// </summary>
    /// <param name="startTimeMS"></param>
    /// <returns></returns>
    private bool GoneOverTimeBudget(int startTimeMS)
    {
        return Time.realtimeSinceStartup * 1000 - startTimeMS > MAX_TIME;
    }


    /// <summary>
    ///     Update the unity mesh with the last data retrieved from the ZED, create new submesh if needed.
    ///     Launch the OnMeshUpdate event when the update is done.
    /// </summary>
    private void UpdateMeshMainthread(bool spreadUpdateOverTime = true)
    {
        var startTimeMS = (int) (Time.realtimeSinceStartup * 1000);
        var indexUpdate = 0;
        lock (lockScanning)
        {
            if (updatedTexture) spreadUpdateOverTime = false;
            //Set the offset of the buffers to the offset of the last frame
            int verticesOffset = 0, trianglesOffset = 0, uvsOffset = 0;
            if (remainMeshes && spreadUpdateOverTime)
            {
                verticesOffset = verticesOffsetLastFrame;
                trianglesOffset = trianglesOffsetLastFrame;
                uvsOffset = uvsOffsetLastFrame;
                indexUpdate = indexLastFrame;
            }

            //Clear all the mesh and process the last ones
            if (updatedTexture)
            {
                ClearMeshes();
                zedSpatialMapping.SetMeshAndTexture();
                ZEDMeshRenderer.isTextured = isTextured;
            }
            //Process the last meshes

            for (; indexUpdate < zedSpatialMapping.NumberUpdatedSubMesh; indexUpdate++)
            {
                zedSpatialMapping.SetMesh(indexUpdate, ref verticesOffset, ref trianglesOffset, ref uvsOffset,
                    holder.transform, updatedTexture);
                if (spreadUpdateOverTime && GoneOverTimeBudget(startTimeMS))
                {
                    remainMeshes = true;
                    break;
                }
            }

            if (spreadUpdateOverTime)
                indexLastFrame = indexUpdate;
            else
                indexLastFrame = 0;

            //If all the meshes are processed
            if (indexUpdate == zedSpatialMapping.NumberUpdatedSubMesh || zedSpatialMapping.NumberUpdatedSubMesh == 0)
            {
                verticesOffsetLastFrame = 0;
                trianglesOffsetLastFrame = 0;
                uvsOffsetLastFrame = 0;
                indexLastFrame = 0;
                remainMeshes = false;
                meshUpdated = false;
                zedCamera.RequestMesh();
            }
            //If it remains meshes, we save the offsets
            else if (indexUpdate != zedSpatialMapping.NumberUpdatedSubMesh)
            {
                remainMeshes = true;
                indexLastFrame = indexUpdate + 1;

                verticesOffsetLastFrame = verticesOffset;
                trianglesOffsetLastFrame = trianglesOffset;
                uvsOffsetLastFrame = uvsOffset;
            }
        }


        if (OnMeshUpdate != null) OnMeshUpdate();

        //The update texture is done in one pass
        if (updatedTexture)
        {
            DisableSpatialMapping();
            updatedTexture = false;
            running = false;
            IsTexturingRunning = false;
            if (hasColliders)
                UpdateMeshCollider();
            else
                running = false;
        }
    }

    /// <summary>
    ///     Display or hide the mesh.
    /// </summary>
    /// <param name="newDisplayState"> If true, the mesh will be displayed, else it will be hide. </param>
    public void SwitchDisplayMeshState(bool newDisplayState)
    {
        display = newDisplayState;
    }

    /// <summary>
    ///     Pause or resume the spatial mapping. If the spatial mapping is not enable, nothing will happend.
    /// </summary>
    /// <param name="newPauseState"> If true, the spatial mapping will be paused, else it will be resumed. </param>
    public void SwitchPauseState(bool newPauseState)
    {
        pause = newPauseState;
        zedCamera.PauseSpatialMapping(newPauseState);
    }

    /// <summary>
    ///     Update the mesh collider with the current mesh so it can handle physic.
    ///     Calling it is time consuming.
    /// </summary>
    public void UpdateMeshCollider(bool timeSlicing = false)
    {
        ChunkList.Clear();
        foreach (var submesh in Chunks) ChunkList.Add(submesh.Value);
        lock (lockScanning)
        {
            zedSpatialMapping.UpdateMeshCollider(ChunkList);
        }

        if (OnMeshReady != null) OnMeshReady();
        running = false;
    }

    public void Dispose()
    {
        if (scanningThread != null)
        {
            updateThreadRunning = false;
            scanningThread.Join();
        }

        ClearMeshes();
        Object.Destroy(holder);
        DisableSpatialMapping();
    }

    /// <summary>
    ///     Disable the ZED's spatial mapping.
    ///     The mesh will no longer be updated, but it is not delete.
    /// </summary>
    private void DisableSpatialMapping()
    {
        lock (lockScanning)
        {
            updateThreadRunning = false;
            zedSpatialMapping.DisableSpatialMapping();
            scanningInitState = ERROR_CODE.FAILURE;
            spatialMappingRequested = false;
        }
    }

    /// <summary>
    ///     Save the mesh as an obj file and the area database as an area file. It can be quite time comsuming if you mapped a
    ///     large area.
    /// </summary>
    public bool SaveMesh(string meshFilePath = "Assets/ZEDMesh.obj")
    {
        var err = false;

        if (updateThreadRunning) StopStatialMapping();
        lock (lockScanning)
        {
            var splitedPath = meshFilePath.Split('.');
            if (splitedPath[splitedPath.Length - 1].Contains("obj"))
                err = zedCamera.SaveMesh(meshFilePath, MESH_FILE_FORMAT.OBJ);
            else if (splitedPath[splitedPath.Length - 1].Contains("ply"))
                err = zedCamera.SaveMesh(meshFilePath, MESH_FILE_FORMAT.BIN);
            else
                err = zedCamera.SaveMesh(meshFilePath + ".obj", MESH_FILE_FORMAT.OBJ);
        }

        var areaName = meshFilePath.Substring(0, meshFilePath.LastIndexOf(".")) + ".area";

        zedCamera.SaveCurrentArea(areaName);
        return err;
    }

    /// <summary>
    ///     Load the mesh and the corresponding area file if it exists. It can be quite time comsuming if you mapped a large
    ///     area.
    ///     If there are no areas found, the mesh will not be loaded
    /// </summary>
    public bool LoadMesh(string meshFilePath = "ZEDMesh.obj")
    {
        if (OnMeshStarted != null) OnMeshStarted();
        //If a spatail mapping have started, disable it
        DisableSpatialMapping();

        //Find and load the area
        var basePath = meshFilePath.Substring(0, meshFilePath.LastIndexOf("."));
        if (!File.Exists(basePath + ".area"))
        {
            Debug.LogWarning(ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.TRACKING_BASE_AREA_NOT_FOUND));
            return false;
        }

        zedCamera.DisableTracking();
        var quat = Quaternion.identity;
        var tr = Vector3.zero;
        if (zedCamera.EnableTracking(ref quat, ref tr, true, false,
                File.Exists(basePath + ".area") ? basePath + ".area" : "") != ERROR_CODE.SUCCESS)
        {
            Debug.LogWarning(ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.TRACKING_NOT_INITIALIZED));
            return false;
        }

        updateTexture = false;
        updatedTexture = false;
        var meshUpdatedLoad = false;
        lock (lockScanning)
        {
            ClearMeshes();
            meshUpdatedLoad = zedSpatialMapping.LoadMesh(meshFilePath);

            if (meshUpdatedLoad)
            {
                //Checks if a texture exists
                if (zedSpatialMapping.GetWidthTexture() != -1)
                {
                    updateTexture = true;
                    updatedTexture = true;
                }

                //Retrieves the mesh sizes to be updated in the unity's buffer later
                if (!updateTexture) zedSpatialMapping.RetrieveMesh();
            }
        }


        if (meshUpdatedLoad)
        {
            //Update the buffer on the unity's side
            UpdateMeshMainthread(false);

            //Add colliders and scan for gravity
            if (hasColliders)
            {
                if (!ZEDManager.IsStereoRig && gravityEstimation != Vector3.zero)
                {
                    var rotationToApplyForGravity =
                        Quaternion.Inverse(Quaternion.FromToRotation(Vector3.up, -gravityEstimation.normalized));
                    holder.transform.rotation = rotationToApplyForGravity;
                    zedManager.gravityRotation = rotationToApplyForGravity;
                }

                UpdateMeshCollider();
                foreach (var c in ChunkList) c.o.transform.localRotation = Quaternion.identity;
            }

            if (OnMeshReady != null) OnMeshReady();

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Filters the mesh with the current parameter.
    /// </summary>
    public void FilterMesh()
    {
        lock (lockScanning)
        {
            zedSpatialMapping.FilterMesh(filterParameters);
            zedSpatialMapping.ResizeMesh();
            zedSpatialMapping.RetrieveMesh();
            meshUpdated = true;
        }
    }

    private void PostProcessMesh(bool filter = true)
    {
        isFiltering = true;
        if (filter) FilterMesh();
        MergeChunks();
    }

    /// <summary>
    ///     Reshape the mesh to get less chunks
    /// </summary>
    public void MergeChunks()
    {
        lock (lockScanning)
        {
            zedSpatialMapping.MergeChunks();
            zedSpatialMapping.ResizeMesh();
            zedSpatialMapping.RetrieveMesh();
            meshUpdated = true;
        }

        isFiltering = false;
        isFilteringOver = true;
    }

    private void ApplyTextureThreaded()
    {
        FilterMesh();
        UpdateMesh();
    }


    /// <summary>
    ///     Start the texturing of the mesh, this will stop the spatial mapping.
    /// </summary>
    public bool ApplyTexture()
    {
        updateTexture = true;
        if (updateThreadRunning)
        {
            updateThreadRunning = false;
            scanningThread.Join();
        }

        scanningThread = new Thread(ApplyTextureThreaded);
        updateThreadRunning = true;
        scanningThread.Start();
        IsTexturingRunning = true;
        return true;
    }

    /// <summary>
    ///     Stop the spatial mapping
    /// </summary>
    private void Stop()
    {
        gravityEstimation = zedCamera.GetGravityEstimate();

        if (isTextured)
        {
            ApplyTexture();
        }
        else
        {
            stopRunning = false;
            if (updateThreadRunning)
            {
                updateThreadRunning = false;
                scanningThread.Join();
            }

            ClearMeshes();
            filterThread = new Thread(() => PostProcessMesh(true));
            filterThread.Start();
            stopRunning = true;
        }

        SwitchDisplayMeshState(true);
    }

    /// <summary>
    ///     Returns true from the enable tracking until the post-process is done.
    /// </summary>
    /// <returns></returns>
    public bool IsRunning()
    {
        return running;
    }

    /// <summary>
    ///     Stop the spatial mapping (filter the mesh, disable the spatial mapping and update the mesh collider). The stop will
    ///     occured after all the post-process.
    /// </summary>
    public void StopStatialMapping()
    {
        stopWanted = true;
    }

    //To draw the meshes in the editor with a double pass shader
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = colorMesh;


        if (!IsRunning() && zedSpatialMapping != null && zedSpatialMapping.chunks.Count != 0 && display)
            foreach (var submesh in zedSpatialMapping.chunks)
                if (submesh.Value.proceduralMesh.mesh != null)
                    Gizmos.DrawWireMesh(submesh.Value.proceduralMesh.mesh.sharedMesh,
                        submesh.Value.o.transform.position, submesh.Value.o.transform.rotation);
    }
#endif

    /// <summary>
    ///     Submesh created by the ZEDSpatialMapping
    /// </summary>
    public struct Chunk
    {
        /// <summary>
        ///     Contains the gameobject
        /// </summary>
        public GameObject o;

        /// <summary>
        ///     Dynamic mesh, will change during the spatial mapping
        /// </summary>
        public ProceduralMesh proceduralMesh;

        /// <summary>
        ///     Final mesh, should be used at the end of the spatial mapping
        /// </summary>
        public Mesh mesh;
    }

    /// <summary>
    ///     Structure to contain a temporary buffer in change for triangles and vertices
    /// </summary>
    public struct ProceduralMesh
    {
        /// <summary>
        ///     List of indices
        /// </summary>
        public int[] triangles;

        /// <summary>
        ///     List of vertices
        /// </summary>
        public Vector3[] vertices;

        /// <summary>
        ///     Interface mesh for Unity
        /// </summary>
        public MeshFilter mesh;
    }


    /// <summary>
    ///     Low level spatial mapping class.
    /// </summary>
    private class ZEDSpatialMappingHelper
    {
        /// <summary>
        ///     Number of vertices per chunk. Best value to get few chunks and to update them quickly
        /// </summary>
        private const int MAX_SUBMESH = 10000;


        public readonly Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>(MAX_SUBMESH);
        private readonly Material materialMesh;
        private readonly Material materialTexture;
        private int numTriangles;
        private readonly int[] numTrianglesInSubmesh = new int[MAX_SUBMESH];
        private int numUpdatedSubmesh;

        /*** Number of vertices/uvs/indices at the moment**/
        private int numVertices;

        /*** Number of vertices/uvs/indices per chunk***/
        private readonly int[] numVerticesInSubmesh = new int[MAX_SUBMESH];
        private readonly int[] texturesSize = new int[2];
        private int[] triangles;
        private readonly int[] UpdatedIndices = new int[MAX_SUBMESH];
        private Vector2[] uvs;

        /*** The current data in the current submesh***/
        private Vector3[] vertices;

        private readonly ZEDCamera zedCamera;

        public ZEDSpatialMappingHelper(Material materialTexture, Material materialMesh)
        {
            zedCamera = ZEDCamera.GetInstance();
            this.materialTexture = materialTexture;
            this.materialMesh = materialMesh;
        }

        /// <summary>
        ///     Number of chunks updated
        /// </summary>
        public int NumberUpdatedSubMesh => numUpdatedSubmesh;

        /// <summary>
        ///     Gets the spatial mapping material to draw
        /// </summary>
        /// <returns></returns>
        public Material GetMaterialSpatialMapping()
        {
            return materialMesh;
        }

        /// <summary>
        ///     Updates the range and resolution to match the specified preset.
        /// </summary>
        public static float ConvertRangePreset(RANGE rangePreset)
        {
            if (rangePreset == RANGE.NEAR)
                return 3.5f;
            if (rangePreset == RANGE.MEDIUM) return 5.0f;
            if (rangePreset == RANGE.FAR) return 10.0f;
            return 5.0f;
        }

        /// <summary>
        ///     Update the range and resolution to match the specified preset.
        /// </summary>
        public static float ConvertResolutionPreset(RESOLUTION resolutionPreset)
        {
            if (resolutionPreset == RESOLUTION.HIGH)
                return 0.05f;
            if (resolutionPreset == RESOLUTION.MEDIUM) return 0.10f;
            if (resolutionPreset == RESOLUTION.LOW) return 0.15f;
            return 0.10f;
        }

        /// <summary>
        ///     Enables the spatial mapping
        /// </summary>
        /// <returns></returns>
        public ERROR_CODE EnableSpatialMapping(float resolutionMeter, float maxRangeMeter, bool saveTexture)
        {
            return zedCamera.EnableSpatialMapping(resolutionMeter, maxRangeMeter, saveTexture);
        }

        /// <summary>
        ///     Disable the spatial mapping
        /// </summary>
        public void DisableSpatialMapping()
        {
            zedCamera.DisableSpatialMapping();
        }

        /// <summary>
        ///     Create a new submesh to contain the data retrieved from the ZED.
        /// </summary>
        public Chunk CreateNewMesh(int i, Material meshMat, Transform holder)
        {
            //The chunk
            var chunk = new Chunk();
            chunk.o = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //subMesh.o.hideFlags = HideFlags.HideAndDontSave;
            chunk.o.layer = ZEDCamera.TagOneObject;
            chunk.o.GetComponent<MeshCollider>().sharedMesh = null;
            chunk.o.name = "Chunk" + chunks.Count;
            chunk.o.transform.localPosition = Vector3.zero;
            chunk.o.transform.localRotation = Quaternion.identity;

            var m = new Mesh();
            m.MarkDynamic();
            chunk.mesh = m;

            //Set the options of the mesh (no shadows, no reflections, no lights)
            var meshRenderer = chunk.o.GetComponent<MeshRenderer>();
            meshRenderer.material = meshMat;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.enabled = true;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            //Sets the positions and parent of the chunk
            chunk.o.transform.parent = holder;
            chunk.o.layer = ZEDCamera.TagOneObject;

            //Add the chunk to the list
            chunk.proceduralMesh.mesh = chunk.o.GetComponent<MeshFilter>();
            chunks.Add(i, chunk);
            return chunk;
        }

        /// <summary>
        ///     Set the mesh collider to each chunck
        /// </summary>
        public void UpdateMeshCollider(List<Chunk> listMeshes, int startIndex = 0)
        {
            var idsToDestroy = new List<int>();
            //Update each mesh with a collider
            for (var i = startIndex; i < listMeshes.Count; ++i)
            {
                var submesh = listMeshes[i];
                var m = submesh.o.GetComponent<MeshCollider>();

                if (m == null) m = submesh.o.AddComponent<MeshCollider>();
                //If a mesh is degenerated it should be destroyed
                var tempMesh = submesh.o.GetComponent<MeshFilter>().sharedMesh;
                if (tempMesh.vertexCount < 3)
                {
                    idsToDestroy.Add(i);
                    continue;
                }

                m.sharedMesh = tempMesh;

                m.sharedMesh.RecalculateNormals();
                m.sharedMesh.RecalculateBounds();
            }

            //Delay id destruction
            for (var i = 0; i < idsToDestroy.Count; ++i)
            {
                Object.Destroy(chunks[idsToDestroy[i]].o);
                chunks.Remove(idsToDestroy[i]);
            }

            Clear();
        }

        /// <summary>
        ///     Refresh the size of the texture and prepare the uvs
        /// </summary>
        public void ApplyTexture()
        {
            zedCamera.ApplyTexture(numVerticesInSubmesh, numTrianglesInSubmesh, ref numUpdatedSubmesh, UpdatedIndices,
                ref numVertices, ref numTriangles, texturesSize, MAX_SUBMESH);
        }

        /// <summary>
        ///     Update the mesh
        /// </summary>
        public void UpdateMesh()
        {
            zedCamera.UpdateMesh(numVerticesInSubmesh, numTrianglesInSubmesh, ref numUpdatedSubmesh, UpdatedIndices,
                ref numVertices, ref numTriangles, MAX_SUBMESH);
            ResizeMesh();
        }

        /// <summary>
        ///     Retrieve the mesh vertices and triangles
        /// </summary>
        public void RetrieveMesh()
        {
            zedCamera.RetrieveMesh(vertices, triangles, MAX_SUBMESH, null, IntPtr.Zero);
        }


        /// <summary>
        ///     Clear all the current data
        /// </summary>
        public void Clear()
        {
            chunks.Clear();

            vertices = new Vector3[0];
            triangles = new int[0];
            uvs = new Vector2[0];

            Array.Clear(vertices, 0, vertices.Length);
            Array.Clear(triangles, 0, triangles.Length);
        }

        /// <summary>
        ///     Set the mesh
        /// </summary>
        /// <param name="indexUpdate">Index which is going to be updated</param>
        /// <param name="verticesOffset">Starting index in the vertices stack</param>
        /// <param name="trianglesOffset">Starting index in the triangles stack</param>
        /// <param name="uvsOffset">Starting index in the uvs index</param>
        /// <param name="meshMat"></param>
        /// <param name="display"></param>
        /// <param name="transform"></param>
        /// <param name="material"></param>
        /// <param name="updatedTex"></param>
        public void SetMesh(int indexUpdate, ref int verticesOffset, ref int trianglesOffset, ref int uvsOffset,
            Transform holder, bool updatedTex)
        {
            Chunk subMesh;
            var updatedIndex = UpdatedIndices[indexUpdate];
            if (!chunks.TryGetValue(updatedIndex, out subMesh))
                subMesh = CreateNewMesh(updatedIndex, materialMesh, holder);

            var currentMesh = subMesh.mesh;
            var dynamicMesh = subMesh.proceduralMesh;
            //Check if allocated, if the size is the same do not allocate
            if (dynamicMesh.triangles == null || dynamicMesh.triangles.Length != 3 * numTrianglesInSubmesh[indexUpdate])
                dynamicMesh.triangles = new int[3 * numTrianglesInSubmesh[indexUpdate]];
            if (dynamicMesh.vertices == null || dynamicMesh.vertices.Length != numVerticesInSubmesh[indexUpdate])
                dynamicMesh.vertices = new Vector3[numVerticesInSubmesh[indexUpdate]];

            //Clear the old data
            currentMesh.Clear();

            //Copy the new data
            Array.Copy(vertices, verticesOffset, dynamicMesh.vertices, 0, numVerticesInSubmesh[indexUpdate]);
            verticesOffset += numVerticesInSubmesh[indexUpdate];
            Buffer.BlockCopy(triangles, trianglesOffset * sizeof(int), dynamicMesh.triangles, 0,
                3 * numTrianglesInSubmesh[indexUpdate] * sizeof(int)); //Block copy has better performance than Array
            trianglesOffset += 3 * numTrianglesInSubmesh[indexUpdate];
            currentMesh.vertices = dynamicMesh.vertices;
            currentMesh.SetTriangles(dynamicMesh.triangles, 0, false);

            dynamicMesh.mesh.sharedMesh = currentMesh;

            //If texture add UVS
            if (updatedTex)
            {
                var localUvs = new Vector2[numVerticesInSubmesh[indexUpdate]];
                subMesh.o.GetComponent<MeshRenderer>().sharedMaterial = materialTexture;
                Array.Copy(uvs, uvsOffset, localUvs, 0, numVerticesInSubmesh[indexUpdate]);
                uvsOffset += numVerticesInSubmesh[indexUpdate];
                currentMesh.uv = localUvs;
            }
        }

        /// <summary>
        ///     Set the texture and mesh (vertices, triangles, and uvs)
        /// </summary>
        public void SetMeshAndTexture()
        {
            //If the texture is too large, impossible to add a texture to the mesh
            if (texturesSize[0] > 8192) return;

            var textureMesh = new Texture2D(texturesSize[0], texturesSize[1], TextureFormat.RGB24, false);

            if (textureMesh != null)
            {
                var texture = textureMesh.GetNativeTexturePtr();
                materialTexture.SetTexture("_MainTex", textureMesh);

                vertices = new Vector3[numVertices];
                uvs = new Vector2[numVertices];
                triangles = new int[3 * numTriangles];

                zedCamera.RetrieveMesh(vertices, triangles, MAX_SUBMESH, uvs, texture);
            }
        }

        /// <summary>
        ///     Load the mesh from a file path and allocate the buffers
        /// </summary>
        /// <param name="meshFilePath"></param>
        /// <returns></returns>
        public bool LoadMesh(string meshFilePath)
        {
            var r = zedCamera.LoadMesh(meshFilePath, numVerticesInSubmesh, numTrianglesInSubmesh, ref numUpdatedSubmesh,
                UpdatedIndices, ref numVertices, ref numTriangles, MAX_SUBMESH, texturesSize);
            vertices = new Vector3[numVertices];
            uvs = new Vector2[numVertices];
            triangles = new int[3 * numTriangles];
            return r;
        }

        /// <summary>
        ///     Get the texture width, if this one is over 8k, the texture will not be taken.
        /// </summary>
        /// <returns></returns>
        public int GetWidthTexture()
        {
            return texturesSize[0];
        }

        /// <summary>
        ///     Resize the mesh buffer
        /// </summary>
        public void ResizeMesh()
        {
            if (vertices.Length < numVertices)
                vertices = new Vector3[numVertices * 2]; // Better allocate than resize, faster

            if (triangles.Length < 3 * numTriangles) triangles = new int[3 * numTriangles * 2];
        }

        /// <summary>
        ///     Filter a mesh with predefined parameters
        /// </summary>
        /// <param name="filterParameters"></param>
        public void FilterMesh(FILTER filterParameters)
        {
            zedCamera.FilterMesh(filterParameters, numVerticesInSubmesh, numTrianglesInSubmesh, ref numUpdatedSubmesh,
                UpdatedIndices, ref numVertices, ref numTriangles, MAX_SUBMESH);
        }

        /// <summary>
        ///     Reshape the chunks to get less
        /// </summary>
        public void MergeChunks()
        {
            zedCamera.MergeChunks(MAX_SUBMESH, numVerticesInSubmesh, numTrianglesInSubmesh, ref numUpdatedSubmesh,
                UpdatedIndices, ref numVertices, ref numTriangles, MAX_SUBMESH);
        }
    }
}
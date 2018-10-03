//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
///     Creates and draws the NavMesh
/// </summary>
[RequireComponent(typeof(ZEDSpatialMappingManager))]
public class NavMeshSurface : MonoBehaviour
{
    [HideInInspector] [SerializeField] public bool hideFlag;

    [HideInInspector] [SerializeField] public bool isOver;

    /// <summary>
    ///     Navmesh object created
    /// </summary>
    private GameObject navMeshObject;

    private Vector3 navMeshPosition;

    /// <summary>
    ///     Reference to the spatial mapping
    /// </summary>
    private ZEDSpatialMappingManager zedSpatialMapping;

    public Vector3 NavMeshPosition => navMeshObject != null ? navMeshPosition : Vector3.zero;

    /// <summary>
    ///     Event when the navMesh is ready
    /// </summary>
    public static event EventHandler<PositionEventArgs> OnNavMeshReady;

    // Use this for initialization
    private void Start()
    {
        zedSpatialMapping = GetComponent<ZEDSpatialMappingManager>();

#if UNITY_5_6_OR_NEWER
        navMesh = new NavMeshData();
        NavMesh.AddNavMeshData(navMesh);

        //Initialize a position of the bound
        bounds = new Bounds(transform.position, new Vector3(30, 30, 30));
        materialTransparent = Resources.Load("Materials/Mat_ZED_Transparent_NavMesh") as Material;
#endif
    }

    private void OnEnable()
    {
        ZEDSpatialMapping.OnMeshReady += MeshIsOver;
        ZEDSpatialMapping.OnMeshStarted += NewNavMesh;
    }

    private void NewNavMesh()
    {
        if (navMeshObject != null) Destroy(navMeshObject);
    }

    private void OnDisable()
    {
        ZEDSpatialMapping.OnMeshReady -= MeshIsOver;
        ZEDSpatialMapping.OnMeshStarted -= NewNavMesh;
    }

    private void MeshIsOver()
    {
#if UNITY_5_6_OR_NEWER
        UpdateNavMesh();

        //Nav mesh has been build. Clear the sources as we don't need them
        sources.Clear();

        //Draw the nav mesh is possible (triangulation from navmesh has return an area) 
        var isDrawn = Draw();

#endif
        if (isDrawn)
        {
            var args = new PositionEventArgs();
            args.position = NavMeshPosition;
            args.valid = isDrawn;
            args.agentTypeID = agentTypeID;
            var handler = OnNavMeshReady;
            if (handler != null) handler(this, args);
        }
        else
        {
            Debug.LogWarning(ZEDLogMessage.Error2Str(ZEDLogMessage.ERROR.NAVMESH_NOT_GENERATED));
        }

        isOver = true;
    }

    /// <summary>
    ///     Draws the navMesh
    /// </summary>
    /// <returns></returns>
    private bool Draw()
    {
        var triangulation = NavMesh.CalculateTriangulation();

        if (triangulation.areas.Length == 0)
            return false;


        if (navMeshObject == null) navMeshObject = new GameObject("NavMesh");
        navMeshObject.transform.parent = transform;


        var meshFilter = navMeshObject.GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = navMeshObject.AddComponent<MeshFilter>();
        meshFilter.mesh.Clear();
        meshFilter.mesh.vertices = triangulation.vertices;

        meshFilter.mesh.triangles = triangulation.indices;
        meshFilter.mesh.RecalculateNormals();
        var r = navMeshObject.GetComponent<MeshRenderer>();
        if (!r) r = navMeshObject.AddComponent<MeshRenderer>();
        r.sharedMaterial = materialTransparent;
        navMeshPosition = r.bounds.center;
        navMeshObject.SetActive(hideFlag);
        return true;
    }

    /// <summary>
    ///     Collect all the submeshes to build the Nav Mesh
    /// </summary>
    private void CollectSources()
    {
#if UNITY_5_6_OR_NEWER
        sources.Clear();

        foreach (var o in zedSpatialMapping.ChunkList)
        {
            var m = o.o.GetComponent<MeshFilter>();
            if (m != null)
            {
                var s = new NavMeshBuildSource();
                s.shape = NavMeshBuildSourceShape.Mesh;
                s.sourceObject = m.mesh;
                s.transform = o.o.transform.localToWorldMatrix;
                s.area = agentTypeID;
                sources.Add(s);
            }
        }

#endif
    }


    /// <summary>
    ///     Calculates the bounds once sources have been collected and inflate them
    /// </summary>
    /// <param name="sources">Sources.</param>
    private void CalculateBounds(List<NavMeshBuildSource> sources)
    {
        // Use the unscaled matrix for the NavMeshSurface
        if (sources.Count != 0)
        {
            bounds.center = transform.position;

            //For each src, grows the bounds
            foreach (var src in sources)
            {
                var m = src.sourceObject as Mesh;
                bounds.Encapsulate(m.bounds);
            }
        }

        // Inflate the bounds a bit to avoid clipping co-planar sources
        bounds.Expand(0.1f);
    }

    /// <summary>
    ///     Collect the meshes and construct a nav mesh
    /// </summary>
    private void UpdateNavMesh()
    {
        isOver = false;

        // First collect all the sources (submeshes)
        CollectSources();

#if UNITY_5_6_OR_NEWER
        if (sources.Count != 0)
        {
            // adjust bounds
            CalculateBounds(sources);

            // update the nav mesh with sources and bounds
            var defaultBuildSettings = NavMesh.GetSettingsByID(agentTypeID);
            NavMeshBuilder.UpdateNavMeshData(navMesh, defaultBuildSettings, sources, bounds);
        }
#endif
    }

#if UNITY_5_6_OR_NEWER
    /// <summary>
    ///     Hide or display the navmesh drawn
    /// </summary>
    public void SwitchStateDisplayNavMesh()
    {
        hideFlag = !hideFlag;
        if (navMeshObject != null) navMeshObject.SetActive(hideFlag);
    }
#endif

    private void OnApplicationQuit()
    {
        Destroy(navMeshObject);
    }

    public class PositionEventArgs : EventArgs
    {
        public int agentTypeID;
        public Vector3 position;
        public bool valid;
    }


#if UNITY_5_6_OR_NEWER
    /// <summary>
    ///     List of sources built from meshes
    /// </summary>
    private readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

    /// <summary>
    ///     Final nav mesh
    /// </summary>
    private NavMeshData navMesh;

    /// <summary>
    ///     Bounds of the mesh, it is computed at the collect of mesh
    /// </summary>
    private Bounds bounds;

    [SerializeField] public int agentTypeID;

    private Material materialTransparent;
#endif
}
#if UNITY_5_6_OR_NEWER
#if UNITY_EDITOR

[CustomEditor(typeof(NavMeshSurface))]
internal class ZEDNavMeshEditor : Editor
{
    private SerializedProperty agentID;
    private SerializedProperty hideFlag;
    private SerializedProperty isOver;

    private ZEDNavMeshEditor()
    {
    }

    private void OnEnable()
    {
        agentID = serializedObject.FindProperty("agentTypeID");
        hideFlag = serializedObject.FindProperty("hideFlag");
        isOver = serializedObject.FindProperty("isOver");
    }

    public static void AgentTypePopup(string labelName, SerializedProperty agentTypeID)
    {
        var index = -1;
        var count = NavMesh.GetSettingsCount();
        var agentTypeNames = new string[count + 2];
        for (var i = 0; i < count; i++)
        {
            var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            var name = NavMesh.GetSettingsNameFromID(id);
            agentTypeNames[i] = name;
            if (id == agentTypeID.intValue)
                index = i;
        }

        agentTypeNames[count] = "";
        agentTypeNames[count + 1] = "Open Agent Settings...";

        var validAgentType = index != -1;
        if (!validAgentType) EditorGUILayout.HelpBox("Agent Type invalid.", MessageType.Warning);

        var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginProperty(rect, GUIContent.none, agentTypeID);

        EditorGUI.BeginChangeCheck();
        index = EditorGUI.Popup(rect, labelName, index, agentTypeNames);
        if (EditorGUI.EndChangeCheck())
        {
            if (index >= 0 && index < count)
            {
                var id = NavMesh.GetSettingsByIndex(index).agentTypeID;
                agentTypeID.intValue = id;
            }
            else if (index == count + 1)
            {
                NavMeshEditorHelpers.OpenAgentSettings(-1);
            }
        }

        EditorGUI.EndProperty();
    }

    public override void OnInspectorGUI()
    {
        var obj = (NavMeshSurface) target;
        serializedObject.Update();
        AgentTypePopup("Agent Type", agentID);

        GUI.enabled = isOver.boolValue;
        if (GUILayout.Button(hideFlag.boolValue ? "Hide" : "Display")) obj.SwitchStateDisplayNavMesh();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endif
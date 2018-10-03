using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(RenderEffect))]
public class RenderEffectInspector : Editor
{
    public bool m_EnableSortLayer;
    private int[] m_LayerID;
    private string[] m_LayerName;

    private void OnEnable()
    {
        m_LayerName = GetSortingLayerNames();
        m_LayerID = GetSortingLayerUniqueIDs();
    }

    public string[] GetSortingLayerNames()
    {
        var internalEditorUtilityType = typeof(InternalEditorUtility);
        var sortingLayersProperty =
            internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        return (string[]) sortingLayersProperty.GetValue(null, new object[0]);
    }

    // Get the unique sorting layer IDs -- tossed this in for good measure
    public int[] GetSortingLayerUniqueIDs()
    {
        var internalEditorUtilityType = typeof(InternalEditorUtility);
        var sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",
            BindingFlags.Static | BindingFlags.NonPublic);
        return (int[]) sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
    }

    public override void OnInspectorGUI()
    {
        var renderEffect = target as RenderEffect;
        var particleSystem = renderEffect.gameObject.GetComponent<ParticleSystem>();
        EditorGUILayout.BeginVertical();


        if (particleSystem == null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Open BillBoardOption");
            renderEffect.m_EnableBillBoard = EditorGUILayout.Toggle(renderEffect.m_EnableBillBoard);
            EditorGUILayout.EndHorizontal();

            if (renderEffect.m_EnableBillBoard)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("BillBoard Type");
                renderEffect.m_BillBoardType =
                    (RenderBillBoardType) EditorGUILayout.EnumPopup(renderEffect.m_BillBoardType);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }

        if (particleSystem == null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Open Sort Layer Option");
            renderEffect.m_EnableSetSortLayer = EditorGUILayout.Toggle(renderEffect.m_EnableSetSortLayer);
            EditorGUILayout.EndHorizontal();
            if (renderEffect.m_EnableSetSortLayer)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sorting Layer");
                renderEffect.m_SortingLayerID =
                    EditorGUILayout.IntPopup(renderEffect.m_SortingLayerID, m_LayerName, m_LayerID);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sorting Order");
                renderEffect.m_SortingOrder = EditorGUILayout.IntField(renderEffect.m_SortingOrder);
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck()) renderEffect.UpdateRenderLayer();
                EditorGUI.indentLevel--;
            }
        }

        var render = renderEffect.gameObject.GetComponent<Renderer>();
        if (render != null)
        {
            if (GUILayout.Button("Refresh Material")) renderEffect.RefreshMaterial();
            EditorGUILayout.LabelField("Materials");
        }

        EditorGUI.indentLevel++;
        var index = 0;
        foreach (var matEffect in renderEffect.m_MaterialEffects)
        {
            var strIndex = "Element:" + index + "    ";
            if (matEffect.m_EffectMaterial == null)
            {
                GUILayout.Button(strIndex + "Material Not Assign");
                index++;
            }
            else
            {
                if (GUILayout.Button(strIndex + matEffect.m_EffectMaterial.name))
                    matEffect.m_EditorExtend = !matEffect.m_EditorExtend;
                index++;
                if (matEffect.m_EditorExtend)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Main Texture WrapMode");
                    matEffect.m_MainTexWrapMode =
                        (TextureWrapMode) EditorGUILayout.EnumPopup(matEffect.m_MainTexWrapMode);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Cutoff Texture WrapMode");
                    matEffect.m_MaskTexWrapMode =
                        (TextureWrapMode) EditorGUILayout.EnumPopup(matEffect.m_MaskTexWrapMode);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUI.indentLevel--;
        if (render != null && particleSystem == null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Switch Render Type");
            if (render.GetType() != typeof(LineRenderer))
                if (GUILayout.Button("Switch To Line Render"))
                {
                    var lineRender = renderEffect.gameObject.AddComponent<LineRenderer>();
                    renderEffect.m_Render = lineRender;
                    lineRender.sharedMaterials = render.sharedMaterials;
                    DestroyImmediate(render);
                    var meshFilter = renderEffect.gameObject.GetComponent<MeshFilter>();
                    if (meshFilter != null) DestroyImmediate(meshFilter);
                    var meshCollider = renderEffect.gameObject.GetComponent<Collider>();
                    if (meshCollider != null) DestroyImmediate(meshCollider);
                    GUIUtility.ExitGUI();
                }

            if (render.GetType() != typeof(MeshRenderer))
                if (GUILayout.Button("Switch To Mesh Render"))
                {
                    var lineRender = renderEffect.gameObject.AddComponent<MeshRenderer>();
                    lineRender.sharedMaterials = render.sharedMaterials;
                    renderEffect.m_Render = lineRender;
                    DestroyImmediate(render);
                    var meshFilter = renderEffect.gameObject.GetComponent<MeshFilter>();
                    if (meshFilter == null) renderEffect.gameObject.AddComponent<MeshFilter>();
                    var Collider = renderEffect.gameObject.GetComponent<Collider>();
                    if (Collider != null) DestroyImmediate(Collider);
                    var meshCollider = renderEffect.gameObject.GetComponent<MeshCollider>();
                    if (meshCollider == null) renderEffect.gameObject.AddComponent<MeshCollider>();
                    GUIUtility.ExitGUI();
                }

            if (render.GetType() != typeof(TrailRenderer))
                if (GUILayout.Button("Switch To Trail Render"))
                {
                    var lineRender = renderEffect.gameObject.AddComponent<TrailRenderer>();
                    lineRender.sharedMaterials = render.sharedMaterials;
                    renderEffect.m_Render = lineRender;
                    DestroyImmediate(render);
                    var meshFilter = renderEffect.gameObject.GetComponent<MeshFilter>();
                    if (meshFilter != null) DestroyImmediate(meshFilter);
                    var Collider = renderEffect.gameObject.GetComponent<Collider>();
                    if (Collider != null) DestroyImmediate(Collider);
                    GUIUtility.ExitGUI();
                }

            if (render.GetType() == typeof(TrailRenderer))
                if (GUILayout.Button("Clear Trail"))
                {
                    var trailRender = render.GetComponent<TrailRenderer>();
                    trailRender.Clear();
                }

            EditorGUI.indentLevel--;
        }


        EditorGUILayout.EndVertical();
    }
}
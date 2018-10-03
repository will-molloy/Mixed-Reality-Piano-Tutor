using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
#endif

public enum RenderBillBoardType
{
    Normal,
    Horizontal,
    Vertical
}

[Serializable]
public class MaterialEffect
{
    public float m_AlphaAnimationTimeScale = 1.0f;
    public AnimationCurve m_AlphaCurve = new AnimationCurve();
#if UNITY_EDITOR
    public bool m_EditorExtend;
#endif
    public Material m_EffectMaterial;
    public bool m_EnableAlphaAnimation;

    public bool m_EnableUVScroll;

    //public bool enableSetTextureWrapMode = true;
    public Texture m_MainTexture;
    public TextureWrapMode m_MainTexWrapMode;
    public Texture m_MaskTexutre;
    public TextureWrapMode m_MaskTexWrapMode;
    public Vector2 m_UVScrollCutTex;
    public Vector2 m_UVScrollMainTex;

    public MaterialEffect(Material material)
    {
    }

    public void ReInitMaterial(Material material)
    {
        if (material == null)
            return;
        m_EffectMaterial = material;
        //effectMaterial.renderQueue += renderSeqFix;
        if (material.HasProperty(EffectShaderPropertyStr.MainTexStr))
            m_MainTexture = material.GetTexture(EffectShaderPropertyStr.MainTexStr);
        if (material.HasProperty(EffectShaderPropertyStr.CutTexStr))
            m_MaskTexutre = material.GetTexture(EffectShaderPropertyStr.CutTexStr);
    }

    public void UpdateEffect(float execueTime)
    {
        if (m_MainTexture != null && m_MainTexWrapMode != m_MainTexture.wrapMode)
            m_MainTexture.wrapMode = m_MainTexWrapMode;
        if (m_MaskTexutre != null && m_MaskTexWrapMode != m_MaskTexutre.wrapMode)
            m_MaskTexutre.wrapMode = m_MaskTexWrapMode;
        if (m_EnableUVScroll)
        {
            if (m_MainTexture)
                m_EffectMaterial.SetTextureOffset(EffectShaderPropertyStr.MainTexStr, m_UVScrollMainTex * execueTime);
            if (m_MaskTexutre)
                m_EffectMaterial.SetTextureOffset(EffectShaderPropertyStr.CutTexStr, m_UVScrollCutTex * execueTime);
        }
    }

    private void SetAlpha(float value)
    {
        var color = m_EffectMaterial.color;
        color.a = value;
        m_EffectMaterial.color = color;
    }
}

[ExecuteInEditMode]
//[RequireComponent(typeof(Renderer))]
public class RenderEffect : MonoBehaviour
{
    public RenderBillBoardType m_BillBoardType;
    public bool m_EnableBillBoard;
    public bool m_EnableSetSortLayer = true;
    public List<MaterialEffect> m_MaterialEffects = new List<MaterialEffect>();
    private Camera m_ReferenceCamera;
    public Renderer m_Render;

    [HideInInspector] public int m_SortingLayerID;

    [HideInInspector] public int m_SortingOrder;

    private float m_TimeLine;

    private void Awake()
    {
        m_ReferenceCamera = Camera.main;
        m_Render = GetComponent<Renderer>();
        if (m_Render == null)
            return;
    }

    private void OnEnable()
    {
        RefreshMaterial();
    }

    public void UpdateRenderLayer()
    {
        if (m_EnableSetSortLayer)
        {
            m_Render.sortingLayerID = m_SortingLayerID;
            m_Render.sortingOrder = m_SortingOrder;
        }
    }

    public void RefreshMaterial()
    {
        if (m_Render == null)
        {
            m_Render = GetComponent<Renderer>();
            if (m_Render == null)
                return;
        }

        var i = 0;
        for (i = 0; i < m_Render.sharedMaterials.Length; i++)
            if (m_MaterialEffects.Count <= i)
            {
                var matEffect = new MaterialEffect(m_Render.sharedMaterials[i]);
                m_MaterialEffects.Add(matEffect);
            }
            else
            {
                m_MaterialEffects[i].ReInitMaterial(m_Render.sharedMaterials[i]);
            }

        for (var j = m_MaterialEffects.Count - 1; i <= j; j--) m_MaterialEffects.RemoveAt(j);
        UpdateRenderLayer();
    }

    private void UpdateBillBoard()
    {
        if (m_EnableBillBoard == false)
            return;
        if (m_ReferenceCamera == null)
            m_ReferenceCamera = Camera.main;
        if (m_BillBoardType == RenderBillBoardType.Normal)
        {
            var targetPos = transform.position + m_ReferenceCamera.transform.rotation * Vector3.forward;
            var targetOrientation = m_ReferenceCamera.transform.rotation * Vector3.up;
            transform.LookAt(targetPos, targetOrientation);
        }
        else if (m_BillBoardType == RenderBillBoardType.Vertical)
        {
            var v = m_ReferenceCamera.transform.forward;
            v.y = 0;
            transform.rotation = Quaternion.LookRotation(v, Vector3.up);
        }
        else if (m_BillBoardType == RenderBillBoardType.Horizontal)
        {
            var targetPos = transform.position + m_ReferenceCamera.transform.rotation * Vector3.down;
            var targetOrientation = m_ReferenceCamera.transform.rotation * Vector3.up;
            transform.LookAt(targetPos, targetOrientation);
            var rotation = transform.rotation.eulerAngles;
            rotation.x = 90.0f;
            transform.rotation = Quaternion.Euler(rotation);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        m_TimeLine += Time.deltaTime;
        foreach (var matEffect in m_MaterialEffects) matEffect.UpdateEffect(m_TimeLine);
    }

    private void LateUpdate()
    {
        UpdateBillBoard();
    }

    public void Sim(float timer)
    {
        UpdateBillBoard();
        foreach (var matEffect in m_MaterialEffects) matEffect.UpdateEffect(timer);
    }
}
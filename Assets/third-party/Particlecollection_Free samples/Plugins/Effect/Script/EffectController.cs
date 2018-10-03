using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectData
{
    public bool m_bFoldoutOpen = true;

    public bool m_bSortingFoldout = true;

    public bool m_bTransformFoldout = true;

    public float m_fTimeSec;
    public GameObject m_goEffect;
    public Vector3 m_goPos = new Vector3(0, 0, 0);
    public Vector3 m_goRotation = new Vector3(0, 0, 0);
    public Vector3 m_goScale = new Vector3(1, 1, 1);
    public int m_SortingLayerID;
    public int m_SortingOrder;
}

public class EffectController : MonoBehaviour
{
    ///< 特效數量.
    public bool m_bLockNums;

    ///< 特效數量鎖定.
    public List<EffectData> m_kEffectGenList = new List<EffectData>();

    ///< 特效設定清單.
    private int m_nNowIndex;

    public int m_nNumOfEffects;

    private void Awake()
    {
        for (var i = 0; i < m_kEffectGenList.Count; i++) Invoke("GenEffect", m_kEffectGenList[i].m_fTimeSec);

        var comparer = new Comp(); ///< 時間Comparer.
        m_kEffectGenList.Sort(comparer); ///< 依時間排序.
    }

    private void Update()
    {
        CheckTransfromUpdate();
    }

	/// <summary>
	///     特效生成.
	/// </summary>
	private void GenEffect()
    {
        var effectData = m_kEffectGenList[m_nNowIndex];
        if (effectData == null)
            return;

        if (effectData.m_goEffect != null)
        {
            var go = Instantiate(effectData.m_goEffect);
            go.transform.parent = transform;
            go.name = m_nNowIndex.ToString(); ///< 上編號.
            UpdateEffectTransformByIndex(m_nNowIndex);
            UPdateRenderLayerByIndex(m_nNowIndex);
        }

        m_nNowIndex++;
    }

	/// <summary>
	///     原生功能更改值.
	/// </summary>
	private void CheckTransfromUpdate()
    {
        foreach (Transform tf in transform)
        {
            var nIndex = int.Parse(tf.name);
            var effectData = m_kEffectGenList[nIndex];
            if (effectData == null)
                return;

            if (tf.position != effectData.m_goPos)
                effectData.m_goPos = tf.position;
            if (tf.localRotation.eulerAngles != effectData.m_goRotation)
                effectData.m_goRotation = tf.localRotation.eulerAngles;
            if (tf.localScale != effectData.m_goScale)
                effectData.m_goScale = tf.localScale;
        }
    }

	/// <summary>
	///     更新對應編號特效之Transform數值.
	/// </summary>
	/// <param name="nIndex">特效編號.</param>
	public void UpdateEffectTransformByIndex(int nIndex)
    {
        /// 取得特效資料.
        var tf = transform.Find(nIndex.ToString());
        if (tf == null)
            return;
        var effectData = m_kEffectGenList[nIndex];
        if (effectData == null)
            return;

        /// 設定特效物件Transform.
        tf.position = effectData.m_goPos;
        var effectObjRotation = new Quaternion();
        effectObjRotation.eulerAngles = effectData.m_goRotation;
        tf.localRotation = effectObjRotation;
        tf.localScale = effectData.m_goScale;
    }

	/// <summary>
	///     檢查對應編號特效是否含有粒子系統.
	/// </summary>
	/// <returns><c>true</c>,有Particle System, <c>false</c> 沒article System.</returns>
	/// <param name="nIndex">特效編號.</param>
	public ParticleSystem CheckHasParticleSystem(int nIndex)
    {
        /// 取得特效物件.
        var tf = transform.Find(nIndex.ToString());
        if (tf == null)
            return null;

        /// 取得粒子系統.
        var particleSystem = tf.gameObject.GetComponent<ParticleSystem>();
        return particleSystem;
    }

	/// <summary>
	///     檢查對應編號特效是否使用RenderEffect.
	/// </summary>
	/// <returns>RenderEffect元件.</returns>
	/// <param name="nIndex">特效編號.</param>
	public RenderEffect CheckHasRenderEffectScript(int nIndex)
    {
        /// 取得特效物件.
        var tf = transform.Find(nIndex.ToString());
        if (tf == null)
            return null;

        /// 取得RenderEffect元件.
        var renderEffect = tf.gameObject.GetComponent<RenderEffect>();
        return renderEffect;
    }

	/// <summary>
	///     更新對應編號特效物件Render Layer.
	/// </summary>
	/// <param name="nIndex">特效編號.</param>
	public void UPdateRenderLayerByIndex(int nIndex)
    {
        /// 取得特效物件.
        var tf = transform.Find(nIndex.ToString());
        if (tf == null)
            return;
        var effectData = m_kEffectGenList[nIndex];
        if (effectData == null)
            return;

        /// Render Layer 更新.
        var render = tf.gameObject.GetComponent<Renderer>();
        render.sortingLayerID = effectData.m_SortingLayerID;
        render.sortingOrder = effectData.m_SortingOrder;
    }
}

/// <summary>
///     Effect Data Time comparer.
/// </summary>
public class Comp : IComparer<EffectData>
{
    public int Compare(EffectData x, EffectData y)
    {
        if (x == null)
        {
            if (y == null)
                return 0;
            return 1;
        }

        if (y == null) return -1;

        float fDiff = x.m_fTimeSec.CompareTo(y.m_fTimeSec);
        if (fDiff > 0)
            return 1;
        if (fDiff < 0)
            return -1;
        return 0;
    }
}
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class EffectDemo : MonoBehaviour
{
    public const string EFFECT_ASSET_PATH = "Assets/Prefab/";
    public List<GameObject> m_EffectPrefabList = new List<GameObject>();
    public bool m_LookAtEffect = true;
    private string m_NowEffectName;
    private int m_NowIndex;

    private GameObject m_NowShowEffect;

    // Use this for initialization
    private void Awake()
    {
#if (UNITY_EDITOR_WIN && !UNITY_WEBPLAYER)
        m_EffectPrefabList.Clear();
        var aPrefabFiles = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        foreach (var prefabFile in aPrefabFiles)
        {
            var assetPath = "Assets" + prefabFile.Replace(Application.dataPath, "").Replace('\\', '/');
            if (assetPath.Contains("_noshow")) continue;
            var sourcePrefab = (GameObject) AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
            m_EffectPrefabList.Add(sourcePrefab);
        }
#endif
        if (Application.isPlaying == false)
            return;
        m_NowIndex = 1;
        GenPrevEffect();
    }

    private void OnDestroy()
    {
        DestroyImmediate(m_NowShowEffect);
    }

    private void LateUpdate()
    {
        if (Application.isPlaying == false)
            return;
        if (m_LookAtEffect && m_NowShowEffect) transform.LookAt(m_NowShowEffect.transform.position);
    }

    // Update is called once per frame
    private void OnGUI()
    {
        if (Application.isPlaying == false)
            return;
        if (GUI.Button(new Rect(0, 25, 80, 50), "Prev")) GenPrevEffect();
        if (GUI.Button(new Rect(90, 25, 80, 50), "Next")) GenNextEffect();
        GUI.Label(new Rect(5, 0, 350, 50), m_NowEffectName);
    }

    private void GenPrevEffect()
    {
        m_NowIndex--;
        if (m_NowIndex < 0)
        {
            m_NowIndex = 0;
            return;
        }

        if (m_NowShowEffect != null) Destroy(m_NowShowEffect);
        m_NowShowEffect = Instantiate(m_EffectPrefabList[m_NowIndex]);
        m_NowEffectName = m_NowShowEffect.name;
    }

    private void GenNextEffect()
    {
        m_NowIndex++;
        if (m_NowIndex >= m_EffectPrefabList.Count)
        {
            m_NowIndex = m_EffectPrefabList.Count - 1;
            return;
        }

        if (m_NowShowEffect != null) Destroy(m_NowShowEffect);
        m_NowShowEffect = Instantiate(m_EffectPrefabList[m_NowIndex]);
        m_NowEffectName = m_NowShowEffect.name;
    }
}
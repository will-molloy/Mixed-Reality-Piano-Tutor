using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EffectToolBar
{
    public static void SelectedObjAddComponent<T>(string notSelectStr, string hasComponentStr) where T : MonoBehaviour
    {
        var selectObjList = Selection.GetFiltered(typeof(GameObject), SelectionMode.Unfiltered);
        if (selectObjList.Length == 0)
        {
            EditorUtility.DisplayDialog("", notSelectStr, "OK");
            return;
        }

        foreach (var go in selectObjList)
        {
            var Go = go as GameObject;
            if (Go.GetComponent<T>())
            {
                EditorUtility.DisplayDialog("", hasComponentStr, "OK");
                continue;
            }

            Go.AddComponent<T>();
        }
    }

    public static GameObject[] InstaniceEmptyPrimitiveType(string objName, PrimitiveType primitiveType)
    {
        var gameobjectList = new List<GameObject>();
        var selectObjList = Selection.GetFiltered(typeof(GameObject), SelectionMode.Unfiltered);
        if (selectObjList.Length > 1)
        {
            EditorUtility.DisplayDialog("", "Select To", "OK");
            return null;
        }

        if (selectObjList.Length == 0)
        {
            var go = GameObject.CreatePrimitive(primitiveType);
            go.name = objName;
            go.transform.position = Vector3.zero;
            gameobjectList.Add(go);
        }
        else
        {
            var go = selectObjList[0] as GameObject; // as GameObject;
            var childGo = GameObject.CreatePrimitive(primitiveType);
            childGo.transform.parent = go.transform;
            childGo.name = objName;
            childGo.transform.position = Vector3.zero;
            gameobjectList.Add(childGo);
        }

        return gameobjectList.ToArray();
    }

    public static GameObject[] InstaniceEmptyGameobject(string objName)
    {
        var gameobjectList = new List<GameObject>();
        var selectObjList = Selection.GetFiltered(typeof(GameObject), SelectionMode.Unfiltered);
        if (selectObjList.Length > 1)
        {
            EditorUtility.DisplayDialog("", "Select To", "OK");
            return null;
        }

        if (selectObjList.Length == 0)
        {
            var go = new GameObject();
            go.name = objName;
            go.transform.position = Vector3.zero;
            gameobjectList.Add(go);
        }
        else
        {
            var go = selectObjList[0] as GameObject; // as GameObject;
            var childGo = new GameObject();
            childGo.transform.parent = go.transform;
            childGo.name = objName;
            childGo.transform.position = Vector3.zero;
            gameobjectList.Add(childGo);
        }

        return gameobjectList.ToArray();
    }

    public static void AddComponentToGameObjectArray<T>(GameObject[] goArray) where T : Component
    {
        if (goArray == null)
            return;
        foreach (var go in goArray) go.AddComponent<T>();
    }

    [MenuItem("GameObject/Create Other/Dummy")]
    private static void CreateEmptyObject()
    {
        var goArray = InstaniceEmptyGameobject("empty_dummy");
        Selection.activeGameObject = goArray[0];
    }

    [MenuItem("GameObject/Create Other/Billboard(Dummy)")]
    private static void CreateEffectObject()
    {
        var goArray = InstaniceEmptyGameobject("Billboard_dummy");
        AddComponentToGameObjectArray<RenderEffect>(goArray);
        Selection.activeGameObject = goArray[0];
    }

    [MenuItem("GameObject/Create Other/Effect_Quad")]
    private static void CreateEffectObjectQuad()
    {
        var goArray = InstaniceEmptyPrimitiveType("EF_Quad", PrimitiveType.Quad);
        AddComponentToGameObjectArray<RenderEffect>(goArray);
        Selection.activeGameObject = goArray[0];
    }

    [MenuItem("GameObject/Create Other/TrailRender")]
    private static void CreateEffectObjectTrail()
    {
        var goArray = InstaniceEmptyGameobject("EF_Trail");
        AddComponentToGameObjectArray<TrailRenderer>(goArray);
        AddComponentToGameObjectArray<RenderEffect>(goArray);
        Selection.activeGameObject = goArray[0];
    }

    [MenuItem("GameObject/Create Other/LineRender")]
    private static void CreateEffectObjectLine()
    {
        var goArray = InstaniceEmptyGameobject("EF_Laser");
        AddComponentToGameObjectArray<LineRenderer>(goArray);
        AddComponentToGameObjectArray<RenderEffect>(goArray);
        Selection.activeGameObject = goArray[0];
    }

    [MenuItem("GameObject/Create Other/UV_Scorll")]
    private static void CreateEffectObjectParticle()
    {
        var goArray = InstaniceEmptyGameobject("Particle_UV");
        AddComponentToGameObjectArray<ParticleSystem>(goArray);
        AddComponentToGameObjectArray<RenderEffect>(goArray);
        Selection.activeGameObject = goArray[0];
    }
}
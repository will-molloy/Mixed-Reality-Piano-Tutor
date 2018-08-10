using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectsOnSceneChange : MonoBehaviour
{
    [SerializeField] private GameObject[] destroyees;

    void OnDestroy()
    {
        Debug.Log("Scene changed, destroying VR prefabs");
        foreach (var i in destroyees)
        {
            DestroyImmediate(i);
        }
    }

}

//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using UnityEngine;

/// <summary>
///     Set box and rigidbodies on the Controllers
/// </summary>
public class ControllerCollider : MonoBehaviour
{
#if ZED_STEAM_VR

    private ZEDSteamVRControllerManager padManager;

    private void Start()
    {
        padManager = GetComponent<ZEDSteamVRControllerManager>();
    }

    private void OnEnable()
    {
        ZEDSteamVRControllerManager.ZEDOnPadIndexSet += PadIndexSet;
    }

    private void OnDisable()
    {
        ZEDSteamVRControllerManager.ZEDOnPadIndexSet += PadIndexSet;
    }

    private void PadIndexSet()
    {
        var i = 0;
        foreach (var o in padManager.controllersGameObject)
        {
            if (o != null) Setcollider(o);

            i++;
        }
    }

    private void Setcollider(GameObject o)
    {
        var listMesh = o.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in listMesh)
            if (mf.name == "body")
            {
                var mesh = mf.gameObject.GetComponent<MeshCollider>();
                if (!mesh) mf.gameObject.AddComponent<MeshCollider>();
                var rigid = mf.gameObject.GetComponent<Rigidbody>();
                if (!rigid) rigid = mf.gameObject.AddComponent<Rigidbody>();
                rigid.useGravity = false;
                rigid.isKinematic = true;
            }
    }
#endif
}
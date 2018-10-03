//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

public struct PointerEventArgs
{
    public uint controllerIndex;
    public uint flags;
    public float distance;
    public Transform target;
}

public delegate void PointerEventHandler(object sender, PointerEventArgs e);


public class SteamVR_LaserPointer : MonoBehaviour
{
    public bool active = true;
    public bool addRigidBody;
    public Color color;
    public GameObject holder;
    private bool isActive;
    public GameObject pointer;

    private Transform previousContact;
    public Transform reference;
    public float thickness = 0.002f;
    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerOut;

    // Use this for initialization
    private void Start()
    {
        holder = new GameObject();
        holder.transform.parent = transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        pointer.transform.localRotation = Quaternion.identity;
        var collider = pointer.GetComponent<BoxCollider>();
        if (addRigidBody)
        {
            if (collider) collider.isTrigger = true;
            var rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else
        {
            if (collider) Destroy(collider);
        }

        var newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;
    }

    public virtual void OnPointerIn(PointerEventArgs e)
    {
        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerOut(PointerEventArgs e)
    {
        if (PointerOut != null)
            PointerOut(this, e);
    }


    // Update is called once per frame
    private void Update()
    {
        if (!isActive)
        {
            isActive = true;
            transform.GetChild(0).gameObject.SetActive(true);
        }

        var dist = 100f;

        var controller = GetComponent<SteamVR_TrackedController>();

        var raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        var bHit = Physics.Raycast(raycast, out hit);

        if (previousContact && previousContact != hit.transform)
        {
            var args = new PointerEventArgs();
            if (controller != null) args.controllerIndex = controller.controllerIndex;
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;
            OnPointerOut(args);
            previousContact = null;
        }

        if (bHit && previousContact != hit.transform)
        {
            var argsIn = new PointerEventArgs();
            if (controller != null) argsIn.controllerIndex = controller.controllerIndex;
            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;
            OnPointerIn(argsIn);
            previousContact = hit.transform;
        }

        if (!bHit) previousContact = null;
        if (bHit && hit.distance < 100f) dist = hit.distance;

        if (controller != null && controller.triggerPressed)
            pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
        else
            pointer.transform.localScale = new Vector3(thickness, thickness, dist);
        pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
    }
}
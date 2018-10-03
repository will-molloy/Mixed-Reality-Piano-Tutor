//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Enables/disables objects based on connectivity and assigned roles.
//
//=============================================================================

using UnityEngine;
using Valve.VR;

public class SteamVR_ControllerManager : MonoBehaviour
{
    private static readonly string hiddenPrefix = "hidden (";
    private static readonly string hiddenPostfix = ")";
    private static readonly string[] labels = {"left", "right"};

    [Tooltip(
        "Set to true if you want objects arbitrarily assigned to controllers before their role (left vs right) is identified")]
    public bool assignAllBeforeIdentified;

    private readonly bool[] connected = new bool[OpenVR.k_unMaxTrackedDeviceCount]; // controllers only

    private uint[] indices; // assigned

    private readonly SteamVR_Events.Action inputFocusAction;
    private readonly SteamVR_Events.Action deviceConnectedAction;
    private readonly SteamVR_Events.Action trackedDeviceRoleChangedAction;
    public GameObject left, right;

    // cached roles - may or may not be connected
    private uint leftIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

    [Tooltip("Populate with objects you want to assign to additional controllers")]
    public GameObject[] objects;

    private uint rightIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

    private SteamVR_ControllerManager()
    {
        inputFocusAction = SteamVR_Events.InputFocusAction(OnInputFocus);
        deviceConnectedAction = SteamVR_Events.DeviceConnectedAction(OnDeviceConnected);
        trackedDeviceRoleChangedAction = SteamVR_Events.SystemAction(EVREventType.VREvent_TrackedDeviceRoleChanged,
            OnTrackedDeviceRoleChanged);
    }

    // Helper function to avoid adding duplicates to object array.
    private void SetUniqueObject(GameObject o, int index)
    {
        for (var i = 0; i < index; i++)
            if (objects[i] == o)
                return;

        objects[index] = o;
    }

    // This needs to be called if you update left, right or objects at runtime (e.g. when dyanmically spawned).
    public void UpdateTargets()
    {
        // Add left and right entries to the head of the list so we only have to operate on the list itself.
        var objects = this.objects;
        var additional = objects != null ? objects.Length : 0;
        this.objects = new GameObject[2 + additional];
        SetUniqueObject(right, 0);
        SetUniqueObject(left, 1);
        for (var i = 0; i < additional; i++)
            SetUniqueObject(objects[i], 2 + i);

        // Reset assignments.
        indices = new uint[2 + additional];
        for (var i = 0; i < indices.Length; i++)
            indices[i] = OpenVR.k_unTrackedDeviceIndexInvalid;
    }

    private void Awake()
    {
        UpdateTargets();
    }

    private void OnEnable()
    {
        for (var i = 0; i < objects.Length; i++)
        {
            var obj = objects[i];
            if (obj != null)
                obj.SetActive(false);

            indices[i] = OpenVR.k_unTrackedDeviceIndexInvalid;
        }

        Refresh();

        for (var i = 0; i < SteamVR.connected.Length; i++)
            if (SteamVR.connected[i])
                OnDeviceConnected(i, true);

        inputFocusAction.enabled = true;
        deviceConnectedAction.enabled = true;
        trackedDeviceRoleChangedAction.enabled = true;
    }

    private void OnDisable()
    {
        inputFocusAction.enabled = false;
        deviceConnectedAction.enabled = false;
        trackedDeviceRoleChangedAction.enabled = false;
    }

    // Hide controllers when the dashboard is up.
    private void OnInputFocus(bool hasFocus)
    {
        if (hasFocus)
            for (var i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                if (obj != null)
                {
                    var label = i < 2 ? labels[i] : (i - 1).ToString();
                    ShowObject(obj.transform, hiddenPrefix + label + hiddenPostfix);
                }
            }
        else
            for (var i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                if (obj != null)
                {
                    var label = i < 2 ? labels[i] : (i - 1).ToString();
                    HideObject(obj.transform, hiddenPrefix + label + hiddenPostfix);
                }
            }
    }

    // Reparents to a new object and deactivates that object (this allows
    // us to call SetActive in OnDeviceConnected independently.
    private void HideObject(Transform t, string name)
    {
        if (t.gameObject.name.StartsWith(hiddenPrefix))
        {
            Debug.Log("Ignoring double-hide.");
            return;
        }

        var hidden = new GameObject(name).transform;
        hidden.parent = t.parent;
        t.parent = hidden;
        hidden.gameObject.SetActive(false);
    }

    private void ShowObject(Transform t, string name)
    {
        var hidden = t.parent;
        if (hidden.gameObject.name != name)
            return;
        t.parent = hidden.parent;
        Destroy(hidden.gameObject);
    }

    private void SetTrackedDeviceIndex(int objectIndex, uint trackedDeviceIndex)
    {
        // First make sure no one else is already using this index.
        if (trackedDeviceIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
            for (var i = 0; i < objects.Length; i++)
                if (i != objectIndex && indices[i] == trackedDeviceIndex)
                {
                    var obj = objects[i];
                    if (obj != null)
                        obj.SetActive(false);

                    indices[i] = OpenVR.k_unTrackedDeviceIndexInvalid;
                }

        // Only set when changed.
        if (trackedDeviceIndex != indices[objectIndex])
        {
            indices[objectIndex] = trackedDeviceIndex;

            var obj = objects[objectIndex];
            if (obj != null)
            {
                if (trackedDeviceIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
                {
                    obj.SetActive(false);
                }
                else
                {
                    obj.SetActive(true);
                    obj.BroadcastMessage("SetDeviceIndex", (int) trackedDeviceIndex,
                        SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    // Keep track of assigned roles.
    private void OnTrackedDeviceRoleChanged(VREvent_t vrEvent)
    {
        Refresh();
    }

    // Keep track of connected controller indices.
    private void OnDeviceConnected(int index, bool connected)
    {
        var changed = this.connected[index];
        this.connected[index] = false;

        if (connected)
        {
            var system = OpenVR.System;
            if (system != null)
            {
                var deviceClass = system.GetTrackedDeviceClass((uint) index);
                if (deviceClass == ETrackedDeviceClass.Controller ||
                    deviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    this.connected[index] = true;
                    changed = !changed; // if we clear and set the same index, nothing has changed
                }
            }
        }

        if (changed)
            Refresh();
    }

    public void Refresh()
    {
        var objectIndex = 0;

        var system = OpenVR.System;
        if (system != null)
        {
            leftIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            rightIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
        }

        // If neither role has been assigned yet, try hooking up at least the right controller.
        if (leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            for (uint deviceIndex = 0; deviceIndex < connected.Length; deviceIndex++)
            {
                if (objectIndex >= objects.Length)
                    break;

                if (!connected[deviceIndex])
                    continue;

                SetTrackedDeviceIndex(objectIndex++, deviceIndex);

                if (!assignAllBeforeIdentified)
                    break;
            }
        }
        else
        {
            SetTrackedDeviceIndex(objectIndex++,
                rightIndex < connected.Length && connected[rightIndex]
                    ? rightIndex
                    : OpenVR.k_unTrackedDeviceIndexInvalid);
            SetTrackedDeviceIndex(objectIndex++,
                leftIndex < connected.Length && connected[leftIndex]
                    ? leftIndex
                    : OpenVR.k_unTrackedDeviceIndexInvalid);

            // Assign out any additional controllers only after both left and right have been assigned.
            if (leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
                for (uint deviceIndex = 0; deviceIndex < connected.Length; deviceIndex++)
                {
                    if (objectIndex >= objects.Length)
                        break;

                    if (!connected[deviceIndex])
                        continue;

                    if (deviceIndex != leftIndex && deviceIndex != rightIndex)
                        SetTrackedDeviceIndex(objectIndex++, deviceIndex);
                }
        }

        // Reset the rest.
        while (objectIndex < objects.Length) SetTrackedDeviceIndex(objectIndex++, OpenVR.k_unTrackedDeviceIndexInvalid);
    }
}
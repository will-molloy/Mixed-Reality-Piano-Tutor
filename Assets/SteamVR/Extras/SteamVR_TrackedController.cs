//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System.Runtime.InteropServices;
using UnityEngine;
using Valve.VR;

public struct ClickedEventArgs
{
    public uint controllerIndex;
    public uint flags;
    public float padX, padY;
}

public delegate void ClickedEventHandler(object sender, ClickedEventArgs e);

public class SteamVR_TrackedController : MonoBehaviour
{
    public uint controllerIndex;
    public VRControllerState_t controllerState;
    public bool gripped;
    public bool menuPressed;
    public bool padPressed;
    public bool padTouched;
    public bool steamPressed;
    public bool triggerPressed;

    public event ClickedEventHandler MenuButtonClicked;
    public event ClickedEventHandler MenuButtonUnclicked;
    public event ClickedEventHandler TriggerClicked;
    public event ClickedEventHandler TriggerUnclicked;
    public event ClickedEventHandler SteamClicked;
    public event ClickedEventHandler PadClicked;
    public event ClickedEventHandler PadUnclicked;
    public event ClickedEventHandler PadTouched;
    public event ClickedEventHandler PadUntouched;
    public event ClickedEventHandler Gripped;
    public event ClickedEventHandler Ungripped;

    // Use this for initialization
    protected virtual void Start()
    {
        if (GetComponent<SteamVR_TrackedObject>() == null) gameObject.AddComponent<SteamVR_TrackedObject>();

        if (controllerIndex != 0)
        {
            GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) controllerIndex;
            if (GetComponent<SteamVR_RenderModel>() != null)
                GetComponent<SteamVR_RenderModel>().index = (SteamVR_TrackedObject.EIndex) controllerIndex;
        }
        else
        {
            controllerIndex = (uint) GetComponent<SteamVR_TrackedObject>().index;
        }
    }

    public void SetDeviceIndex(int index)
    {
        controllerIndex = (uint) index;
    }

    public virtual void OnTriggerClicked(ClickedEventArgs e)
    {
        if (TriggerClicked != null)
            TriggerClicked(this, e);
    }

    public virtual void OnTriggerUnclicked(ClickedEventArgs e)
    {
        if (TriggerUnclicked != null)
            TriggerUnclicked(this, e);
    }

    public virtual void OnMenuClicked(ClickedEventArgs e)
    {
        if (MenuButtonClicked != null)
            MenuButtonClicked(this, e);
    }

    public virtual void OnMenuUnclicked(ClickedEventArgs e)
    {
        if (MenuButtonUnclicked != null)
            MenuButtonUnclicked(this, e);
    }

    public virtual void OnSteamClicked(ClickedEventArgs e)
    {
        if (SteamClicked != null)
            SteamClicked(this, e);
    }

    public virtual void OnPadClicked(ClickedEventArgs e)
    {
        if (PadClicked != null)
            PadClicked(this, e);
    }

    public virtual void OnPadUnclicked(ClickedEventArgs e)
    {
        if (PadUnclicked != null)
            PadUnclicked(this, e);
    }

    public virtual void OnPadTouched(ClickedEventArgs e)
    {
        if (PadTouched != null)
            PadTouched(this, e);
    }

    public virtual void OnPadUntouched(ClickedEventArgs e)
    {
        if (PadUntouched != null)
            PadUntouched(this, e);
    }

    public virtual void OnGripped(ClickedEventArgs e)
    {
        if (Gripped != null)
            Gripped(this, e);
    }

    public virtual void OnUngripped(ClickedEventArgs e)
    {
        if (Ungripped != null)
            Ungripped(this, e);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        var system = OpenVR.System;
        if (system != null && system.GetControllerState(controllerIndex, ref controllerState,
                (uint) Marshal.SizeOf(typeof(VRControllerState_t))))
        {
            var trigger = controllerState.ulButtonPressed & (1UL << (int) EVRButtonId.k_EButton_SteamVR_Trigger);
            if (trigger > 0L && !triggerPressed)
            {
                triggerPressed = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnTriggerClicked(e);
            }
            else if (trigger == 0L && triggerPressed)
            {
                triggerPressed = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnTriggerUnclicked(e);
            }

            var grip = controllerState.ulButtonPressed & (1UL << (int) EVRButtonId.k_EButton_Grip);
            if (grip > 0L && !gripped)
            {
                gripped = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnGripped(e);
            }
            else if (grip == 0L && gripped)
            {
                gripped = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnUngripped(e);
            }

            var pad = controllerState.ulButtonPressed & (1UL << (int) EVRButtonId.k_EButton_SteamVR_Touchpad);
            if (pad > 0L && !padPressed)
            {
                padPressed = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadClicked(e);
            }
            else if (pad == 0L && padPressed)
            {
                padPressed = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadUnclicked(e);
            }

            var menu = controllerState.ulButtonPressed & (1UL << (int) EVRButtonId.k_EButton_ApplicationMenu);
            if (menu > 0L && !menuPressed)
            {
                menuPressed = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnMenuClicked(e);
            }
            else if (menu == 0L && menuPressed)
            {
                menuPressed = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnMenuUnclicked(e);
            }

            pad = controllerState.ulButtonTouched & (1UL << (int) EVRButtonId.k_EButton_SteamVR_Touchpad);
            if (pad > 0L && !padTouched)
            {
                padTouched = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadTouched(e);
            }
            else if (pad == 0L && padTouched)
            {
                padTouched = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint) controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadUntouched(e);
            }
        }
    }
}
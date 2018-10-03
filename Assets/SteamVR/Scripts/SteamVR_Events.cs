//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Simple event system for SteamVR.
//
// Example usage:
//
//			void OnDeviceConnected(int i, bool connected) { ... }
//			SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected); // Usually in OnEnable
//			SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected); // Usually in OnDisable
//
// Alternatively, if Listening/Removing often these can be cached as follows:
//
//			SteamVR_Event.Action deviceConnectedAction;
//			void OnAwake() { deviceConnectedAction = SteamVR_Event.DeviceConnectedAction(OnDeviceConnected); }
//			void OnEnable() { deviceConnectedAction.enabled = true; }
//			void OnDisable() { deviceConnectedAction.enabled = false; }
//
//=============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public static class SteamVR_Events
{
    public static Event<bool> Calibrating = new Event<bool>();

    public static Event<int, bool> DeviceConnected = new Event<int, bool>();

    public static Event<Color, float, bool> Fade = new Event<Color, float, bool>();

    public static Event FadeReady = new Event();

    public static Event<bool> HideRenderModels = new Event<bool>();

    public static Event<bool> Initializing = new Event<bool>();

    public static Event<bool> InputFocus = new Event<bool>();

    public static Event<bool> Loading = new Event<bool>();

    public static Event<float> LoadingFadeIn = new Event<float>();

    public static Event<float> LoadingFadeOut = new Event<float>();

    public static Event<TrackedDevicePose_t[]> NewPoses = new Event<TrackedDevicePose_t[]>();

    public static Event NewPosesApplied = new Event();

    public static Event<bool> OutOfRange = new Event<bool>();

    public static Event<SteamVR_RenderModel, bool> RenderModelLoaded = new Event<SteamVR_RenderModel, bool>();

    private static readonly Dictionary<EVREventType, Event<VREvent_t>> systemEvents =
        new Dictionary<EVREventType, Event<VREvent_t>>();

    public static Action CalibratingAction(UnityAction<bool> action)
    {
        return new Action<bool>(Calibrating, action);
    }

    public static Action DeviceConnectedAction(UnityAction<int, bool> action)
    {
        return new Action<int, bool>(DeviceConnected, action);
    }

    public static Action FadeAction(UnityAction<Color, float, bool> action)
    {
        return new Action<Color, float, bool>(Fade, action);
    }

    public static Action FadeReadyAction(UnityAction action)
    {
        return new ActionNoArgs(FadeReady, action);
    }

    public static Action HideRenderModelsAction(UnityAction<bool> action)
    {
        return new Action<bool>(HideRenderModels, action);
    }

    public static Action InitializingAction(UnityAction<bool> action)
    {
        return new Action<bool>(Initializing, action);
    }

    public static Action InputFocusAction(UnityAction<bool> action)
    {
        return new Action<bool>(InputFocus, action);
    }

    public static Action LoadingAction(UnityAction<bool> action)
    {
        return new Action<bool>(Loading, action);
    }

    public static Action LoadingFadeInAction(UnityAction<float> action)
    {
        return new Action<float>(LoadingFadeIn, action);
    }

    public static Action LoadingFadeOutAction(UnityAction<float> action)
    {
        return new Action<float>(LoadingFadeOut, action);
    }

    public static Action NewPosesAction(UnityAction<TrackedDevicePose_t[]> action)
    {
        return new Action<TrackedDevicePose_t[]>(NewPoses, action);
    }

    public static Action NewPosesAppliedAction(UnityAction action)
    {
        return new ActionNoArgs(NewPosesApplied, action);
    }

    public static Action OutOfRangeAction(UnityAction<bool> action)
    {
        return new Action<bool>(OutOfRange, action);
    }

    public static Action RenderModelLoadedAction(UnityAction<SteamVR_RenderModel, bool> action)
    {
        return new Action<SteamVR_RenderModel, bool>(RenderModelLoaded, action);
    }

    public static Event<VREvent_t> System(EVREventType eventType)
    {
        Event<VREvent_t> e;
        if (!systemEvents.TryGetValue(eventType, out e))
        {
            e = new Event<VREvent_t>();
            systemEvents.Add(eventType, e);
        }

        return e;
    }

    public static Action SystemAction(EVREventType eventType, UnityAction<VREvent_t> action)
    {
        return new Action<VREvent_t>(System(eventType), action);
    }

    public abstract class Action
    {
        public bool enabled
        {
            set { Enable(value); }
        }

        public abstract void Enable(bool enabled);
    }

    [Serializable]
    public class ActionNoArgs : Action
    {
        private readonly Event _event;
        private readonly UnityAction action;

        public ActionNoArgs(Event _event, UnityAction action)
        {
            this._event = _event;
            this.action = action;
        }

        public override void Enable(bool enabled)
        {
            if (enabled)
                _event.Listen(action);
            else
                _event.Remove(action);
        }
    }

    [Serializable]
    public class Action<T> : Action
    {
        private readonly Event<T> _event;
        private readonly UnityAction<T> action;

        public Action(Event<T> _event, UnityAction<T> action)
        {
            this._event = _event;
            this.action = action;
        }

        public override void Enable(bool enabled)
        {
            if (enabled)
                _event.Listen(action);
            else
                _event.Remove(action);
        }
    }

    [Serializable]
    public class Action<T0, T1> : Action
    {
        private readonly Event<T0, T1> _event;
        private readonly UnityAction<T0, T1> action;

        public Action(Event<T0, T1> _event, UnityAction<T0, T1> action)
        {
            this._event = _event;
            this.action = action;
        }

        public override void Enable(bool enabled)
        {
            if (enabled)
                _event.Listen(action);
            else
                _event.Remove(action);
        }
    }

    [Serializable]
    public class Action<T0, T1, T2> : Action
    {
        private readonly Event<T0, T1, T2> _event;
        private readonly UnityAction<T0, T1, T2> action;

        public Action(Event<T0, T1, T2> _event, UnityAction<T0, T1, T2> action)
        {
            this._event = _event;
            this.action = action;
        }

        public override void Enable(bool enabled)
        {
            if (enabled)
                _event.Listen(action);
            else
                _event.Remove(action);
        }
    }

    public class Event : UnityEvent
    {
        public void Listen(UnityAction action)
        {
            AddListener(action);
        }

        public void Remove(UnityAction action)
        {
            RemoveListener(action);
        }

        public void Send()
        {
            Invoke();
        }
    }

    public class Event<T> : UnityEvent<T>
    {
        public void Listen(UnityAction<T> action)
        {
            AddListener(action);
        }

        public void Remove(UnityAction<T> action)
        {
            RemoveListener(action);
        }

        public void Send(T arg0)
        {
            Invoke(arg0);
        }
    }

    public class Event<T0, T1> : UnityEvent<T0, T1>
    {
        public void Listen(UnityAction<T0, T1> action)
        {
            AddListener(action);
        }

        public void Remove(UnityAction<T0, T1> action)
        {
            RemoveListener(action);
        }

        public void Send(T0 arg0, T1 arg1)
        {
            Invoke(arg0, arg1);
        }
    }

    public class Event<T0, T1, T2> : UnityEvent<T0, T1, T2>
    {
        public void Listen(UnityAction<T0, T1, T2> action)
        {
            AddListener(action);
        }

        public void Remove(UnityAction<T0, T1, T2> action)
        {
            RemoveListener(action);
        }

        public void Send(T0 arg0, T1 arg1, T2 arg2)
        {
            Invoke(arg0, arg1, arg2);
        }
    }
}
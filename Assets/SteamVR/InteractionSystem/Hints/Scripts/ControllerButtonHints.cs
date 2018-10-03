//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Displays text and button hints on the controllers
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class ControllerButtonHints : MonoBehaviour
    {
        private Dictionary<EVRButtonId, ButtonHintInfo> buttonHintInfos;
        private Vector3 centerPosition = Vector3.zero;

        private int colorID;

        private readonly List<KeyValuePair<string, ulong>> componentButtonMasks =
            new List<KeyValuePair<string, ulong>>();

        public Material controllerMaterial;

        [Header("Debug")] public bool debugHints;

        public Color flashColor = new Color(1.0f, 0.557f, 0.0f);
        private readonly List<MeshRenderer> flashingRenderers = new List<MeshRenderer>();
        private Player player;

        private readonly List<MeshRenderer> renderers = new List<MeshRenderer>();

        private SteamVR_RenderModel renderModel;

        private SteamVR_Events.Action renderModelLoadedAction;
        private float startTime;
        private Transform textHintParent;
        public GameObject textHintPrefab;
        private float tickCount;

        public bool initialized { get; private set; }


        //-------------------------------------------------
        private void Awake()
        {
            renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(OnRenderModelLoaded);
            colorID = Shader.PropertyToID("_Color");
        }


        //-------------------------------------------------
        private void Start()
        {
            player = Player.instance;
        }


        //-------------------------------------------------
        private void HintDebugLog(string msg)
        {
            if (debugHints) Debug.Log("Hints: " + msg);
        }


        //-------------------------------------------------
        private void OnEnable()
        {
            renderModelLoadedAction.enabled = true;
        }


        //-------------------------------------------------
        private void OnDisable()
        {
            renderModelLoadedAction.enabled = false;
            Clear();
        }


        //-------------------------------------------------
        private void OnParentHandInputFocusLost()
        {
            //Hide all the hints when the controller is no longer the primary attached object
            HideAllButtonHints();
            HideAllText();
        }


        //-------------------------------------------------
        // Gets called when the hand has been initialized and a render model has been set
        //-------------------------------------------------
        private void OnHandInitialized(int deviceIndex)
        {
            //Create a new render model for the controller hints
            renderModel = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
            renderModel.transform.parent = transform;
            renderModel.transform.localPosition = Vector3.zero;
            renderModel.transform.localRotation = Quaternion.identity;
            renderModel.transform.localScale = Vector3.one;
            renderModel.SetDeviceIndex(deviceIndex);

            if (!initialized) renderModel.gameObject.SetActive(true);
        }


        //-------------------------------------------------
        private void OnRenderModelLoaded(SteamVR_RenderModel renderModel, bool succeess)
        {
            //Only initialize when the render model for the controller hints has been loaded
            if (renderModel == this.renderModel)
            {
                textHintParent = new GameObject("Text Hints").transform;
                textHintParent.SetParent(transform);
                textHintParent.localPosition = Vector3.zero;
                textHintParent.localRotation = Quaternion.identity;
                textHintParent.localScale = Vector3.one;

                //Get the button mask for each component of the render model
                using (var holder = new SteamVR_RenderModel.RenderModelInterfaceHolder())
                {
                    var renderModels = holder.instance;
                    if (renderModels != null)
                    {
                        var renderModelDebug = "Components for render model " + renderModel.index;
                        foreach (Transform child in renderModel.transform)
                        {
                            var buttonMask =
                                renderModels.GetComponentButtonMask(renderModel.renderModelName, child.name);

                            componentButtonMasks.Add(new KeyValuePair<string, ulong>(child.name, buttonMask));

                            renderModelDebug += "\n\t" + child.name + ": " + buttonMask;
                        }

                        //Uncomment to show the button mask for each component of the render model
                        HintDebugLog(renderModelDebug);
                    }
                }

                buttonHintInfos = new Dictionary<EVRButtonId, ButtonHintInfo>();

                CreateAndAddButtonInfo(EVRButtonId.k_EButton_SteamVR_Trigger);
                CreateAndAddButtonInfo(EVRButtonId.k_EButton_ApplicationMenu);
                CreateAndAddButtonInfo(EVRButtonId.k_EButton_System);
                CreateAndAddButtonInfo(EVRButtonId.k_EButton_Grip);
                CreateAndAddButtonInfo(EVRButtonId.k_EButton_SteamVR_Touchpad);
                CreateAndAddButtonInfo(EVRButtonId.k_EButton_A);

                ComputeTextEndTransforms();

                initialized = true;

                //Set the controller hints render model to not active
                renderModel.gameObject.SetActive(false);
            }
        }


        //-------------------------------------------------
        private void CreateAndAddButtonInfo(EVRButtonId buttonID)
        {
            Transform buttonTransform = null;
            var buttonRenderers = new List<MeshRenderer>();

            var buttonDebug = "Looking for button: " + buttonID;

            var searchButtonID = buttonID;
            if (buttonID == EVRButtonId.k_EButton_Grip &&
                SteamVR.instance.hmd_TrackingSystemName.ToLowerInvariant().Contains("oculus"))
                searchButtonID = EVRButtonId.k_EButton_Axis2;
            var buttonMaskForID = 1ul << (int) searchButtonID;

            foreach (var componentButtonMask in componentButtonMasks)
                if ((componentButtonMask.Value & buttonMaskForID) == buttonMaskForID)
                {
                    buttonDebug += "\nFound component: " + componentButtonMask.Key + " " + componentButtonMask.Value;
                    var componentTransform = renderModel.FindComponent(componentButtonMask.Key);

                    buttonTransform = componentTransform;

                    buttonDebug += "\nFound componentTransform: " + componentTransform + " buttonTransform: " +
                                   buttonTransform;

                    buttonRenderers.AddRange(componentTransform.GetComponentsInChildren<MeshRenderer>());
                }

            buttonDebug += "\nFound " + buttonRenderers.Count + " renderers for " + buttonID;
            foreach (var renderer in buttonRenderers) buttonDebug += "\n\t" + renderer.name;

            HintDebugLog(buttonDebug);

            if (buttonTransform == null)
            {
                HintDebugLog("Couldn't find buttonTransform for " + buttonID);
                return;
            }

            var hintInfo = new ButtonHintInfo();
            buttonHintInfos.Add(buttonID, hintInfo);

            hintInfo.componentName = buttonTransform.name;
            hintInfo.renderers = buttonRenderers;

            //Get the local transform for the button
            hintInfo.localTransform = buttonTransform.Find(SteamVR_RenderModel.k_localTransformName);

            var offsetType = OffsetType.Right;
            switch (buttonID)
            {
                case EVRButtonId.k_EButton_SteamVR_Trigger:
                {
                    offsetType = OffsetType.Right;
                }
                    break;
                case EVRButtonId.k_EButton_ApplicationMenu:
                {
                    offsetType = OffsetType.Right;
                }
                    break;
                case EVRButtonId.k_EButton_System:
                {
                    offsetType = OffsetType.Right;
                }
                    break;
                case EVRButtonId.k_EButton_Grip:
                {
                    offsetType = OffsetType.Forward;
                }
                    break;
                case EVRButtonId.k_EButton_SteamVR_Touchpad:
                {
                    offsetType = OffsetType.Up;
                }
                    break;
            }

            //Offset for the text end transform
            switch (offsetType)
            {
                case OffsetType.Forward:
                    hintInfo.textEndOffsetDir = hintInfo.localTransform.forward;
                    break;
                case OffsetType.Back:
                    hintInfo.textEndOffsetDir = -hintInfo.localTransform.forward;
                    break;
                case OffsetType.Right:
                    hintInfo.textEndOffsetDir = hintInfo.localTransform.right;
                    break;
                case OffsetType.Up:
                    hintInfo.textEndOffsetDir = hintInfo.localTransform.up;
                    break;
            }

            //Create the text hint object
            var hintStartPos = hintInfo.localTransform.position + hintInfo.localTransform.forward * 0.01f;
            hintInfo.textHintObject = Instantiate(textHintPrefab, hintStartPos, Quaternion.identity);
            hintInfo.textHintObject.name = "Hint_" + hintInfo.componentName + "_Start";
            hintInfo.textHintObject.transform.SetParent(textHintParent);
            hintInfo.textHintObject.layer = gameObject.layer;
            hintInfo.textHintObject.tag = gameObject.tag;

            //Get all the relevant child objects
            hintInfo.textStartAnchor = hintInfo.textHintObject.transform.Find("Start");
            hintInfo.textEndAnchor = hintInfo.textHintObject.transform.Find("End");
            hintInfo.canvasOffset = hintInfo.textHintObject.transform.Find("CanvasOffset");
            hintInfo.line = hintInfo.textHintObject.transform.Find("Line").GetComponent<LineRenderer>();
            hintInfo.textCanvas = hintInfo.textHintObject.GetComponentInChildren<Canvas>();
            hintInfo.text = hintInfo.textCanvas.GetComponentInChildren<Text>();
            hintInfo.textMesh = hintInfo.textCanvas.GetComponentInChildren<TextMesh>();

            hintInfo.textHintObject.SetActive(false);

            hintInfo.textStartAnchor.position = hintStartPos;

            if (hintInfo.text != null) hintInfo.text.text = hintInfo.componentName;

            if (hintInfo.textMesh != null) hintInfo.textMesh.text = hintInfo.componentName;

            centerPosition += hintInfo.textStartAnchor.position;

            // Scale hint components to match player size
            hintInfo.textCanvas.transform.localScale =
                Vector3.Scale(hintInfo.textCanvas.transform.localScale, player.transform.localScale);
            hintInfo.textStartAnchor.transform.localScale = Vector3.Scale(hintInfo.textStartAnchor.transform.localScale,
                player.transform.localScale);
            hintInfo.textEndAnchor.transform.localScale = Vector3.Scale(hintInfo.textEndAnchor.transform.localScale,
                player.transform.localScale);
            hintInfo.line.transform.localScale =
                Vector3.Scale(hintInfo.line.transform.localScale, player.transform.localScale);
        }


        //-------------------------------------------------
        private void ComputeTextEndTransforms()
        {
            //This is done as a separate step after all the ButtonHintInfos have been initialized
            //to make the text hints fan out appropriately based on the button's position on the controller.

            centerPosition /= buttonHintInfos.Count;
            var maxDistanceFromCenter = 0.0f;

            foreach (var hintInfo in buttonHintInfos)
            {
                hintInfo.Value.distanceFromCenter =
                    Vector3.Distance(hintInfo.Value.textStartAnchor.position, centerPosition);

                if (hintInfo.Value.distanceFromCenter > maxDistanceFromCenter)
                    maxDistanceFromCenter = hintInfo.Value.distanceFromCenter;
            }

            foreach (var hintInfo in buttonHintInfos)
            {
                var centerToButton = hintInfo.Value.textStartAnchor.position - centerPosition;
                centerToButton.Normalize();

                centerToButton = Vector3.Project(centerToButton, renderModel.transform.forward);

                //Spread out the text end positions based on the distance from the center
                var t = hintInfo.Value.distanceFromCenter / maxDistanceFromCenter;
                var scale = hintInfo.Value.distanceFromCenter * Mathf.Pow(2, 10 * (t - 1.0f)) * 20.0f;

                //Flip the direction of the end pos based on which hand this is
                var endPosOffset = 0.1f;

                var hintEndPos = hintInfo.Value.textStartAnchor.position +
                                 hintInfo.Value.textEndOffsetDir * endPosOffset + centerToButton * scale * 0.1f;
                hintInfo.Value.textEndAnchor.position = hintEndPos;

                hintInfo.Value.canvasOffset.position = hintEndPos;
                hintInfo.Value.canvasOffset.localRotation = Quaternion.identity;
            }
        }


        //-------------------------------------------------
        private void ShowButtonHint(params EVRButtonId[] buttons)
        {
            renderModel.gameObject.SetActive(true);

            renderModel.GetComponentsInChildren(renderers);
            for (var i = 0; i < renderers.Count; i++)
            {
                var mainTexture = renderers[i].material.mainTexture;
                renderers[i].sharedMaterial = controllerMaterial;
                renderers[i].material.mainTexture = mainTexture;

                // This is to poke unity into setting the correct render queue for the model
                renderers[i].material.renderQueue = controllerMaterial.shader.renderQueue;
            }

            for (var i = 0; i < buttons.Length; i++)
                if (buttonHintInfos.ContainsKey(buttons[i]))
                {
                    var hintInfo = buttonHintInfos[buttons[i]];
                    foreach (var renderer in hintInfo.renderers)
                        if (!flashingRenderers.Contains(renderer))
                            flashingRenderers.Add(renderer);
                }

            startTime = Time.realtimeSinceStartup;
            tickCount = 0.0f;
        }


        //-------------------------------------------------
        private void HideAllButtonHints()
        {
            Clear();

            renderModel.gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private void HideButtonHint(params EVRButtonId[] buttons)
        {
            var baseColor = controllerMaterial.GetColor(colorID);
            for (var i = 0; i < buttons.Length; i++)
                if (buttonHintInfos.ContainsKey(buttons[i]))
                {
                    var hintInfo = buttonHintInfos[buttons[i]];
                    foreach (var renderer in hintInfo.renderers)
                    {
                        renderer.material.color = baseColor;
                        flashingRenderers.Remove(renderer);
                    }
                }

            if (flashingRenderers.Count == 0) renderModel.gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private bool IsButtonHintActive(EVRButtonId button)
        {
            if (buttonHintInfos.ContainsKey(button))
            {
                var hintInfo = buttonHintInfos[button];
                foreach (var buttonRenderer in hintInfo.renderers)
                    if (flashingRenderers.Contains(buttonRenderer))
                        return true;
            }

            return false;
        }


        //-------------------------------------------------
        private IEnumerator TestButtonHints()
        {
            while (true)
            {
                ShowButtonHint(EVRButtonId.k_EButton_SteamVR_Trigger);
                yield return new WaitForSeconds(1.0f);
                ShowButtonHint(EVRButtonId.k_EButton_ApplicationMenu);
                yield return new WaitForSeconds(1.0f);
                ShowButtonHint(EVRButtonId.k_EButton_System);
                yield return new WaitForSeconds(1.0f);
                ShowButtonHint(EVRButtonId.k_EButton_Grip);
                yield return new WaitForSeconds(1.0f);
                ShowButtonHint(EVRButtonId.k_EButton_SteamVR_Touchpad);
                yield return new WaitForSeconds(1.0f);
            }
        }


        //-------------------------------------------------
        private IEnumerator TestTextHints()
        {
            while (true)
            {
                ShowText(EVRButtonId.k_EButton_SteamVR_Trigger, "Trigger");
                yield return new WaitForSeconds(3.0f);
                ShowText(EVRButtonId.k_EButton_ApplicationMenu, "Application");
                yield return new WaitForSeconds(3.0f);
                ShowText(EVRButtonId.k_EButton_System, "System");
                yield return new WaitForSeconds(3.0f);
                ShowText(EVRButtonId.k_EButton_Grip, "Grip");
                yield return new WaitForSeconds(3.0f);
                ShowText(EVRButtonId.k_EButton_SteamVR_Touchpad, "Touchpad");
                yield return new WaitForSeconds(3.0f);

                HideAllText();
                yield return new WaitForSeconds(3.0f);
            }
        }


        //-------------------------------------------------
        private void Update()
        {
            if (renderModel != null && renderModel.gameObject.activeInHierarchy && flashingRenderers.Count > 0)
            {
                var baseColor = controllerMaterial.GetColor(colorID);

                var flash = (Time.realtimeSinceStartup - startTime) * Mathf.PI * 2.0f;
                flash = Mathf.Cos(flash);
                flash = Util.RemapNumberClamped(flash, -1.0f, 1.0f, 0.0f, 1.0f);

                var ticks = Time.realtimeSinceStartup - startTime;
                if (ticks - tickCount > 1.0f)
                {
                    tickCount += 1.0f;
                    var device = SteamVR_Controller.Input((int) renderModel.index);
                    if (device != null) device.TriggerHapticPulse();
                }

                for (var i = 0; i < flashingRenderers.Count; i++)
                {
                    Renderer r = flashingRenderers[i];
                    r.material.SetColor(colorID, Color.Lerp(baseColor, flashColor, flash));
                }

                if (initialized)
                    foreach (var hintInfo in buttonHintInfos)
                        if (hintInfo.Value.textHintActive)
                            UpdateTextHint(hintInfo.Value);
            }
        }


        //-------------------------------------------------
        private void UpdateTextHint(ButtonHintInfo hintInfo)
        {
            var playerTransform = player.hmdTransform;
            var vDir = playerTransform.position - hintInfo.canvasOffset.position;

            var standardLookat = Quaternion.LookRotation(vDir, Vector3.up);
            var upsideDownLookat = Quaternion.LookRotation(vDir, playerTransform.up);

            float flInterp;
            if (playerTransform.forward.y > 0.0f)
                flInterp = Util.RemapNumberClamped(playerTransform.forward.y, 0.6f, 0.4f, 1.0f, 0.0f);
            else
                flInterp = Util.RemapNumberClamped(playerTransform.forward.y, -0.8f, -0.6f, 1.0f, 0.0f);

            hintInfo.canvasOffset.rotation = Quaternion.Slerp(standardLookat, upsideDownLookat, flInterp);

            var lineTransform = hintInfo.line.transform;

            hintInfo.line.useWorldSpace = false;
            hintInfo.line.SetPosition(0, lineTransform.InverseTransformPoint(hintInfo.textStartAnchor.position));
            hintInfo.line.SetPosition(1, lineTransform.InverseTransformPoint(hintInfo.textEndAnchor.position));
        }


        //-------------------------------------------------
        private void Clear()
        {
            renderers.Clear();
            flashingRenderers.Clear();
        }


        //-------------------------------------------------
        private void ShowText(EVRButtonId button, string text, bool highlightButton = true)
        {
            if (buttonHintInfos.ContainsKey(button))
            {
                var hintInfo = buttonHintInfos[button];
                hintInfo.textHintObject.SetActive(true);
                hintInfo.textHintActive = true;

                if (hintInfo.text != null) hintInfo.text.text = text;

                if (hintInfo.textMesh != null) hintInfo.textMesh.text = text;

                UpdateTextHint(hintInfo);

                if (highlightButton) ShowButtonHint(button);

                renderModel.gameObject.SetActive(true);
            }
        }


        //-------------------------------------------------
        private void HideText(EVRButtonId button)
        {
            if (buttonHintInfos.ContainsKey(button))
            {
                var hintInfo = buttonHintInfos[button];
                hintInfo.textHintObject.SetActive(false);
                hintInfo.textHintActive = false;

                HideButtonHint(button);
            }
        }


        //-------------------------------------------------
        private void HideAllText()
        {
            foreach (var hintInfo in buttonHintInfos)
            {
                hintInfo.Value.textHintObject.SetActive(false);
                hintInfo.Value.textHintActive = false;
            }

            HideAllButtonHints();
        }


        //-------------------------------------------------
        private string GetActiveHintText(EVRButtonId button)
        {
            if (buttonHintInfos.ContainsKey(button))
            {
                var hintInfo = buttonHintInfos[button];
                if (hintInfo.textHintActive) return hintInfo.text.text;
            }

            return string.Empty;
        }


        //-------------------------------------------------
        // These are the static functions which are used to show/hide the hints
        //-------------------------------------------------

        //-------------------------------------------------
        private static ControllerButtonHints GetControllerButtonHints(Hand hand)
        {
            if (hand != null)
            {
                var hints = hand.GetComponentInChildren<ControllerButtonHints>();
                if (hints != null && hints.initialized) return hints;
            }

            return null;
        }


        //-------------------------------------------------
        public static void ShowButtonHint(Hand hand, params EVRButtonId[] buttons)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) hints.ShowButtonHint(buttons);
        }


        //-------------------------------------------------
        public static void HideButtonHint(Hand hand, params EVRButtonId[] buttons)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) hints.HideButtonHint(buttons);
        }


        //-------------------------------------------------
        public static void HideAllButtonHints(Hand hand)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) hints.HideAllButtonHints();
        }


        //-------------------------------------------------
        public static bool IsButtonHintActive(Hand hand, EVRButtonId button)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) return hints.IsButtonHintActive(button);

            return false;
        }


        //-------------------------------------------------
        public static void ShowTextHint(Hand hand, EVRButtonId button, string text, bool highlightButton = true)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) hints.ShowText(button, text, highlightButton);
        }


        //-------------------------------------------------
        public static void HideTextHint(Hand hand, EVRButtonId button)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) hints.HideText(button);
        }


        //-------------------------------------------------
        public static void HideAllTextHints(Hand hand)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) hints.HideAllText();
        }


        //-------------------------------------------------
        public static string GetActiveHintText(Hand hand, EVRButtonId button)
        {
            var hints = GetControllerButtonHints(hand);
            if (hints != null) return hints.GetActiveHintText(button);

            return string.Empty;
        }

        private enum OffsetType
        {
            Up,
            Right,
            Forward,
            Back
        }

        //Info for each of the buttons
        private class ButtonHintInfo
        {
            public Transform canvasOffset;
            public string componentName;

            public float distanceFromCenter;
            public LineRenderer line;
            public Transform localTransform;
            public List<MeshRenderer> renderers;

            public Text text;
            public Canvas textCanvas;
            public Transform textEndAnchor;
            public Vector3 textEndOffsetDir;
            public bool textHintActive;

            //Text hint
            public GameObject textHintObject;
            public TextMesh textMesh;
            public Transform textStartAnchor;
        }
    }
}
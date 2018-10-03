//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles all the teleport logic
//
//=============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class Teleport : MonoBehaviour
    {
        // Events

        public static SteamVR_Events.Event<float> ChangeScene = new SteamVR_Events.Event<float>();

        public static SteamVR_Events.Event<TeleportMarkerBase> Player = new SteamVR_Events.Event<TeleportMarkerBase>();

        public static SteamVR_Events.Event<TeleportMarkerBase> PlayerPre =
            new SteamVR_Events.Event<TeleportMarkerBase>();

        //-------------------------------------------------
        private static Teleport _instance;
        public float activateObjectTime = 1.0f;
        private AllowTeleportWhileAttachedToHand allowTeleportWhileAttached;

        public float arcDistance = 10.0f;
        public Material areaHighlightedMaterial;
        public Material areaLockedMaterial;
        public Material areaVisibleMaterial;
        public AudioClip badHighlightSound;

        private SteamVR_Events.Action chaperoneInfoInitializedAction;
        private float currentFadeTime;
        public float deactivateObjectTime = 1.0f;

        [Header("Debug")] public bool debugFloor;

        public Transform destinationReticleTransform;
        public LineRenderer floorDebugLine;
        public MeshRenderer floorDebugSphere;
        public float floorFixupMaximumTraceDistance = 1.0f;
        public LayerMask floorFixupTraceLayerMask;
        private float fullTintAlpha;
        public AudioClip goodHighlightSound;
        public AudioSource headAudioSource;

        private Coroutine hintCoroutine;
        private float invalidReticleMaxScale = 1.0f;
        private readonly float invalidReticleMaxScaleDistance = 2.0f;

        private float invalidReticleMinScale = 0.2f;
        private readonly float invalidReticleMinScaleDistance = 0.4f;
        private Vector3 invalidReticleScale = Vector3.one;
        private Quaternion invalidReticleTargetRotation = Quaternion.identity;
        public Transform invalidReticleTransform;

        private float loopingAudioMaxVolume;
        public AudioSource loopingAudioSource;

        private float meshAlphaPercent = 1.0f;
        public float meshFadeTime = 0.2f;
        private bool meshFading;
        private bool movedFeetFarEnough;
        public Transform offsetReticleTransform;

        [Header("Effects")] public Transform onActivateObjectTransform;

        public Transform onDeactivateObjectTransform;
        private Interactable originalHoveringInteractable;

        private bool originalHoverLockState;
        public GameObject playAreaPreviewCorner;
        private Transform[] playAreaPreviewCorners;
        public GameObject playAreaPreviewSide;
        private Transform[] playAreaPreviewSides;

        private Transform playAreaPreviewTransform;
        private Player player;
        private Vector3 pointedAtPosition;
        private TeleportMarkerBase pointedAtTeleportMarker;

        [Header("Audio Sources")] public AudioSource pointerAudioSource;

        private Hand pointerHand;
        private float pointerHideStartTime;
        public Color pointerInvalidColor;

        private LineRenderer pointerLineRenderer;
        public Color pointerLockedColor;
        public AudioClip pointerLoopSound;
        private float pointerShowStartTime;
        public AudioClip pointerStartSound;
        private Transform pointerStartTransform;
        public AudioClip pointerStopSound;
        public Color pointerValidColor;
        public Material pointHighlightedMaterial;
        public Material pointLockedMaterial;
        public Material pointVisibleMaterial;
        private Vector3 prevPointedAtPosition;
        public AudioSource reticleAudioSource;
        public bool showOffsetReticle;
        public bool showPlayAreaMarker = true;

        private Vector3 startingFeetOffset = Vector3.zero;
        private TeleportArc teleportArc;

        public float teleportFadeTime = 0.1f;
        private bool teleporting;
        private TeleportMarkerBase teleportingToMarker;

        private TeleportMarkerBase[] teleportMarkers;
        private GameObject teleportPointerObject;

        [Header("Sounds")] public AudioClip teleportSound;

        public LayerMask traceLayerMask;

        private bool visible;

        public static Teleport instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<Teleport>();

                return _instance;
            }
        }

        public static SteamVR_Events.Action<float> ChangeSceneAction(UnityAction<float> action)
        {
            return new SteamVR_Events.Action<float>(ChangeScene, action);
        }

        public static SteamVR_Events.Action<TeleportMarkerBase> PlayerAction(UnityAction<TeleportMarkerBase> action)
        {
            return new SteamVR_Events.Action<TeleportMarkerBase>(Player, action);
        }

        public static SteamVR_Events.Action<TeleportMarkerBase> PlayerPreAction(UnityAction<TeleportMarkerBase> action)
        {
            return new SteamVR_Events.Action<TeleportMarkerBase>(PlayerPre, action);
        }


        //-------------------------------------------------
        private void Awake()
        {
            _instance = this;

            chaperoneInfoInitializedAction = ChaperoneInfo.InitializedAction(OnChaperoneInfoInitialized);

            pointerLineRenderer = GetComponentInChildren<LineRenderer>();
            teleportPointerObject = pointerLineRenderer.gameObject;

            var tintColorID = Shader.PropertyToID("_TintColor");
            fullTintAlpha = pointVisibleMaterial.GetColor(tintColorID).a;

            teleportArc = GetComponent<TeleportArc>();
            teleportArc.traceLayerMask = traceLayerMask;

            loopingAudioMaxVolume = loopingAudioSource.volume;

            playAreaPreviewCorner.SetActive(false);
            playAreaPreviewSide.SetActive(false);

            var invalidReticleStartingScale = invalidReticleTransform.localScale.x;
            invalidReticleMinScale *= invalidReticleStartingScale;
            invalidReticleMaxScale *= invalidReticleStartingScale;
        }


        //-------------------------------------------------
        private void Start()
        {
            teleportMarkers = FindObjectsOfType<TeleportMarkerBase>();

            HidePointer();

            player = InteractionSystem.Player.instance;

            if (player == null)
            {
                Debug.LogError("Teleport: No Player instance found in map.");
                Destroy(gameObject);
                return;
            }

            CheckForSpawnPoint();

            Invoke("ShowTeleportHint", 5.0f);
        }


        //-------------------------------------------------
        private void OnEnable()
        {
            chaperoneInfoInitializedAction.enabled = true;
            OnChaperoneInfoInitialized(); // In case it's already initialized
        }


        //-------------------------------------------------
        private void OnDisable()
        {
            chaperoneInfoInitializedAction.enabled = false;
            HidePointer();
        }


        //-------------------------------------------------
        private void CheckForSpawnPoint()
        {
            foreach (var teleportMarker in teleportMarkers)
            {
                var teleportPoint = teleportMarker as TeleportPoint;
                if (teleportPoint && teleportPoint.playerSpawnPoint)
                {
                    teleportingToMarker = teleportMarker;
                    TeleportPlayer();
                    break;
                }
            }
        }


        //-------------------------------------------------
        public void HideTeleportPointer()
        {
            if (pointerHand != null) HidePointer();
        }


        //-------------------------------------------------
        private void Update()
        {
            var oldPointerHand = pointerHand;
            Hand newPointerHand = null;

            foreach (var hand in player.hands)
            {
                if (visible)
                    if (WasTeleportButtonReleased(hand))
                        if (pointerHand == hand) //This is the pointer hand
                            TryTeleportPlayer();

                if (WasTeleportButtonPressed(hand)) newPointerHand = hand;
            }

            //If something is attached to the hand that is preventing teleport
            if (allowTeleportWhileAttached && !allowTeleportWhileAttached.teleportAllowed)
            {
                HidePointer();
            }
            else
            {
                if (!visible && newPointerHand != null)
                {
                    //Begin showing the pointer
                    ShowPointer(newPointerHand, oldPointerHand);
                }
                else if (visible)
                {
                    if (newPointerHand == null && !IsTeleportButtonDown(pointerHand))
                        HidePointer();
                    else if (newPointerHand != null) ShowPointer(newPointerHand, oldPointerHand);
                }
            }

            if (visible)
            {
                UpdatePointer();

                if (meshFading) UpdateTeleportColors();

                if (onActivateObjectTransform.gameObject.activeSelf &&
                    Time.time - pointerShowStartTime > activateObjectTime)
                    onActivateObjectTransform.gameObject.SetActive(false);
            }
            else
            {
                if (onDeactivateObjectTransform.gameObject.activeSelf &&
                    Time.time - pointerHideStartTime > deactivateObjectTime)
                    onDeactivateObjectTransform.gameObject.SetActive(false);
            }
        }


        //-------------------------------------------------
        private void UpdatePointer()
        {
            var pointerStart = pointerStartTransform.position;
            Vector3 pointerEnd;
            var pointerDir = pointerStartTransform.forward;
            var hitSomething = false;
            var showPlayAreaPreview = false;
            var playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;

            var arcVelocity = pointerDir * arcDistance;

            TeleportMarkerBase hitTeleportMarker = null;

            //Check pointer angle
            var dotUp = Vector3.Dot(pointerDir, Vector3.up);
            var dotForward = Vector3.Dot(pointerDir, player.hmdTransform.forward);
            var pointerAtBadAngle = false;
            if (dotForward > 0 && dotUp > 0.75f || dotForward < 0.0f && dotUp > 0.5f) pointerAtBadAngle = true;

            //Trace to see if the pointer hit anything
            RaycastHit hitInfo;
            teleportArc.SetArcData(pointerStart, arcVelocity, true, pointerAtBadAngle);
            if (teleportArc.DrawArc(out hitInfo))
            {
                hitSomething = true;
                hitTeleportMarker = hitInfo.collider.GetComponentInParent<TeleportMarkerBase>();
            }

            if (pointerAtBadAngle) hitTeleportMarker = null;

            HighlightSelected(hitTeleportMarker);

            if (hitTeleportMarker != null) //Hit a teleport marker
            {
                if (hitTeleportMarker.locked)
                {
                    teleportArc.SetColor(pointerLockedColor);
#if (UNITY_5_4)
					pointerLineRenderer.SetColors( pointerLockedColor, pointerLockedColor );
#else
                    pointerLineRenderer.startColor = pointerLockedColor;
                    pointerLineRenderer.endColor = pointerLockedColor;
#endif
                    destinationReticleTransform.gameObject.SetActive(false);
                }
                else
                {
                    teleportArc.SetColor(pointerValidColor);
#if (UNITY_5_4)
					pointerLineRenderer.SetColors( pointerValidColor, pointerValidColor );
#else
                    pointerLineRenderer.startColor = pointerValidColor;
                    pointerLineRenderer.endColor = pointerValidColor;
#endif
                    destinationReticleTransform.gameObject.SetActive(hitTeleportMarker.showReticle);
                }

                offsetReticleTransform.gameObject.SetActive(true);

                invalidReticleTransform.gameObject.SetActive(false);

                pointedAtTeleportMarker = hitTeleportMarker;
                pointedAtPosition = hitInfo.point;

                if (showPlayAreaMarker)
                {
                    //Show the play area marker if this is a teleport area
                    var teleportArea = pointedAtTeleportMarker as TeleportArea;
                    if (teleportArea != null && !teleportArea.locked && playAreaPreviewTransform != null)
                    {
                        var offsetToUse = playerFeetOffset;

                        //Adjust the actual offset to prevent the play area marker from moving too much
                        if (!movedFeetFarEnough)
                        {
                            var distanceFromStartingOffset = Vector3.Distance(playerFeetOffset, startingFeetOffset);
                            if (distanceFromStartingOffset < 0.1f)
                                offsetToUse = startingFeetOffset;
                            else if (distanceFromStartingOffset < 0.4f)
                                offsetToUse = Vector3.Lerp(startingFeetOffset, playerFeetOffset,
                                    (distanceFromStartingOffset - 0.1f) / 0.3f);
                            else
                                movedFeetFarEnough = true;
                        }

                        playAreaPreviewTransform.position = pointedAtPosition + offsetToUse;

                        showPlayAreaPreview = true;
                    }
                }

                pointerEnd = hitInfo.point;
            }
            else //Hit neither
            {
                destinationReticleTransform.gameObject.SetActive(false);
                offsetReticleTransform.gameObject.SetActive(false);

                teleportArc.SetColor(pointerInvalidColor);
#if (UNITY_5_4)
				pointerLineRenderer.SetColors( pointerInvalidColor, pointerInvalidColor );
#else
                pointerLineRenderer.startColor = pointerInvalidColor;
                pointerLineRenderer.endColor = pointerInvalidColor;
#endif
                invalidReticleTransform.gameObject.SetActive(!pointerAtBadAngle);

                //Orient the invalid reticle to the normal of the trace hit point
                var normalToUse = hitInfo.normal;
                var angle = Vector3.Angle(hitInfo.normal, Vector3.up);
                if (angle < 15.0f) normalToUse = Vector3.up;
                invalidReticleTargetRotation = Quaternion.FromToRotation(Vector3.up, normalToUse);
                invalidReticleTransform.rotation = Quaternion.Slerp(invalidReticleTransform.rotation,
                    invalidReticleTargetRotation, 0.1f);

                //Scale the invalid reticle based on the distance from the player
                var distanceFromPlayer = Vector3.Distance(hitInfo.point, player.hmdTransform.position);
                var invalidReticleCurrentScale = Util.RemapNumberClamped(distanceFromPlayer,
                    invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, invalidReticleMinScale,
                    invalidReticleMaxScale);
                invalidReticleScale.x = invalidReticleCurrentScale;
                invalidReticleScale.y = invalidReticleCurrentScale;
                invalidReticleScale.z = invalidReticleCurrentScale;
                invalidReticleTransform.transform.localScale = invalidReticleScale;

                pointedAtTeleportMarker = null;

                if (hitSomething)
                    pointerEnd = hitInfo.point;
                else
                    pointerEnd = teleportArc.GetArcPositionAtTime(teleportArc.arcDuration);

                //Debug floor
                if (debugFloor)
                {
                    floorDebugSphere.gameObject.SetActive(false);
                    floorDebugLine.gameObject.SetActive(false);
                }
            }

            if (playAreaPreviewTransform != null) playAreaPreviewTransform.gameObject.SetActive(showPlayAreaPreview);

            if (!showOffsetReticle) offsetReticleTransform.gameObject.SetActive(false);

            destinationReticleTransform.position = pointedAtPosition;
            invalidReticleTransform.position = pointerEnd;
            onActivateObjectTransform.position = pointerEnd;
            onDeactivateObjectTransform.position = pointerEnd;
            offsetReticleTransform.position = pointerEnd - playerFeetOffset;

            reticleAudioSource.transform.position = pointedAtPosition;

            pointerLineRenderer.SetPosition(0, pointerStart);
            pointerLineRenderer.SetPosition(1, pointerEnd);
        }


        //-------------------------------------------------
        private void FixedUpdate()
        {
            if (!visible) return;

            if (debugFloor)
            {
                //Debug floor
                var teleportArea = pointedAtTeleportMarker as TeleportArea;
                if (teleportArea != null)
                    if (floorFixupMaximumTraceDistance > 0.0f)
                    {
                        floorDebugSphere.gameObject.SetActive(true);
                        floorDebugLine.gameObject.SetActive(true);

                        RaycastHit raycastHit;
                        var traceDir = Vector3.down;
                        traceDir.x = 0.01f;
                        if (Physics.Raycast(pointedAtPosition + 0.05f * traceDir, traceDir, out raycastHit,
                            floorFixupMaximumTraceDistance, floorFixupTraceLayerMask))
                        {
                            floorDebugSphere.transform.position = raycastHit.point;
                            floorDebugSphere.material.color = Color.green;
#if (UNITY_5_4)
							floorDebugLine.SetColors( Color.green, Color.green );
#else
                            floorDebugLine.startColor = Color.green;
                            floorDebugLine.endColor = Color.green;
#endif
                            floorDebugLine.SetPosition(0, pointedAtPosition);
                            floorDebugLine.SetPosition(1, raycastHit.point);
                        }
                        else
                        {
                            var rayEnd = pointedAtPosition + traceDir * floorFixupMaximumTraceDistance;
                            floorDebugSphere.transform.position = rayEnd;
                            floorDebugSphere.material.color = Color.red;
#if (UNITY_5_4)
							floorDebugLine.SetColors( Color.red, Color.red );
#else
                            floorDebugLine.startColor = Color.red;
                            floorDebugLine.endColor = Color.red;
#endif
                            floorDebugLine.SetPosition(0, pointedAtPosition);
                            floorDebugLine.SetPosition(1, rayEnd);
                        }
                    }
            }
        }


        //-------------------------------------------------
        private void OnChaperoneInfoInitialized()
        {
            var chaperone = ChaperoneInfo.instance;

            if (chaperone.initialized && chaperone.roomscale)
            {
                //Set up the render model for the play area bounds

                if (playAreaPreviewTransform == null)
                {
                    playAreaPreviewTransform = new GameObject("PlayAreaPreviewTransform").transform;
                    playAreaPreviewTransform.parent = transform;
                    Util.ResetTransform(playAreaPreviewTransform);

                    playAreaPreviewCorner.SetActive(true);
                    playAreaPreviewCorners = new Transform[4];
                    playAreaPreviewCorners[0] = playAreaPreviewCorner.transform;
                    playAreaPreviewCorners[1] = Instantiate(playAreaPreviewCorners[0]);
                    playAreaPreviewCorners[2] = Instantiate(playAreaPreviewCorners[0]);
                    playAreaPreviewCorners[3] = Instantiate(playAreaPreviewCorners[0]);

                    playAreaPreviewCorners[0].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewCorners[1].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewCorners[2].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewCorners[3].transform.parent = playAreaPreviewTransform;

                    playAreaPreviewSide.SetActive(true);
                    playAreaPreviewSides = new Transform[4];
                    playAreaPreviewSides[0] = playAreaPreviewSide.transform;
                    playAreaPreviewSides[1] = Instantiate(playAreaPreviewSides[0]);
                    playAreaPreviewSides[2] = Instantiate(playAreaPreviewSides[0]);
                    playAreaPreviewSides[3] = Instantiate(playAreaPreviewSides[0]);

                    playAreaPreviewSides[0].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewSides[1].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewSides[2].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewSides[3].transform.parent = playAreaPreviewTransform;
                }

                var x = chaperone.playAreaSizeX;
                var z = chaperone.playAreaSizeZ;

                playAreaPreviewSides[0].localPosition = new Vector3(0.0f, 0.0f, 0.5f * z - 0.25f);
                playAreaPreviewSides[1].localPosition = new Vector3(0.0f, 0.0f, -0.5f * z + 0.25f);
                playAreaPreviewSides[2].localPosition = new Vector3(0.5f * x - 0.25f, 0.0f, 0.0f);
                playAreaPreviewSides[3].localPosition = new Vector3(-0.5f * x + 0.25f, 0.0f, 0.0f);

                playAreaPreviewSides[0].localScale = new Vector3(x - 0.5f, 1.0f, 1.0f);
                playAreaPreviewSides[1].localScale = new Vector3(x - 0.5f, 1.0f, 1.0f);
                playAreaPreviewSides[2].localScale = new Vector3(z - 0.5f, 1.0f, 1.0f);
                playAreaPreviewSides[3].localScale = new Vector3(z - 0.5f, 1.0f, 1.0f);

                playAreaPreviewSides[0].localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                playAreaPreviewSides[1].localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                playAreaPreviewSides[2].localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                playAreaPreviewSides[3].localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);

                playAreaPreviewCorners[0].localPosition = new Vector3(0.5f * x - 0.25f, 0.0f, 0.5f * z - 0.25f);
                playAreaPreviewCorners[1].localPosition = new Vector3(0.5f * x - 0.25f, 0.0f, -0.5f * z + 0.25f);
                playAreaPreviewCorners[2].localPosition = new Vector3(-0.5f * x + 0.25f, 0.0f, -0.5f * z + 0.25f);
                playAreaPreviewCorners[3].localPosition = new Vector3(-0.5f * x + 0.25f, 0.0f, 0.5f * z - 0.25f);

                playAreaPreviewCorners[0].localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                playAreaPreviewCorners[1].localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                playAreaPreviewCorners[2].localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                playAreaPreviewCorners[3].localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);

                playAreaPreviewTransform.gameObject.SetActive(false);
            }
        }


        //-------------------------------------------------
        private void HidePointer()
        {
            if (visible) pointerHideStartTime = Time.time;

            visible = false;
            if (pointerHand)
            {
                if (ShouldOverrideHoverLock())
                {
                    //Restore the original hovering interactable on the hand
                    if (originalHoverLockState)
                        pointerHand.HoverLock(originalHoveringInteractable);
                    else
                        pointerHand.HoverUnlock(null);
                }

                //Stop looping sound
                loopingAudioSource.Stop();
                PlayAudioClip(pointerAudioSource, pointerStopSound);
            }

            teleportPointerObject.SetActive(false);

            teleportArc.Hide();

            foreach (var teleportMarker in teleportMarkers)
                if (teleportMarker != null && teleportMarker.markerActive && teleportMarker.gameObject != null)
                    teleportMarker.gameObject.SetActive(false);

            destinationReticleTransform.gameObject.SetActive(false);
            invalidReticleTransform.gameObject.SetActive(false);
            offsetReticleTransform.gameObject.SetActive(false);

            if (playAreaPreviewTransform != null) playAreaPreviewTransform.gameObject.SetActive(false);

            if (onActivateObjectTransform.gameObject.activeSelf) onActivateObjectTransform.gameObject.SetActive(false);
            onDeactivateObjectTransform.gameObject.SetActive(true);

            pointerHand = null;
        }


        //-------------------------------------------------
        private void ShowPointer(Hand newPointerHand, Hand oldPointerHand)
        {
            if (!visible)
            {
                pointedAtTeleportMarker = null;
                pointerShowStartTime = Time.time;
                visible = true;
                meshFading = true;

                teleportPointerObject.SetActive(false);
                teleportArc.Show();

                foreach (var teleportMarker in teleportMarkers)
                    if (teleportMarker.markerActive && teleportMarker.ShouldActivate(player.feetPositionGuess))
                    {
                        teleportMarker.gameObject.SetActive(true);
                        teleportMarker.Highlight(false);
                    }

                startingFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
                movedFeetFarEnough = false;

                if (onDeactivateObjectTransform.gameObject.activeSelf)
                    onDeactivateObjectTransform.gameObject.SetActive(false);
                onActivateObjectTransform.gameObject.SetActive(true);

                loopingAudioSource.clip = pointerLoopSound;
                loopingAudioSource.loop = true;
                loopingAudioSource.Play();
                loopingAudioSource.volume = 0.0f;
            }


            if (oldPointerHand)
                if (ShouldOverrideHoverLock())
                {
                    //Restore the original hovering interactable on the hand
                    if (originalHoverLockState)
                        oldPointerHand.HoverLock(originalHoveringInteractable);
                    else
                        oldPointerHand.HoverUnlock(null);
                }

            pointerHand = newPointerHand;

            if (visible && oldPointerHand != pointerHand) PlayAudioClip(pointerAudioSource, pointerStartSound);

            if (pointerHand)
            {
                pointerStartTransform = GetPointerStartTransform(pointerHand);

                if (pointerHand.currentAttachedObject != null)
                    allowTeleportWhileAttached =
                        pointerHand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();

                //Keep track of any existing hovering interactable on the hand
                originalHoverLockState = pointerHand.hoverLocked;
                originalHoveringInteractable = pointerHand.hoveringInteractable;

                if (ShouldOverrideHoverLock()) pointerHand.HoverLock(null);

                pointerAudioSource.transform.SetParent(pointerStartTransform);
                pointerAudioSource.transform.localPosition = Vector3.zero;

                loopingAudioSource.transform.SetParent(pointerStartTransform);
                loopingAudioSource.transform.localPosition = Vector3.zero;
            }
        }


        //-------------------------------------------------
        private void UpdateTeleportColors()
        {
            var deltaTime = Time.time - pointerShowStartTime;
            if (deltaTime > meshFadeTime)
            {
                meshAlphaPercent = 1.0f;
                meshFading = false;
            }
            else
            {
                meshAlphaPercent = Mathf.Lerp(0.0f, 1.0f, deltaTime / meshFadeTime);
            }

            //Tint color for the teleport points
            foreach (var teleportMarker in teleportMarkers)
                teleportMarker.SetAlpha(fullTintAlpha * meshAlphaPercent, meshAlphaPercent);
        }


        //-------------------------------------------------
        private void PlayAudioClip(AudioSource source, AudioClip clip)
        {
            source.clip = clip;
            source.Play();
        }


        //-------------------------------------------------
        private void PlayPointerHaptic(bool validLocation)
        {
            if (pointerHand.controller != null)
            {
                if (validLocation)
                    pointerHand.controller.TriggerHapticPulse(800);
                else
                    pointerHand.controller.TriggerHapticPulse(100);
            }
        }


        //-------------------------------------------------
        private void TryTeleportPlayer()
        {
            if (visible && !teleporting)
                if (pointedAtTeleportMarker != null && pointedAtTeleportMarker.locked == false)
                {
                    //Pointing at an unlocked teleport marker
                    teleportingToMarker = pointedAtTeleportMarker;
                    InitiateTeleportFade();

                    CancelTeleportHint();
                }
        }


        //-------------------------------------------------
        private void InitiateTeleportFade()
        {
            teleporting = true;

            currentFadeTime = teleportFadeTime;

            var teleportPoint = teleportingToMarker as TeleportPoint;
            if (teleportPoint != null && teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene)
            {
                currentFadeTime *= 3.0f;
                ChangeScene.Send(currentFadeTime);
            }

            SteamVR_Fade.Start(Color.clear, 0);
            SteamVR_Fade.Start(Color.black, currentFadeTime);

            headAudioSource.transform.SetParent(player.hmdTransform);
            headAudioSource.transform.localPosition = Vector3.zero;
            PlayAudioClip(headAudioSource, teleportSound);

            Invoke("TeleportPlayer", currentFadeTime);
        }


        //-------------------------------------------------
        private void TeleportPlayer()
        {
            teleporting = false;

            PlayerPre.Send(pointedAtTeleportMarker);

            SteamVR_Fade.Start(Color.clear, currentFadeTime);

            var teleportPoint = teleportingToMarker as TeleportPoint;
            var teleportPosition = pointedAtPosition;

            if (teleportPoint != null)
            {
                teleportPosition = teleportPoint.transform.position;

                //Teleport to a new scene
                if (teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene)
                {
                    teleportPoint.TeleportToScene();
                    return;
                }
            }

            // Find the actual floor position below the navigation mesh
            var teleportArea = teleportingToMarker as TeleportArea;
            if (teleportArea != null)
                if (floorFixupMaximumTraceDistance > 0.0f)
                {
                    RaycastHit raycastHit;
                    if (Physics.Raycast(teleportPosition + 0.05f * Vector3.down, Vector3.down, out raycastHit,
                        floorFixupMaximumTraceDistance, floorFixupTraceLayerMask)) teleportPosition = raycastHit.point;
                }

            if (teleportingToMarker.ShouldMovePlayer())
            {
                var playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
                player.trackingOriginTransform.position = teleportPosition + playerFeetOffset;
            }
            else
            {
                teleportingToMarker.TeleportPlayer(pointedAtPosition);
            }

            Player.Send(pointedAtTeleportMarker);
        }


        //-------------------------------------------------
        private void HighlightSelected(TeleportMarkerBase hitTeleportMarker)
        {
            if (pointedAtTeleportMarker != hitTeleportMarker) //Pointing at a new teleport marker
            {
                if (pointedAtTeleportMarker != null) pointedAtTeleportMarker.Highlight(false);

                if (hitTeleportMarker != null)
                {
                    hitTeleportMarker.Highlight(true);

                    prevPointedAtPosition = pointedAtPosition;
                    PlayPointerHaptic(!hitTeleportMarker.locked);

                    PlayAudioClip(reticleAudioSource, goodHighlightSound);

                    loopingAudioSource.volume = loopingAudioMaxVolume;
                }
                else if (pointedAtTeleportMarker != null)
                {
                    PlayAudioClip(reticleAudioSource, badHighlightSound);

                    loopingAudioSource.volume = 0.0f;
                }
            }
            else if (hitTeleportMarker != null) //Pointing at the same teleport marker
            {
                if (Vector3.Distance(prevPointedAtPosition, pointedAtPosition) > 1.0f)
                {
                    prevPointedAtPosition = pointedAtPosition;
                    PlayPointerHaptic(!hitTeleportMarker.locked);
                }
            }
        }


        //-------------------------------------------------
        public void ShowTeleportHint()
        {
            CancelTeleportHint();

            hintCoroutine = StartCoroutine(TeleportHintCoroutine());
        }


        //-------------------------------------------------
        public void CancelTeleportHint()
        {
            if (hintCoroutine != null)
            {
                foreach (var hand in player.hands)
                    ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_SteamVR_Touchpad);

                StopCoroutine(hintCoroutine);
                hintCoroutine = null;
            }

            CancelInvoke("ShowTeleportHint");
        }


        //-------------------------------------------------
        private IEnumerator TeleportHintCoroutine()
        {
            var prevBreakTime = Time.time;
            var prevHapticPulseTime = Time.time;

            while (true)
            {
                var pulsed = false;

                //Show the hint on each eligible hand
                foreach (var hand in player.hands)
                {
                    var showHint = IsEligibleForTeleport(hand);
                    var isShowingHint =
                        !string.IsNullOrEmpty(
                            ControllerButtonHints.GetActiveHintText(hand, EVRButtonId.k_EButton_SteamVR_Touchpad));
                    if (showHint)
                    {
                        if (!isShowingHint)
                        {
                            ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_SteamVR_Touchpad,
                                "Teleport");
                            prevBreakTime = Time.time;
                            prevHapticPulseTime = Time.time;
                        }

                        if (Time.time > prevHapticPulseTime + 0.05f)
                        {
                            //Haptic pulse for a few seconds
                            pulsed = true;

                            hand.controller.TriggerHapticPulse(500);
                        }
                    }
                    else if (!showHint && isShowingHint)
                    {
                        ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_SteamVR_Touchpad);
                    }
                }

                if (Time.time > prevBreakTime + 3.0f)
                {
                    //Take a break for a few seconds
                    yield return new WaitForSeconds(3.0f);

                    prevBreakTime = Time.time;
                }

                if (pulsed) prevHapticPulseTime = Time.time;

                yield return null;
            }
        }


        //-------------------------------------------------
        public bool IsEligibleForTeleport(Hand hand)
        {
            if (hand == null) return false;

            if (!hand.gameObject.activeInHierarchy) return false;

            if (hand.hoveringInteractable != null) return false;

            if (hand.noSteamVRFallbackCamera == null)
            {
                if (hand.controller == null) return false;

                //Something is attached to the hand
                if (hand.currentAttachedObject != null)
                {
                    var allowTeleportWhileAttachedToHand =
                        hand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();

                    if (allowTeleportWhileAttachedToHand != null && allowTeleportWhileAttachedToHand.teleportAllowed)
                        return true;
                    return false;
                }
            }

            return true;
        }


        //-------------------------------------------------
        private bool ShouldOverrideHoverLock()
        {
            if (!allowTeleportWhileAttached || allowTeleportWhileAttached.overrideHoverLock) return true;

            return false;
        }


        //-------------------------------------------------
        private bool WasTeleportButtonReleased(Hand hand)
        {
            if (IsEligibleForTeleport(hand))
            {
                if (hand.noSteamVRFallbackCamera != null)
                    return Input.GetKeyUp(KeyCode.T);
                return hand.controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
            }

            return false;
        }


        //-------------------------------------------------
        private bool IsTeleportButtonDown(Hand hand)
        {
            if (IsEligibleForTeleport(hand))
            {
                if (hand.noSteamVRFallbackCamera != null)
                    return Input.GetKey(KeyCode.T);
                return hand.controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
            }

            return false;
        }


        //-------------------------------------------------
        private bool WasTeleportButtonPressed(Hand hand)
        {
            if (IsEligibleForTeleport(hand))
            {
                if (hand.noSteamVRFallbackCamera != null)
                    return Input.GetKeyDown(KeyCode.T);
                return hand.controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
            }

            return false;
        }


        //-------------------------------------------------
        private Transform GetPointerStartTransform(Hand hand)
        {
            if (hand.noSteamVRFallbackCamera != null)
                return hand.noSteamVRFallbackCamera.transform;
            return pointerHand.GetAttachmentTransform("Attach_ControllerTip");
        }
    }
}
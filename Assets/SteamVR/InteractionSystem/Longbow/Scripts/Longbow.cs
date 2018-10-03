//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The bow
//
//=============================================================================

using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class Longbow : MonoBehaviour
    {
        public enum Handedness
        {
            Left,
            Right
        }

        private const float minPull = 0.05f;
        private const float maxPull = 0.5f;
        private const float bowPullPulseStrengthLow = 100;
        private const float bowPullPulseStrengthHigh = 500;
        private ArrowHand arrowHand;
        public ItemPackage arrowHandItemPackage;
        public GameObject arrowHandPrefab;
        public float arrowMaxVelocity = 30f;

        public float arrowMinVelocity = 3f;
        public SoundPlayOneshot arrowSlideSound;
        private float arrowVelocity = 30f;

        public bool autoSpawnArrowHand = true;

        public LinearMapping bowDrawLinearMapping;
        private Vector3 bowLeftVector;

        public Handedness currentHandGuess = Handedness.Left;

        private bool deferNewPoses;

        public float drawOffset = 0.06f;

        public SoundBowClick drawSound;
        private float drawTension;

        private Hand hand;
        public Transform handleTransform;
        private readonly float hapticDistanceThreshold = 0.01f;
        private float lastTickDistance;
        private Vector3 lateUpdatePos;
        private Quaternion lateUpdateRot;

        private bool lerpBackToZeroRotation;
        private readonly float lerpDuration = 0.15f;
        private Quaternion lerpStartRotation;
        private float lerpStartTime;
        private readonly float maxStrainTickTime = 0.5f;

        private readonly float minStrainTickTime = 0.1f;

        private SteamVR_Events.Action newPosesAppliedAction;
        private float nextStrainTick;
        private float nockDistanceTravelled;

        public bool nocked;

        private Quaternion nockLerpStartRotation;

        private float nockLerpStartTime;
        public Transform nockRestTransform;
        public SoundPlayOneshot nockSound;

        public Transform nockTransform;

        public Transform pivotTransform;
        private bool possibleHandSwitch;
        public bool pulled;
        public SoundPlayOneshot releaseSound;
        private readonly float timeBeforeConfirmingHandSwitch = 1.5f;
        private float timeOfPossibleHandSwitch;


        //-------------------------------------------------
        private void OnAttachedToHand(Hand attachedHand)
        {
            hand = attachedHand;
        }


        //-------------------------------------------------
        private void Awake()
        {
            newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        }


        //-------------------------------------------------
        private void OnEnable()
        {
            newPosesAppliedAction.enabled = true;
        }


        //-------------------------------------------------
        private void OnDisable()
        {
            newPosesAppliedAction.enabled = false;
        }


        //-------------------------------------------------
        private void LateUpdate()
        {
            if (deferNewPoses)
            {
                lateUpdatePos = transform.position;
                lateUpdateRot = transform.rotation;
            }
        }


        //-------------------------------------------------
        private void OnNewPosesApplied()
        {
            if (deferNewPoses)
            {
                // Set longbow object back to previous pose position to avoid jitter
                transform.position = lateUpdatePos;
                transform.rotation = lateUpdateRot;

                deferNewPoses = false;
            }
        }


        //-------------------------------------------------
        private void HandAttachedUpdate(Hand hand)
        {
            // Reset transform since we cheated it right after getting poses on previous frame
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Update handedness guess
            EvaluateHandedness();

            if (nocked)
            {
                deferNewPoses = true;

                var nockToarrowHand =
                    arrowHand.arrowNockTransform.parent.position -
                    nockRestTransform
                        .position; // Vector from bow nock transform to arrowhand nock transform - used to align bow when drawing

                // Align bow
                // Time lerp value used for ramping into drawn bow orientation
                var lerp = Util.RemapNumberClamped(Time.time, nockLerpStartTime, nockLerpStartTime + lerpDuration, 0f,
                    1f);

                var pullLerp =
                    Util.RemapNumberClamped(nockToarrowHand.magnitude, minPull, maxPull, 0f,
                        1f); // Normalized current state of bow draw 0 - 1

                var arrowNockTransformToHeadset =
                    (Player.instance.hmdTransform.position + Vector3.down * 0.05f -
                     arrowHand.arrowNockTransform.parent.position).normalized;
                var arrowHandPosition = arrowHand.arrowNockTransform.parent.position +
                                        arrowNockTransformToHeadset * drawOffset *
                                        pullLerp; // Use this line to lerp arrowHand nock position
                //Vector3 arrowHandPosition = arrowHand.arrowNockTransform.position; // Use this line if we don't want to lerp arrowHand nock position

                var pivotToString = (arrowHandPosition - pivotTransform.position).normalized;
                var pivotToLowerHandle = (handleTransform.position - pivotTransform.position).normalized;
                bowLeftVector = -Vector3.Cross(pivotToLowerHandle, pivotToString);
                pivotTransform.rotation = Quaternion.Lerp(nockLerpStartRotation,
                    Quaternion.LookRotation(pivotToString, bowLeftVector), lerp);

                // Move nock position
                if (Vector3.Dot(nockToarrowHand, -nockTransform.forward) > 0)
                {
                    var distanceToarrowHand = nockToarrowHand.magnitude * lerp;

                    nockTransform.localPosition = new Vector3(0f, 0f, Mathf.Clamp(-distanceToarrowHand, -maxPull, 0f));

                    nockDistanceTravelled = -nockTransform.localPosition.z;

                    arrowVelocity = Util.RemapNumber(nockDistanceTravelled, minPull, maxPull, arrowMinVelocity,
                        arrowMaxVelocity);

                    drawTension = Util.RemapNumberClamped(nockDistanceTravelled, 0, maxPull, 0f, 1f);

                    bowDrawLinearMapping.value =
                        drawTension; // Send drawTension value to LinearMapping script, which drives the bow draw animation

                    if (nockDistanceTravelled > minPull)
                        pulled = true;
                    else
                        pulled = false;

                    if (nockDistanceTravelled > lastTickDistance + hapticDistanceThreshold ||
                        nockDistanceTravelled < lastTickDistance - hapticDistanceThreshold)
                    {
                        var hapticStrength = (ushort) Util.RemapNumber(nockDistanceTravelled, 0, maxPull,
                            bowPullPulseStrengthLow, bowPullPulseStrengthHigh);
                        hand.controller.TriggerHapticPulse(hapticStrength);
                        hand.otherHand.controller.TriggerHapticPulse(hapticStrength);

                        drawSound.PlayBowTensionClicks(drawTension);

                        lastTickDistance = nockDistanceTravelled;
                    }

                    if (nockDistanceTravelled >= maxPull)
                        if (Time.time > nextStrainTick)
                        {
                            hand.controller.TriggerHapticPulse(400);
                            hand.otherHand.controller.TriggerHapticPulse(400);

                            drawSound.PlayBowTensionClicks(drawTension);

                            nextStrainTick = Time.time + Random.Range(minStrainTickTime, maxStrainTickTime);
                        }
                }
                else
                {
                    nockTransform.localPosition = new Vector3(0f, 0f, 0f);

                    bowDrawLinearMapping.value = 0f;
                }
            }
            else
            {
                if (lerpBackToZeroRotation)
                {
                    var lerp = Util.RemapNumber(Time.time, lerpStartTime, lerpStartTime + lerpDuration, 0, 1);

                    pivotTransform.localRotation = Quaternion.Lerp(lerpStartRotation, Quaternion.identity, lerp);

                    if (lerp >= 1) lerpBackToZeroRotation = false;
                }
            }
        }


        //-------------------------------------------------
        public void ArrowReleased()
        {
            nocked = false;
            hand.HoverUnlock(GetComponent<Interactable>());
            hand.otherHand.HoverUnlock(arrowHand.GetComponent<Interactable>());

            if (releaseSound != null) releaseSound.Play();

            StartCoroutine(ResetDrawAnim());
        }


        //-------------------------------------------------
        private IEnumerator ResetDrawAnim()
        {
            var startTime = Time.time;
            var startLerp = drawTension;

            while (Time.time < startTime + 0.02f)
            {
                var lerp = Util.RemapNumberClamped(Time.time, startTime, startTime + 0.02f, startLerp, 0f);
                bowDrawLinearMapping.value = lerp;
                yield return null;
            }

            bowDrawLinearMapping.value = 0;
        }


        //-------------------------------------------------
        public float GetArrowVelocity()
        {
            return arrowVelocity;
        }


        //-------------------------------------------------
        public void StartRotationLerp()
        {
            lerpStartTime = Time.time;
            lerpBackToZeroRotation = true;
            lerpStartRotation = pivotTransform.localRotation;

            Util.ResetTransform(nockTransform);
        }


        //-------------------------------------------------
        public void StartNock(ArrowHand currentArrowHand)
        {
            arrowHand = currentArrowHand;
            hand.HoverLock(GetComponent<Interactable>());
            nocked = true;
            nockLerpStartTime = Time.time;
            nockLerpStartRotation = pivotTransform.rotation;

            // Sound of arrow sliding on nock as it's being pulled back
            arrowSlideSound.Play();

            // Decide which hand we're drawing with and lerp to the correct side
            DoHandednessCheck();
        }


        //-------------------------------------------------
        private void EvaluateHandedness()
        {
            var handType = hand.GuessCurrentHandType();

            if (handType == Hand.HandType.Left) // Bow hand is further left than arrow hand.
            {
                // We were considering a switch, but the current controller orientation matches our currently assigned handedness, so no longer consider a switch
                if (possibleHandSwitch && currentHandGuess == Handedness.Left) possibleHandSwitch = false;

                // If we previously thought the bow was right-handed, and were not already considering switching, start considering a switch
                if (!possibleHandSwitch && currentHandGuess == Handedness.Right)
                {
                    possibleHandSwitch = true;
                    timeOfPossibleHandSwitch = Time.time;
                }

                // If we are considering a handedness switch, and it's been this way long enough, switch
                if (possibleHandSwitch && Time.time > timeOfPossibleHandSwitch + timeBeforeConfirmingHandSwitch)
                {
                    currentHandGuess = Handedness.Left;
                    possibleHandSwitch = false;
                }
            }
            else // Bow hand is further right than arrow hand
            {
                // We were considering a switch, but the current controller orientation matches our currently assigned handedness, so no longer consider a switch
                if (possibleHandSwitch && currentHandGuess == Handedness.Right) possibleHandSwitch = false;

                // If we previously thought the bow was right-handed, and were not already considering switching, start considering a switch
                if (!possibleHandSwitch && currentHandGuess == Handedness.Left)
                {
                    possibleHandSwitch = true;
                    timeOfPossibleHandSwitch = Time.time;
                }

                // If we are considering a handedness switch, and it's been this way long enough, switch
                if (possibleHandSwitch && Time.time > timeOfPossibleHandSwitch + timeBeforeConfirmingHandSwitch)
                {
                    currentHandGuess = Handedness.Right;
                    possibleHandSwitch = false;
                }
            }
        }


        //-------------------------------------------------
        private void DoHandednessCheck()
        {
            // Based on our current best guess about hand, switch bow orientation and arrow lerp direction
            if (currentHandGuess == Handedness.Left)
                pivotTransform.localScale = new Vector3(1f, 1f, 1f);
            else
                pivotTransform.localScale = new Vector3(1f, -1f, 1f);
        }


        //-------------------------------------------------
        public void ArrowInPosition()
        {
            DoHandednessCheck();

            if (nockSound != null) nockSound.Play();
        }


        //-------------------------------------------------
        public void ReleaseNock()
        {
            // ArrowHand tells us to do this when we release the buttons when bow is nocked but not drawn far enough
            nocked = false;
            hand.HoverUnlock(GetComponent<Interactable>());
            StartCoroutine(ResetDrawAnim());
        }


        //-------------------------------------------------
        private void ShutDown()
        {
            if (hand != null && hand.otherHand.currentAttachedObject != null)
                if (hand.otherHand.currentAttachedObject.GetComponent<ItemPackageReference>() != null)
                    if (hand.otherHand.currentAttachedObject.GetComponent<ItemPackageReference>().itemPackage ==
                        arrowHandItemPackage)
                        hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
        }


        //-------------------------------------------------
        private void OnHandFocusLost(Hand hand)
        {
            gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private void OnHandFocusAcquired(Hand hand)
        {
            gameObject.SetActive(true);
            OnAttachedToHand(hand);
        }


        //-------------------------------------------------
        private void OnDetachedFromHand(Hand hand)
        {
            Destroy(gameObject);
        }


        //-------------------------------------------------
        private void OnDestroy()
        {
            ShutDown();
        }
    }
}
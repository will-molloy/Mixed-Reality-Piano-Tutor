//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(VelocityEstimator))]
    public class Throwable : MonoBehaviour
    {
        public bool attachEaseIn;
        public string[] attachEaseInAttachmentNames;
        private Transform attachEaseInTransform;
        private bool attached;

        [EnumFlags] [Tooltip("The flags used to attach this object to the hand.")]
        public Hand.AttachmentFlags attachmentFlags =
            Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachFromOtherHand;

        [Tooltip(
            "Name of the attachment transform under in the hand's hierarchy which the object should should snap to.")]
        public string attachmentPoint;

        private Vector3 attachPosition;
        private Quaternion attachRotation;
        private float attachTime;

        [Tooltip("How fast must this object be moving to attach due to a trigger hold instead of a trigger press?")]
        public float catchSpeedThreshold;

        public UnityEvent onDetachFromHand;

        public UnityEvent onPickUp;

        [Tooltip("When detaching the object, should it return to its original parent?")]
        public bool restoreOriginalParent;

        public bool snapAttachEaseInCompleted;
        public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float snapAttachEaseInTime = 0.15f;

        private VelocityEstimator velocityEstimator;


        //-------------------------------------------------
        private void Awake()
        {
            velocityEstimator = GetComponent<VelocityEstimator>();

            if (attachEaseIn) attachmentFlags &= ~Hand.AttachmentFlags.SnapOnAttach;

            var rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = 50.0f;
        }


        //-------------------------------------------------
        private void OnHandHoverBegin(Hand hand)
        {
            var showHint = false;

            // "Catch" the throwable by holding down the interaction button instead of pressing it.
            // Only do this if the throwable is moving faster than the prescribed threshold speed,
            // and if it isn't attached to another hand
            if (!attached)
                if (hand.GetStandardInteractionButton())
                {
                    var rb = GetComponent<Rigidbody>();
                    if (rb.velocity.magnitude >= catchSpeedThreshold)
                    {
                        hand.AttachObject(gameObject, attachmentFlags, attachmentPoint);
                        showHint = false;
                    }
                }

            if (showHint) ControllerButtonHints.ShowButtonHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger);
        }


        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            ControllerButtonHints.HideButtonHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger);
        }


        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            //Trigger got pressed
            if (hand.GetStandardInteractionButtonDown())
            {
                hand.AttachObject(gameObject, attachmentFlags, attachmentPoint);
                ControllerButtonHints.HideButtonHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger);
            }
        }

        //-------------------------------------------------
        private void OnAttachedToHand(Hand hand)
        {
            attached = true;

            onPickUp.Invoke();

            hand.HoverLock(null);

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;

            if (hand.controller == null) velocityEstimator.BeginEstimatingVelocity();

            attachTime = Time.time;
            attachPosition = transform.position;
            attachRotation = transform.rotation;

            if (attachEaseIn)
            {
                attachEaseInTransform = hand.transform;
                if (!Util.IsNullOrEmpty(attachEaseInAttachmentNames))
                {
                    var smallestAngle = float.MaxValue;
                    for (var i = 0; i < attachEaseInAttachmentNames.Length; i++)
                    {
                        var t = hand.GetAttachmentTransform(attachEaseInAttachmentNames[i]);
                        var angle = Quaternion.Angle(t.rotation, attachRotation);
                        if (angle < smallestAngle)
                        {
                            attachEaseInTransform = t;
                            smallestAngle = angle;
                        }
                    }
                }
            }

            snapAttachEaseInCompleted = false;
        }


        //-------------------------------------------------
        private void OnDetachedFromHand(Hand hand)
        {
            attached = false;

            onDetachFromHand.Invoke();

            hand.HoverUnlock(null);

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var position = Vector3.zero;
            var velocity = Vector3.zero;
            var angularVelocity = Vector3.zero;
            if (hand.controller == null)
            {
                velocityEstimator.FinishEstimatingVelocity();
                velocity = velocityEstimator.GetVelocityEstimate();
                angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                position = velocityEstimator.transform.position;
            }
            else
            {
                velocity = Player.instance.trackingOriginTransform.TransformVector(hand.controller.velocity);
                angularVelocity =
                    Player.instance.trackingOriginTransform.TransformVector(hand.controller.angularVelocity);
                position = hand.transform.position;
            }

            var r = transform.TransformPoint(rb.centerOfMass) - position;
            rb.velocity = velocity + Vector3.Cross(angularVelocity, r);
            rb.angularVelocity = angularVelocity;

            // Make the object travel at the release velocity for the amount
            // of time it will take until the next fixed update, at which
            // point Unity physics will take over
            var timeUntilFixedUpdate = Time.fixedDeltaTime + Time.fixedTime - Time.time;
            transform.position += timeUntilFixedUpdate * velocity;
            var angle = Mathf.Rad2Deg * angularVelocity.magnitude;
            var axis = angularVelocity.normalized;
            transform.rotation *= Quaternion.AngleAxis(angle * timeUntilFixedUpdate, axis);
        }


        //-------------------------------------------------
        private void HandAttachedUpdate(Hand hand)
        {
            //Trigger got released
            if (!hand.GetStandardInteractionButton()) StartCoroutine(LateDetach(hand));

            if (attachEaseIn)
            {
                var t = Util.RemapNumberClamped(Time.time, attachTime, attachTime + snapAttachEaseInTime, 0.0f, 1.0f);
                if (t < 1.0f)
                {
                    t = snapAttachEaseInCurve.Evaluate(t);
                    transform.position = Vector3.Lerp(attachPosition, attachEaseInTransform.position, t);
                    transform.rotation = Quaternion.Lerp(attachRotation, attachEaseInTransform.rotation, t);
                }
                else if (!snapAttachEaseInCompleted)
                {
                    gameObject.SendMessage("OnThrowableAttachEaseInCompleted", hand,
                        SendMessageOptions.DontRequireReceiver);
                    snapAttachEaseInCompleted = true;
                }
            }
        }


        //-------------------------------------------------
        private IEnumerator LateDetach(Hand hand)
        {
            yield return new WaitForEndOfFrame();

            hand.DetachObject(gameObject, restoreOriginalParent);
        }


        //-------------------------------------------------
        private void OnHandFocusAcquired(Hand hand)
        {
            gameObject.SetActive(true);
            velocityEstimator.BeginEstimatingVelocity();
        }


        //-------------------------------------------------
        private void OnHandFocusLost(Hand hand)
        {
            gameObject.SetActive(false);
            velocityEstimator.FinishEstimatingVelocity();
        }
    }
}
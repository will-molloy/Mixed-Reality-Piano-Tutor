//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles the spawning and returning of the ItemPackage
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR

#endif

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class ItemPackageSpawner : MonoBehaviour
    {
        public ItemPackage _itemPackage;

        public bool acceptDifferentItems;

        [EnumFlags] public Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags;

        public string attachmentPoint;
        public UnityEvent dropEvent;
        private bool itemIsSpawned;

        public bool justPickedUpItem;

        public UnityEvent pickupEvent;
        private GameObject previewObject;
        public bool requireTriggerPressToReturn;

        public bool requireTriggerPressToTake;
        public bool showTriggerHint;

        private GameObject spawnedItem;

        public bool
            takeBackItem; // if a hand enters this trigger and has the item this spawner dispenses at the top of the stack, remove it from the stack

        private bool useFadedPreview;

        private readonly bool useItemPackagePreview = true;

        public ItemPackage itemPackage
        {
            get { return _itemPackage; }
            set { CreatePreviewObject(); }
        }


        //-------------------------------------------------
        private void CreatePreviewObject()
        {
            if (!useItemPackagePreview) return;

            ClearPreview();

            if (useItemPackagePreview)
            {
                if (itemPackage == null) return;

                if (useFadedPreview == false) // if we don't have a spawned item out there, use the regular preview
                {
                    if (itemPackage.previewPrefab != null)
                    {
                        previewObject = Instantiate(itemPackage.previewPrefab, transform.position, Quaternion.identity);
                        previewObject.transform.parent = transform;
                        previewObject.transform.localRotation = Quaternion.identity;
                    }
                }
                else // there's a spawned item out there. Use the faded preview
                {
                    if (itemPackage.fadedPreviewPrefab != null)
                    {
                        previewObject = Instantiate(itemPackage.fadedPreviewPrefab, transform.position,
                            Quaternion.identity);
                        previewObject.transform.parent = transform;
                        previewObject.transform.localRotation = Quaternion.identity;
                    }
                }
            }
        }


        //-------------------------------------------------
        private void Start()
        {
            VerifyItemPackage();
        }


        //-------------------------------------------------
        private void VerifyItemPackage()
        {
            if (itemPackage == null) ItemPackageNotValid();

            if (itemPackage.itemPrefab == null) ItemPackageNotValid();
        }


        //-------------------------------------------------
        private void ItemPackageNotValid()
        {
            Debug.LogError("ItemPackage assigned to " + gameObject.name +
                           " is not valid. Destroying this game object.");
            Destroy(gameObject);
        }


        //-------------------------------------------------
        private void ClearPreview()
        {
            foreach (Transform child in transform)
                if (Time.time > 0)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
        }


        //-------------------------------------------------
        private void Update()
        {
            if (itemIsSpawned && spawnedItem == null)
            {
                itemIsSpawned = false;
                useFadedPreview = false;
                dropEvent.Invoke();
                CreatePreviewObject();
            }
        }


        //-------------------------------------------------
        private void OnHandHoverBegin(Hand hand)
        {
            var currentAttachedItemPackage = GetAttachedItemPackage(hand);

            if (currentAttachedItemPackage == itemPackage
            ) // the item at the top of the hand's stack has an associated ItemPackage
                if (takeBackItem && !requireTriggerPressToReturn
                ) // if we want to take back matching items and aren't waiting for a trigger press
                    TakeBackItem(hand);

            if (!requireTriggerPressToTake) // we don't require trigger press for pickup. Spawn and attach object.
                SpawnAndAttachObject(hand);

            if (requireTriggerPressToTake && showTriggerHint)
                ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger, "PickUp");
        }


        //-------------------------------------------------
        private void TakeBackItem(Hand hand)
        {
            RemoveMatchingItemsFromHandStack(itemPackage, hand);

            if (itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded)
                RemoveMatchingItemsFromHandStack(itemPackage, hand.otherHand);
        }


        //-------------------------------------------------
        private ItemPackage GetAttachedItemPackage(Hand hand)
        {
            var currentAttachedObject = hand.currentAttachedObject;

            if (currentAttachedObject == null) // verify the hand is holding something
                return null;

            var packageReference = hand.currentAttachedObject.GetComponent<ItemPackageReference>();
            if (packageReference == null) // verify the item in the hand is matchable
                return null;

            var attachedItemPackage = packageReference.itemPackage; // return the ItemPackage reference we find.

            return attachedItemPackage;
        }


        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            if (takeBackItem && requireTriggerPressToReturn)
                if (hand.controller != null && hand.controller.GetHairTriggerDown())
                {
                    var currentAttachedItemPackage = GetAttachedItemPackage(hand);
                    if (currentAttachedItemPackage == itemPackage)
                    {
                        TakeBackItem(hand);
                        return; // So that we don't pick up an ItemPackage the same frame that we return it
                    }
                }

            if (requireTriggerPressToTake)
                if (hand.controller != null && hand.controller.GetHairTriggerDown())
                    SpawnAndAttachObject(hand);
        }


        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            if (!justPickedUpItem && requireTriggerPressToTake && showTriggerHint)
                ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger);

            justPickedUpItem = false;
        }


        //-------------------------------------------------
        private void RemoveMatchingItemsFromHandStack(ItemPackage package, Hand hand)
        {
            for (var i = 0; i < hand.AttachedObjects.Count; i++)
            {
                var packageReference = hand.AttachedObjects[i].attachedObject.GetComponent<ItemPackageReference>();
                if (packageReference != null)
                {
                    var attachedObjectItemPackage = packageReference.itemPackage;
                    if (attachedObjectItemPackage != null && attachedObjectItemPackage == package)
                    {
                        var detachedItem = hand.AttachedObjects[i].attachedObject;
                        hand.DetachObject(detachedItem);
                    }
                }
            }
        }


        //-------------------------------------------------
        private void RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType packageType, Hand hand)
        {
            for (var i = 0; i < hand.AttachedObjects.Count; i++)
            {
                var packageReference = hand.AttachedObjects[i].attachedObject.GetComponent<ItemPackageReference>();
                if (packageReference != null)
                    if (packageReference.itemPackage.packageType == packageType)
                    {
                        var detachedItem = hand.AttachedObjects[i].attachedObject;
                        hand.DetachObject(detachedItem);
                    }
            }
        }


        //-------------------------------------------------
        private void SpawnAndAttachObject(Hand hand)
        {
            if (hand.otherHand != null)
            {
                //If the other hand has this item package, take it back from the other hand
                var otherHandItemPackage = GetAttachedItemPackage(hand.otherHand);
                if (otherHandItemPackage == itemPackage) TakeBackItem(hand.otherHand);
            }

            if (showTriggerHint) ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger);

            if (itemPackage.otherHandItemPrefab != null)
                if (hand.otherHand.hoverLocked)
                    return;

            // if we're trying to spawn a one-handed item, remove one and two-handed items from this hand and two-handed items from both hands
            if (itemPackage.packageType == ItemPackage.ItemPackageType.OneHanded)
            {
                RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.OneHanded, hand);
                RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand);
                RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand.otherHand);
            }

            // if we're trying to spawn a two-handed item, remove one and two-handed items from both hands
            if (itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded)
            {
                RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.OneHanded, hand);
                RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.OneHanded, hand.otherHand);
                RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand);
                RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand.otherHand);
            }

            spawnedItem = Instantiate(itemPackage.itemPrefab);
            spawnedItem.SetActive(true);
            hand.AttachObject(spawnedItem, attachmentFlags, attachmentPoint);

            if (itemPackage.otherHandItemPrefab != null && hand.otherHand.controller != null)
            {
                var otherHandObjectToAttach = Instantiate(itemPackage.otherHandItemPrefab);
                otherHandObjectToAttach.SetActive(true);
                hand.otherHand.AttachObject(otherHandObjectToAttach, attachmentFlags);
            }

            itemIsSpawned = true;

            justPickedUpItem = true;

            if (takeBackItem)
            {
                useFadedPreview = true;
                pickupEvent.Invoke();
                CreatePreviewObject();
            }
        }
    }
}
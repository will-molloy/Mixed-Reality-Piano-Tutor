//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes the hand act as an input module for Unity's event system
//
//=============================================================================

using UnityEngine;
using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class InputModule : BaseInputModule
    {
        //-------------------------------------------------
        private static InputModule _instance;
        private GameObject submitObject;

        public static InputModule instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<InputModule>();

                return _instance;
            }
        }


        //-------------------------------------------------
        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            return submitObject != null;
        }


        //-------------------------------------------------
        public void HoverBegin(GameObject gameObject)
        {
            var pointerEventData = new PointerEventData(eventSystem);
            ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
        }


        //-------------------------------------------------
        public void HoverEnd(GameObject gameObject)
        {
            var pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.selectedObject = null;
            ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerExitHandler);
        }


        //-------------------------------------------------
        public void Submit(GameObject gameObject)
        {
            submitObject = gameObject;
        }


        //-------------------------------------------------
        public override void Process()
        {
            if (submitObject)
            {
                var data = GetBaseEventData();
                data.selectedObject = submitObject;
                ExecuteEvents.Execute(submitObject, data, ExecuteEvents.submitHandler);

                submitObject = null;
            }
        }
    }
}
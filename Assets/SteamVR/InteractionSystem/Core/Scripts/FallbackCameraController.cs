//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Controls for the non-VR debug camera
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Camera))]
    public class FallbackCameraController : MonoBehaviour
    {
        private float realTime;
        public float shiftSpeed = 16.0f;
        public bool showInstructions = true;
        public float speed = 4.0f;

        private Vector3 startEulerAngles;
        private Vector3 startMousePosition;

        //-------------------------------------------------
        private void OnEnable()
        {
            realTime = Time.realtimeSinceStartup;
        }


        //-------------------------------------------------
        private void Update()
        {
            var forward = 0.0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) forward += 1.0f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) forward -= 1.0f;

            var right = 0.0f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) right += 1.0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) right -= 1.0f;

            var currentSpeed = speed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) currentSpeed = shiftSpeed;

            var realTimeNow = Time.realtimeSinceStartup;
            var deltaRealTime = realTimeNow - realTime;
            realTime = realTimeNow;

            var delta = new Vector3(right, 0.0f, forward) * currentSpeed * deltaRealTime;

            transform.position += transform.TransformDirection(delta);

            var mousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(1) /* right mouse */)
            {
                startMousePosition = mousePosition;
                startEulerAngles = transform.localEulerAngles;
            }

            if (Input.GetMouseButton(1) /* right mouse */)
            {
                var offset = mousePosition - startMousePosition;
                transform.localEulerAngles = startEulerAngles + new Vector3(-offset.y * 360.0f / Screen.height,
                                                 offset.x * 360.0f / Screen.width, 0.0f);
            }
        }


        //-------------------------------------------------
        private void OnGUI()
        {
            if (showInstructions)
                GUI.Label(new Rect(10.0f, 10.0f, 600.0f, 400.0f),
                    "WASD/Arrow Keys to translate the camera\n" +
                    "Right mouse click to rotate the camera\n" +
                    "Left mouse click for standard interactions.\n");
        }
    }
}
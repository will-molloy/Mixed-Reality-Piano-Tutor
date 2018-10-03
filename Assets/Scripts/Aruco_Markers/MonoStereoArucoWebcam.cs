using System;
using UnityEngine;

namespace ArucoUnity.Cameras
{
    /// <summary>
    ///     - Captures image of a stereoscopic webcam which has output as a single feed like ZED-M.
    ///     - ArucoUnity does not support the ZED-M, this is an attempt to get it to work.
    ///     - WORK IN PROGRESS - See ArucoUnity.Cameras source code.
    /// </summary>
    public class MonoStereoArucoWebcam : ArucoCamera
    {
        protected const int cameraId = 0;

        // Editor fields

        [SerializeField] [Tooltip("The id of the webcam to use.")]
        private int webcamId;

        // IArucoCamera properties

        public override int CameraNumber => 1;

        public override string Name { get; protected set; }

        // Properties

        /// <summary>
        ///     Gets or set the id of the webcam to use.
        /// </summary>
        public int WebcamId
        {
            get { return webcamId; }
            set { webcamId = value; }
        }

        /// <summary>
        ///     Gets the controller of the webcam to use.
        /// </summary>
        public WebcamController WebcamController { get; private set; }

        // MonoBehaviour methods

        /// <summary>
        ///     Initializes <see cref="WebcamController" /> and subscribes to.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            WebcamController = gameObject.AddComponent<WebcamController>();
            WebcamController.Started += WebcamController_Started;
        }

        /// <summary>
        ///     Unsubscribes to <see cref="WebcamController" />.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            WebcamController.Started -= WebcamController_Started;
        }

        // ConfigurableController methods

        /// <summary>
        ///     Calls <see cref="WebcamController.Configure" /> and sets <see cref="Name" />.
        /// </summary>
        protected override void Configuring()
        {
            base.Configuring();

            WebcamController.Ids.Clear();
            WebcamController.Ids.Add(WebcamId);
            WebcamController.Configure();

            Name = WebcamController.Devices[cameraId].name;
        }

        /// <summary>
        ///     Calls <see cref="WebcamController.StartWebcams" />.
        /// </summary>
        protected override void Starting()
        {
            base.Starting();
            WebcamController.StartWebcams();
        }

        /// <summary>
        ///     Calls <see cref="WebcamController.StopWebcams" />.
        /// </summary>
        protected override void Stopping()
        {
            base.Stopping();
            WebcamController.StopWebcams();
        }

        /// <summary>
        ///     Blocks <see cref="ArucoCamera.OnStarted" /> until <see cref="WebcamController.IsStarted" />.
        /// </summary>
        protected override void OnStarted()
        {
        }

        // ArucoCamera methods

        /// <summary>
        ///     Copy current webcam images to <see cref="ArucoCamera.NextImages" />.
        /// </summary>
        protected override bool UpdatingImages()
        {
            var textureData = WebcamController.Textures2D[cameraId];
            var width = textureData.width / 2;
            var height = textureData.height;
            var leftData = textureData.GetPixels(0, 0, width, height);
            var rightData = textureData.GetPixels(width, 0, width, height);
            var leftTexture = new Texture2D(width, height);
            leftTexture.SetPixels(leftData);
            var rightTexture = new Texture2D(width, height);
            rightTexture.SetPixels(rightData);
            Array.Copy(leftTexture.GetRawTextureData(), NextImageDatas[0], ImageDataSizes[0] / 2);
            Array.Copy(rightTexture.GetRawTextureData(), NextImageDatas[0], ImageDataSizes[0] / 2);
            return true;
        }

        // Methods

        /// <summary>
        ///     Configures <see cref="ArucoCamera.Textures" /> and calls <see cref="ArucoCamera.OnStarted" />.
        /// </summary>
        protected virtual void WebcamController_Started(WebcamController webcamController)
        {
            var webcamTexture = WebcamController.Textures2D[cameraId];
            Textures[cameraId] = new Texture2D(webcamTexture.width, webcamTexture.height, webcamTexture.format, false);
            base.OnStarted();
        }
    }
}
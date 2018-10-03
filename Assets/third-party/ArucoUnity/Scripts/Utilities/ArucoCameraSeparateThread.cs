using System;
using System.Threading;
using ArucoUnity.Cameras;
using ArucoUnity.Plugin;

namespace ArucoUnity.Utilities
{
    public class ArucoCameraSeparateThread
    {
        // Constants

        private const int buffersCount = 3;

        // Variables

        protected IArucoCamera arucoCamera;

        protected uint currentBuffer;
        protected Cv.Mat[][] imageBuffers = new Cv.Mat[buffersCount][];
        protected byte[][][] imageDataBuffers = new byte[buffersCount][][];
        protected Mutex mutex = new Mutex();

        protected Thread thread;
        protected Exception threadException, exception;
        protected bool threadUpdated, imagesUpdated;
        protected Action<Cv.Mat[]> threadWork;

        // Constructor

        public ArucoCameraSeparateThread(IArucoCamera arucoCamera, Action<Cv.Mat[]> threadWork)
        {
            this.arucoCamera = arucoCamera;
            this.threadWork = threadWork;
            CopyBackImages = false;

            for (var bufferId = 0; bufferId < buffersCount; bufferId++)
            {
                imageBuffers[bufferId] = new Cv.Mat[arucoCamera.CameraNumber];
                imageDataBuffers[bufferId] = new byte[arucoCamera.CameraNumber][];

                for (var cameraId = 0; cameraId < arucoCamera.CameraNumber; cameraId++)
                {
                    imageBuffers[bufferId][cameraId] = new Cv.Mat(arucoCamera.Textures[cameraId].height,
                        arucoCamera.Textures[cameraId].width,
                        CvMatExtensions.ImageType(arucoCamera.Textures[cameraId].format));

                    imageDataBuffers[bufferId][cameraId] = new byte[arucoCamera.ImageDataSizes[cameraId]];
                    imageBuffers[bufferId][cameraId].DataByte = imageDataBuffers[bufferId][cameraId];
                }
            }
        }

        // Properties

        public bool CopyBackImages { get; set; }
        public bool IsStarted { get; protected set; }
        public bool ImagesUpdated { get; protected set; }

        // Methods

        public void Start()
        {
            IsStarted = true;
            ImagesUpdated = false;

            thread = new Thread(() =>
            {
                try
                {
                    while (IsStarted)
                    {
                        mutex.WaitOne();
                        {
                            imagesUpdated = ImagesUpdated;
                        }
                        mutex.ReleaseMutex();

                        if (imagesUpdated)
                        {
                            threadWork(imageBuffers[currentBuffer]);

                            mutex.WaitOne();
                            {
                                currentBuffer = NextBuffer();
                                ImagesUpdated = false;
                            }
                            mutex.ReleaseMutex();
                        }
                    }
                }
                catch (Exception e)
                {
                    threadException = e;
                    mutex.ReleaseMutex();
                }
            });
            thread.Start();
        }

        public void Stop()
        {
            IsStarted = false;
        }

      /// <summary>
      ///     Swaps the images with the copy used by the thread, and re-throw the thread exceptions.
      /// </summary>
      public void Update(byte[][] cameraImageDatas)
        {
            if (IsStarted)
            {
                mutex.WaitOne();
                {
                    exception = threadException;
                    threadUpdated = !ImagesUpdated;
                }
                mutex.ReleaseMutex();

                if (exception != null)
                {
                    Stop();
                    throw exception;
                }

                if (threadUpdated)
                    for (var cameraId = 0; cameraId < arucoCamera.CameraNumber; cameraId++)
                        Array.Copy(cameraImageDatas[cameraId], imageDataBuffers[NextBuffer()][cameraId],
                            arucoCamera.ImageDataSizes[cameraId]);

                if (CopyBackImages)
                    for (var cameraId = 0; cameraId < arucoCamera.CameraNumber; cameraId++)
                        Array.Copy(imageDataBuffers[PreviousBuffer()][cameraId], cameraImageDatas[cameraId],
                            arucoCamera.ImageDataSizes[cameraId]);

                if (threadUpdated)
                {
                    mutex.WaitOne();
                    {
                        ImagesUpdated = true;
                    }
                    mutex.ReleaseMutex();
                }
            }
        }

      /// <summary>
      ///     Returns the index of the next buffer.
      /// </summary>
      protected uint NextBuffer()
        {
            return (currentBuffer + 1) % buffersCount;
        }

      /// <summary>
      ///     Returns the index of the previous buffer.
      /// </summary>
      protected uint PreviousBuffer()
        {
            return (currentBuffer + buffersCount - 1) % buffersCount;
        }
    }
}
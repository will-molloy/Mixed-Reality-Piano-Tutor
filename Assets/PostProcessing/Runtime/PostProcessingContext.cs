namespace UnityEngine.PostProcessing
{
    public class PostProcessingContext
    {
        public Camera camera;

        public MaterialFactory materialFactory;
        public PostProcessingProfile profile;
        public RenderTextureFactory renderTextureFactory;

        public bool interrupted { get; private set; }

        public void Interrupt()
        {
            interrupted = true;
        }

        public PostProcessingContext Reset()
        {
            profile = null;
            camera = null;
            materialFactory = null;
            renderTextureFactory = null;
            interrupted = false;
            return this;
        }

        #region Helpers

        public bool isGBufferAvailable => camera.actualRenderingPath == RenderingPath.DeferredShading;

        public bool isHdr
        {
            // No UNITY_5_6_OR_NEWER defined in early betas of 5.6
#if UNITY_5_6 || UNITY_5_6_OR_NEWER
            get { return camera.allowHDR; }
#else
            get { return camera.hdr; }
#endif
        }

        public int width => camera.pixelWidth;

        public int height => camera.pixelHeight;

        public Rect viewport => camera.rect;

        #endregion
    }
}
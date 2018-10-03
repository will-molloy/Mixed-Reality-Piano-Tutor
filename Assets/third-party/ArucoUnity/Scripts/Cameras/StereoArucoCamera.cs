namespace ArucoUnity.Cameras
{
  /// <summary>
  ///     Captures images of stereo camera.
  /// </summary>
  public abstract class StereoArucoCamera : ArucoCamera
    {
        // Constants

        public const int StereoCameraNumber = 2;

        // IArucoCamera properties

        public override int CameraNumber => StereoCameraNumber;

        // Properties

      /// <summary>
      ///     Gets the id of the first camera.
      /// </summary>
      public static int CameraId1 => 0;

      /// <summary>
      ///     Gets the id of the second camera.
      /// </summary>
      public static int CameraId2 => 1;
    }
}
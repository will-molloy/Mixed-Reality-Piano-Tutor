// This file was @generated with LibOVRPlatform/codegen/main. Do not modify it!

namespace Oculus.Platform.Models
{
    using System;

    public class AssetFileDownloadResult
  {
    public readonly string Filepath;


    public AssetFileDownloadResult(IntPtr o)
    {
      Filepath = CAPI.ovr_AssetFileDownloadResult_GetFilepath(o);
    }
  }

}

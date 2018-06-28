// This file was @generated with LibOVRPlatform/codegen/main. Do not modify it!

namespace Oculus.Platform.Models
{
    using System;

    public class AssetFileDeleteResult
  {
    public readonly UInt64 AssetFileId;
    public readonly bool Success;


    public AssetFileDeleteResult(IntPtr o)
    {
      AssetFileId = CAPI.ovr_AssetFileDeleteResult_GetAssetFileId(o);
      Success = CAPI.ovr_AssetFileDeleteResult_GetSuccess(o);
    }
  }

}

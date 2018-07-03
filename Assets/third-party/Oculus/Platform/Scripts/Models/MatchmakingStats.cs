// This file was @generated with LibOVRPlatform/codegen/main. Do not modify it!

namespace Oculus.Platform.Models
{
    using System;

    public class MatchmakingStats
  {
    public readonly uint DrawCount;
    public readonly uint LossCount;
    public readonly uint SkillLevel;
    public readonly uint WinCount;


    public MatchmakingStats(IntPtr o)
    {
      DrawCount = CAPI.ovr_MatchmakingStats_GetDrawCount(o);
      LossCount = CAPI.ovr_MatchmakingStats_GetLossCount(o);
      SkillLevel = CAPI.ovr_MatchmakingStats_GetSkillLevel(o);
      WinCount = CAPI.ovr_MatchmakingStats_GetWinCount(o);
    }
  }

}

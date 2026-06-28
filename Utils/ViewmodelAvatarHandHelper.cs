#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
#endif

namespace MoreWeapons.Utils;

internal static class ViewmodelAvatarHandHelper
{
    internal static void CaptureBaseline(float aimAmount = 0f)
    {
        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return;

        ViewmodelAvatarBoneHelper.EvaluatePose(aimAmount);
    }

    internal static void ApplyOffsets(ViewmodelProfile profile, float aimAmount = 0f)
    {
        ViewmodelAvatarBoneHelper.EvaluatePose(aimAmount);
        ViewmodelAvatarBoneHelper.ApplyBoneOverrides(profile);
    }

    internal static void Reset()
    {
        ViewmodelAvatarBoneHelper.EndEditorSession();
    }
}

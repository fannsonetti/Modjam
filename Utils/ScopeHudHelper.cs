#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.UI;
#else
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
#endif

using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ScopeHudHelper
{
    private static GameObject _hotbarContainer;
    private static bool _hotbarHidden;

    internal static void SetScoped(bool scoped)
    {
        if (!EnsureHotbar())
            return;

        if (scoped)
        {
            if (_hotbarHidden)
                return;

            _hotbarContainer.SetActive(false);
            _hotbarHidden = true;
            return;
        }

        if (!_hotbarHidden)
            return;

        _hotbarContainer.SetActive(true);
        _hotbarHidden = false;
    }

    internal static void Reset()
    {
        if (_hotbarContainer != null && _hotbarHidden)
            _hotbarContainer.SetActive(true);

        _hotbarHidden = false;
    }

    private static bool EnsureHotbar()
    {
        if (_hotbarContainer != null)
            return true;

        var hud = Singleton<HUD>.Instance;
        if (hud == null)
            return false;

        var field = typeof(HUD).GetField("HotbarContainer", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(hud) is RectTransform rect)
            _hotbarContainer = rect.gameObject;

        return _hotbarContainer != null;
    }
}

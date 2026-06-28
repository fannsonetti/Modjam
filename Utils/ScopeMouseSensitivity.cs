#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
#endif

using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ScopeMouseSensitivity
{
    private static bool _active;
    private static float _savedMouseSensitivity;
    private static float _savedLookSensitivity;

    internal static void SetScoped(bool scoped, float aimAmount)
    {
        var settings = Singleton<Settings>.Instance;
        var camera = PlayerSingleton<PlayerCamera>.Instance;
        if (settings == null || camera == null)
            return;

        var blend = Mathf.Clamp01(aimAmount);
        if (blend <= 0.01f)
        {
            Restore(settings, camera);
            return;
        }

        if (!_active)
        {
            _savedMouseSensitivity = settings.InputSettings != null ? settings.InputSettings.MouseSensitivity : 1f;
            _savedLookSensitivity = settings.LookSensitivity;
            _active = true;
        }

        var multiplier = Mathf.Lerp(1f, Core.SniperScopedMouseSensitivity, blend);
        if (settings.InputSettings != null)
            settings.InputSettings.MouseSensitivity = _savedMouseSensitivity * multiplier;

        settings.LookSensitivity = _savedLookSensitivity * multiplier;
    }

    internal static void Reset()
    {
        var settings = Singleton<Settings>.Instance;
        var camera = PlayerSingleton<PlayerCamera>.Instance;
        if (settings == null || camera == null)
            return;

        Restore(settings, camera);
    }

    private static void Restore(Settings settings, PlayerCamera camera)
    {
        if (!_active)
            return;

        if (settings.InputSettings != null)
            settings.InputSettings.MouseSensitivity = _savedMouseSensitivity;

        settings.LookSensitivity = _savedLookSensitivity;
        _active = false;
    }
}

#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
#endif

using System.Reflection;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class UiInputBlocker
{
    private static int _blockDepth;

    internal static void Push(PlayerCamera camera)
    {
        if (camera == null)
            return;

        var field = camera.GetType().GetField(
            "activeUIElementCount",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(camera) is int count)
            field.SetValue(camera, count + 1);

        _blockDepth++;
    }

    internal static void Pop(PlayerCamera camera)
    {
        if (camera == null || _blockDepth <= 0)
            return;

        var field = camera.GetType().GetField(
            "activeUIElementCount",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(camera) is int count)
            field.SetValue(camera, Mathf.Max(0, count - 1));

        _blockDepth--;
    }

    internal static void Reset(PlayerCamera camera)
    {
        while (_blockDepth > 0)
            Pop(camera);
    }
}

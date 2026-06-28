#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
#endif

using System;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ViewmodelMountHelper
{
    internal static Transform ResolveParent()
    {
        var playerCamera = PlayerSingleton<PlayerCamera>.Instance;
        if (playerCamera == null)
            return null;

        var root = playerCamera.transform;
        for (var i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            var name = child.name;
            if (name.IndexOf("viewmodel", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("equippable", StringComparison.OrdinalIgnoreCase) >= 0)
                return child;
        }

        return root;
    }
}

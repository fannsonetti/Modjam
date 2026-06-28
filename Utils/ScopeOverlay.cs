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
using UnityEngine.UI;

namespace MoreWeapons.Utils;

internal static class ScopeOverlay
{
    private const string ScopeResourceName = "MoreWeapons.Assets.UI.sniper_scope_overlay.png";
    private const string ScopeFileName = "sniper_scope_overlay.png";

    private static GameObject _root;
    private static Image _scopeImage;

    internal static void SetVisible(bool visible)
    {
        if (!visible)
        {
            if (_root != null)
                _root.SetActive(false);
            return;
        }

        EnsureCreated();
        if (_root == null)
            return;

        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    internal static void Hide() => SetVisible(false);

    private static void EnsureCreated()
    {
        if (_root != null)
            return;

        var hud = Singleton<HUD>.Instance;
        if (hud == null || hud.canvas == null)
            return;

        var sprite = ModAssetLoader.LoadSprite(ScopeResourceName, "Assets/UI/" + ScopeFileName);
        if (sprite == null)
        {
            MelonLoader.MelonLogger.Warning("MoreWeapons: failed to load sniper scope overlay texture.");
            return;
        }

        _root = new GameObject("MoreWeaponsScopeOverlay");
        _root.transform.SetParent(hud.canvas.transform, false);

        var rootRect = _root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        _scopeImage = _root.AddComponent<Image>();
        _scopeImage.sprite = sprite;
        _scopeImage.color = Color.white;
        _scopeImage.preserveAspect = false;
        _scopeImage.raycastTarget = false;

        _root.SetActive(false);
    }
}

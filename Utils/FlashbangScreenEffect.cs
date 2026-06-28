#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.UI;
#else
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
#endif

using System.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MoreWeapons.Utils;

internal static class FlashbangScreenEffect
{
    private static GameObject _root;
    private static Image _flashImage;
    private static object _routine;

    internal static void Play(float strength)
    {
        strength = Mathf.Clamp01(strength);
        if (strength <= 0.008f)
            return;

        EnsureCreated();
        if (_root == null || _flashImage == null)
            return;

        if (_routine != null)
            MelonCoroutines.Stop(_routine);

        _routine = MelonCoroutines.Start(FlashRoutine(strength));
    }

    private static IEnumerator FlashRoutine(float strength)
    {
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();

        const float peakAlpha = 1f;
        var hold = Mathf.Lerp(1.8f, 4.5f, strength);
        var fadeOut = Mathf.Lerp(0.12f, 0.35f, strength);

        _flashImage.color = new Color(1f, 1f, 1f, peakAlpha);

        var elapsed = 0f;
        while (elapsed < hold)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / fadeOut;
            _flashImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(peakAlpha, 0f, t * t));
            yield return null;
        }

        _flashImage.color = new Color(1f, 1f, 1f, 0f);
        _root.SetActive(false);
        _routine = null;
    }

    private static void EnsureCreated()
    {
        if (_root != null)
            return;

        var hud = Singleton<HUD>.Instance;
        if (hud == null || hud.canvas == null)
            return;

        _root = new GameObject("MoreWeaponsFlashbangOverlay");
        _root.transform.SetParent(hud.canvas.transform, false);

        var rootRect = _root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        _flashImage = _root.AddComponent<Image>();
        _flashImage.color = new Color(1f, 1f, 1f, 0f);
        _flashImage.raycastTarget = false;
        _root.SetActive(false);
    }
}

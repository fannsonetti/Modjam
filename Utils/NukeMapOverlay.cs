#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI;
#else
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
#endif

using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MoreWeapons.Utils;

internal static class NukeMapOverlay
{
    private const string MapResourceName = "MoreWeapons.Assets.UI.Map_Full.png";
    private const string MapFileName = "Map_Full.png";
    private const string InputBlockerId = "NukeMapOverlay";
    private const float MapSize = 580f;
    private const float VerticalOffset = 12f;
    private const float DropButtonScale = 0.9f;

    private static GameObject _root;
    private static RectTransform _mapRect;
    private static RectTransform _dropRect;
    private static Image _mapImage;
    private static Image _dropImage;
    private static Action<Vector3, bool> _onConfirm;
    private static Func<bool> _hasCharges;
    private static bool _isOpen;
    private static bool _hasTarget;
    private static Vector3 _selectedPoint;
    private static bool _inputCaptured;
    private static Texture2D _targetTexture;

    internal static bool IsOpen => _isOpen;

    internal static void Initialize()
    {
        MelonEvents.OnUpdate.Subscribe(OnUpdate);
        MelonEvents.OnGUI.Subscribe(DrawGui);
    }

    internal static void Open(Action<Vector3, bool> onConfirm, Func<bool> hasCharges = null)
    {
        if (_isOpen)
            return;

        EnsureCreated();
        if (_root == null || _mapImage == null || _dropRect == null)
            return;

        _onConfirm = onConfirm;
        _hasCharges = hasCharges;
        _hasTarget = false;
        _isOpen = true;
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        RefreshDropButtonVisual();
        BeginInputCapture();
    }

    internal static void Close()
    {
        _isOpen = false;
        _hasTarget = false;
        _onConfirm = null;
        _hasCharges = null;

        if (_root != null)
            _root.SetActive(false);

        EndInputCapture();
    }

    private static void OnUpdate()
    {
        if (!_isOpen)
            return;

        MaintainInputCapture();

        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
        {
            Close();
            return;
        }
    }

    private static void DrawGui()
    {
        if (!_isOpen || _mapRect == null)
            return;

        if (Event.current.type == EventType.KeyDown
            && (Event.current.keyCode == KeyCode.Tab || Event.current.keyCode == KeyCode.Escape))
        {
            Close();
            Event.current.Use();
            return;
        }

        var mapScreenRect = GetScreenRect(_mapRect);
        DrawTargetMarker(mapScreenRect);

        var current = Event.current;
        if (current == null || current.type != EventType.MouseDown || current.button != 0)
            return;

        var dropScreenRect = GetScreenRect(_dropRect);
        if (dropScreenRect.Contains(current.mousePosition))
        {
            ConfirmStrike();
            current.Use();
            return;
        }

        if (!mapScreenRect.Contains(current.mousePosition))
            return;

        SetTarget(NukeMapCoordinates.ScreenPointToWorld(current.mousePosition, mapScreenRect));
        RefreshDropButtonVisual();
        current.Use();
    }

    private static void SetTarget(Vector3 worldPoint)
    {
        _selectedPoint = worldPoint;
        _hasTarget = true;
    }

    private static void DrawTargetMarker(Rect mapScreenRect)
    {
        if (!_hasTarget)
            return;

        EnsureTargetTexture();
        var point = NukeMapCoordinates.WorldPointToGuiPoint(_selectedPoint, mapScreenRect);
        const float size = 28f;
        var rect = new Rect(point.x - size * 0.5f, point.y - size * 0.5f, size, size);
        GUI.color = new Color(1f, 0.18f, 0.12f, 0.95f);
        GUI.DrawTexture(rect, _targetTexture, ScaleMode.StretchToFill, true);
        GUI.color = Color.white;
    }

    private static void ConfirmStrike()
    {
        if (!_hasTarget)
            return;

        if (_hasCharges != null && !_hasCharges())
        {
            RefreshDropButtonVisual();
            return;
        }

        if (!NukeStrikeBilling.TryCharge(out var failureReason))
        {
            MelonLogger.Warning($"MoreWeapons: nuke strike billing failed — {failureReason}");
            RefreshDropButtonVisual();
            return;
        }

        var callback = _onConfirm;
        var point = _selectedPoint;
        var dealDamage = Core.NukeDealsDamage;
        Close();
        callback?.Invoke(point, dealDamage);
    }

    private static void RefreshDropButtonVisual()
    {
        if (_dropImage == null)
            return;

        var canDrop = _hasTarget && NukeStrikeBilling.CanAfford() && (_hasCharges?.Invoke() ?? true);
        _dropImage.color = canDrop
            ? new Color(1f, 1f, 1f, 0.98f)
            : new Color(0.72f, 0.72f, 0.72f, 0.55f);
    }

    private static void BeginInputCapture()
    {
        if (_inputCaptured)
            return;

        var camera = PlayerSingleton<PlayerCamera>.Instance;
        if (camera != null)
            PlayerControlHelper.FreeMouse(camera);

        var movement = PlayerSingleton<PlayerMovement>.Instance;
        movement?.AddSprintBlocker(InputBlockerId);
        PlayerControlHelper.SetCanMove(movement, false);
        PlayerControlHelper.SetCanLook(camera, false);
        Singleton<HUD>.Instance?.SetCrosshairVisible(false);
        UiInputBlocker.Push(camera);
        _inputCaptured = true;
    }

    private static void MaintainInputCapture()
    {
        var camera = PlayerSingleton<PlayerCamera>.Instance;
        var movement = PlayerSingleton<PlayerMovement>.Instance;
        PlayerControlHelper.SetCanMove(movement, false);
        PlayerControlHelper.SetCanLook(camera, false);
    }

    private static void EndInputCapture()
    {
        if (!_inputCaptured)
            return;

        var camera = PlayerSingleton<PlayerCamera>.Instance;
        if (camera != null)
            PlayerControlHelper.LockMouse(camera);

        var movement = PlayerSingleton<PlayerMovement>.Instance;
        movement?.RemoveSprintBlocker(InputBlockerId);
        PlayerControlHelper.SetCanMove(movement, true);
        PlayerControlHelper.SetCanLook(camera, true);
        UiInputBlocker.Pop(camera);
        _inputCaptured = false;
    }

    private static void EnsureTargetTexture()
    {
        if (_targetTexture != null)
            return;

        const int size = 64;
        _targetTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "MoreWeapons_NukeTargetMarker",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var center = size * 0.5f;
        var pixels = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var ringOuter = dist <= 29f && dist >= 24f;
                var ringMid = dist <= 20f && dist >= 15.5f;
                var ringInner = dist <= 11f && dist >= 7f;
                var dot = dist <= 3f;
                var alpha = ringOuter || ringMid || ringInner || dot ? 0.96f : 0f;
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        _targetTexture.SetPixels(pixels);
        _targetTexture.Apply(false, true);
    }

    private static void EnsureCreated()
    {
        if (_root != null)
            return;

        var hud = Singleton<HUD>.Instance;
        if (hud == null || hud.canvas == null)
            return;

        var sprite = ModAssetLoader.LoadSprite(MapResourceName, "Assets/UI/" + MapFileName);
        if (sprite == null)
        {
            MelonLogger.Warning("MoreWeapons: failed to load nuke map texture.");
            return;
        }

        _root = new GameObject("MoreWeaponsNukeMapOverlay");
        var rootRect = _root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        _root.transform.SetParent(hud.canvas.transform, false);

        var half = MapSize * 0.5f;

        _mapRect = CreatePanelRect("MapImage", _root.transform, new Vector2(-half, VerticalOffset), MapSize);
        _mapImage = _mapRect.gameObject.AddComponent<Image>();
        _mapImage.sprite = sprite;
        _mapImage.preserveAspect = true;
        _mapImage.color = Color.white;

        _dropRect = CreatePanelRect("DropButton", _root.transform, new Vector2(half, VerticalOffset), MapSize);
        _dropRect.localScale = Vector3.one * DropButtonScale;
        _dropImage = _dropRect.gameObject.AddComponent<Image>();
        _dropImage.sprite = CreateCircleSprite();
        _dropImage.color = new Color(1f, 1f, 1f, 0.98f);

        _root.SetActive(false);
    }

    private static RectTransform CreatePanelRect(string name, Transform parent, Vector2 anchoredPosition, float size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = anchoredPosition;
        return rect;
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 256;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "MoreWeapons_NukeDropCircle",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var center = size * 0.5f;
        var radius = size * 0.5f - 2f;
        var pixels = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = dist <= radius ? 1f : 0f;
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Rect GetScreenRect(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    }
}

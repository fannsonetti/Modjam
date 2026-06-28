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

using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using MoreWeapons.Weapons;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ViewmodelEditor
{
    private const string InputBlockerId = "ViewmodelEditor";
    private const int PanelWidth = 620;
    private const int PanelHeight = 620;

    private static readonly string[] DefaultAnimPresets =
    {
        "Equip",
        "Fire",
        "Fire2",
        "ReloadStart",
        "ReloadIndividual",
        "ReloadEnd",
        "Cock",
        "Custom",
    };

    private static string[] _animPresets = DefaultAnimPresets;

    private static bool _isOpen;
    private static bool _inputCaptured;
    private static float _savedTimeScale = 1f;
    private static KeyCode _toggleKey = KeyCode.F5;

    private static Vector2 _panelPosition = new(-1f, 30f);
    private static bool _draggingPanel;
    private static Vector2 _dragOffset;

    private static PlaceholderAvatarWeaponEquippable _boundPlaceholder;
    private static CukeViewmodelEquippable _boundCuke;
    private static ViewmodelProfile _editProfile;
    private static ViewmodelProfile _savedProfile;
    private static string _bindStatus = "No weapon bound";

    private static int _activeTab;
    private static Vector2 _scroll;
    private static float _previewAim;
    private static float _mapRaisePreviewBlend;
    private static float _previewFov = 75f;
    private static bool _fovOverridden;

    private static ViewmodelAnimClip _animClip = new();
    private static readonly Dictionary<string, ViewmodelAnimClip> _animClipLibrary = new();
    private static AnimationClip _activeSourceClip;
    private static AnimationClip _activeEditClip;
    private static int _animPresetIndex;
    private static float _animScrub;
    private static int _selectedKeyframe = -1;
    private static bool _animPlaying;
    private static float _animPlayTime;

    private static readonly Dictionary<int, ViewmodelProfile> _sessionProfiles = new();
    private static readonly Dictionary<int, float> _sessionAim = new();
    private static readonly Dictionary<int, float> _sessionMapRaise = new();
    private static int _lastSessionWeaponId;

    internal static bool IsOpen => _isOpen;
    internal static float PreviewAim => _previewAim;
    internal static float MapRaisePreviewBlend => _mapRaisePreviewBlend;

    internal static void Initialize(MelonPreferences_Category prefs = null)
    {
        var category = prefs ?? MelonPreferences.CreateCategory("MoreWeapons.ViewmodelEditor", "MoreWeapons — Viewmodel Editor");
        _toggleKey = category.CreateEntry("ViewmodelEditorToggleKey", KeyCode.F5, "Toggle key", "Key to open the viewmodel editor while holding a mod weapon").Value;
        MelonEvents.OnUpdate.Subscribe(OnUpdate);
        ViewmodelEditorHost.Ensure();
    }

    internal static ViewmodelProfile GetEffectiveProfile(PlaceholderAvatarWeaponEquippable weapon)
    {
        if (_isOpen && _boundPlaceholder == weapon)
            return _editProfile;

        if (TryGetSessionProfile(weapon, out var cached))
            return cached;

        return weapon.GetViewmodelProfile();
    }

    internal static ViewmodelProfile GetEffectiveProfile(CukeViewmodelEquippable weapon)
    {
        if (_isOpen && _boundCuke == weapon)
            return _editProfile;

        if (TryGetSessionProfile(weapon, out var cached))
            return cached;

        return weapon.GetProfile();
    }

    internal static float GetEffectivePreviewAim(PlaceholderAvatarWeaponEquippable weapon)
    {
        if (_isOpen && _boundPlaceholder == weapon)
            return _previewAim;

        return TryGetSessionAim(weapon, out var aim) ? aim : weapon.EditorLiveAimAmount;
    }

    internal static bool IsBoundTo(PlaceholderAvatarWeaponEquippable weapon) =>
        _isOpen && _boundPlaceholder == weapon;

    internal static bool IsBoundTo(CukeViewmodelEquippable weapon) =>
        _isOpen && _boundCuke == weapon;

    internal static float GetMapRaisePreview(CukeViewmodelEquippable weapon) =>
        IsBoundTo(weapon) ? _mapRaisePreviewBlend : 0f;

    internal static float GetMapRaisePreview(PlaceholderAvatarWeaponEquippable weapon) =>
        IsBoundTo(weapon) ? _mapRaisePreviewBlend : 0f;

    private static void OnUpdate()
    {
        if (NukeMapOverlay.IsOpen)
            return;

        if (UnityEngine.Input.GetKeyDown(_toggleKey))
            Toggle();

        if (!_isOpen)
            return;

        MaintainInputCapture();
        UpdatePreviewFov();
        MaintainLivePreview();

        if (_animPlaying)
            UpdateAnimPlayback();
    }

    private static void MaintainLivePreview()
    {
        if (!_isOpen)
            return;

        ApplyEditProfile();
        _boundPlaceholder?.EditorEnsureVisible();
        RefreshAnimationPose();
    }

    internal static void LateUpdatePreview()
    {
        if (!_isOpen || _editProfile.IsFloating)
            return;

        ViewmodelAvatarBoneHelper.ApplyBoneOverrides(_editProfile);
    }

    private static void RefreshAnimationPose()
    {
        if (_editProfile.IsFloating)
            return;

        if (_activeTab == 1)
            SampleActiveClipPose(_animScrub);
        else
            RestoreStaticPreviewPose();
    }

    private static void RestoreStaticPreviewPose()
    {
        ViewmodelAvatarBoneHelper.EvaluatePose(_previewAim);
        ViewmodelAvatarBoneHelper.ApplyBoneOverrides(_editProfile);
    }

    private static void SampleActiveClipPose(float normalizedTime)
    {
        normalizedTime = Mathf.Clamp01(normalizedTime);

        if (_activeEditClip != null)
            ViewmodelAvatarBoneHelper.SampleClip(_activeEditClip, normalizedTime * _animClip.Duration);
        else
            ViewmodelAvatarBoneHelper.EvaluatePose(_previewAim);

        if (_activeTab == 1 && _animClip.Keyframes.Count > 0 && _animClip.TrySample(normalizedTime, out var sample))
            ApplyAnimSample(sample, refreshClipPose: false);
    }

    private static void Toggle()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }

    private static void Open()
    {
        if (NukeMapOverlay.IsOpen)
            return;

        if (!TryBindWeapon())
        {
            MelonLogger.Msg($"Viewmodel editor: {_bindStatus}");
            return;
        }

        _savedProfile = _editProfile.Clone();
        _savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _isOpen = true;
        _animPlaying = false;
        _animPlayTime = 0f;

        var weaponId = GetBoundWeaponId();
        if (weaponId != 0 && weaponId != _lastSessionWeaponId)
        {
            _animClipLibrary.Clear();
            _lastSessionWeaponId = weaponId;
        }

        BeginInputCapture();
        BeginAnimatorSession();
        ApplyEditProfile();
        RestoreStaticPreviewPose();
        LoadAnimPreset(_animPresetIndex, samplePose: false);
        MelonLogger.Msg($"Viewmodel editor opened — {_bindStatus}");
    }

    private static void BeginAnimatorSession()
    {
        if (_editProfile.IsFloating)
            return;

        ViewmodelAvatarBoneHelper.BeginEditorSession();
    }

    private static void Close()
    {
        SaveAnimPreset(_animPresetIndex);
        SaveSessionState();

        var closingProfile = _editProfile.Clone();
        var closingPlaceholder = _boundPlaceholder;
        var closingCuke = _boundCuke;

        _isOpen = false;
        _animPlaying = false;
        _activeSourceClip = null;
        _activeEditClip = null;
        Time.timeScale = _savedTimeScale;
        EndInputCapture();
        StopPreviewFov();
        ViewmodelAvatarBoneHelper.EndEditorSession();
        ClearBinding();
        ApplyClosedSessionProfile(closingPlaceholder, closingCuke, closingProfile);
    }

    private static void ApplyClosedSessionProfile(
        PlaceholderAvatarWeaponEquippable placeholder,
        CukeViewmodelEquippable cuke,
        ViewmodelProfile profile)
    {
        if (placeholder != null)
            placeholder.EditorApplyProfile(profile, applyAvatarOffsets: true);
        else if (cuke != null)
            cuke.EditorApplyProfile(profile);
    }

    private static bool TryBindWeapon()
    {
        _boundPlaceholder = null;
        _boundCuke = null;

        var activePlaceholder = PlaceholderAvatarWeaponEquippable.ActiveEquipped;
        if (activePlaceholder != null && activePlaceholder.EditorCanBind)
        {
            BindPlaceholder(activePlaceholder);
            return true;
        }

        var activeCuke = CukeViewmodelEquippable.ActiveEquipped;
        if (activeCuke != null && activeCuke.EditorCanBind)
        {
            BindCuke(activeCuke);
            return true;
        }

        var fromAvatar = TryBindFromViewmodelAvatar();
        if (fromAvatar)
            return true;

        var fromInventory = TryBindFromPlayerInventory();
        if (fromInventory)
            return true;

        var fromScene = TryBindFromScene();
        if (fromScene)
            return true;

        _bindStatus = "Equip a MoreWeapons item, then press F5.";
        return false;
    }

    private static void BindPlaceholder(PlaceholderAvatarWeaponEquippable weapon)
    {
        _boundPlaceholder = weapon;
        _bindStatus = weapon.name;
        _previewFov = GetDefaultCameraFov();
        BuildAnimPresetsFromWeapon(weapon);

        var key = weapon.GetInstanceID();
        if (_sessionProfiles.TryGetValue(key, out var cached))
        {
            _editProfile = cached.Clone();
            _previewAim = _sessionAim.TryGetValue(key, out var aim) ? aim : weapon.EditorLiveAimAmount;
            _mapRaisePreviewBlend = _sessionMapRaise.TryGetValue(key, out var blend) ? blend : 0f;
            return;
        }

        _editProfile = weapon.GetViewmodelProfile().Clone();
        weapon.EditorSyncLiveViewmodel(ref _editProfile);
        _previewAim = weapon.EditorLiveAimAmount;
        _mapRaisePreviewBlend = 0f;
    }

    private static void BuildAnimPresetsFromWeapon(PlaceholderAvatarWeaponEquippable weapon)
    {
        var presets = new List<string> { "Equip" };

        if (weapon.EditorFireAnimTriggers != null && weapon.EditorFireAnimTriggers.Length > 0)
        {
            for (var i = 0; i < weapon.EditorFireAnimTriggers.Length; i++)
                presets.Add(i == 0 ? "Fire" : $"Fire{i + 1}");
        }
        else
        {
            presets.Add("Fire");
        }

        presets.Add("ReloadStart");
        presets.Add("ReloadIndividual");
        presets.Add("ReloadEnd");
        presets.Add("Cock");
        presets.Add("Custom");
        _animPresets = presets.ToArray();
        _animPresetIndex = 0;
    }

    private static void BindCuke(CukeViewmodelEquippable weapon)
    {
        _boundCuke = weapon;
        _bindStatus = weapon.name;
        _previewFov = GetDefaultCameraFov();
        _animPresets = DefaultAnimPresets;
        _animPresetIndex = 0;

        var key = weapon.GetInstanceID();
        if (_sessionProfiles.TryGetValue(key, out var cached))
        {
            _editProfile = cached.Clone();
            _previewAim = _sessionAim.TryGetValue(key, out var aim) ? aim : 0f;
            _mapRaisePreviewBlend = _sessionMapRaise.TryGetValue(key, out var blend) ? blend : 0f;
            return;
        }

        _editProfile = weapon.GetProfile().Clone();
        weapon.EditorSyncLiveViewmodel(ref _editProfile);
        _previewAim = 0f;
        _mapRaisePreviewBlend = 0f;
    }

    private static bool TryBindFromViewmodelAvatar()
    {
        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return false;

        var right = GetHandTransform(avatar, "RightHandContainer");
        if (right != null)
        {
            var placeholder = right.GetComponentInChildren<PlaceholderAvatarWeaponEquippable>(true);
            if (placeholder != null && placeholder.EditorCanBind)
            {
                BindPlaceholder(placeholder);
                return true;
            }

            var cuke = right.GetComponentInChildren<CukeViewmodelEquippable>(true);
            if (cuke != null && cuke.EditorCanBind)
            {
                BindCuke(cuke);
                return true;
            }
        }

        var left = GetHandTransform(avatar, "LeftHandContainer");
        if (left == null)
            return false;

        var leftPlaceholder = left.GetComponentInChildren<PlaceholderAvatarWeaponEquippable>(true);
        if (leftPlaceholder != null && leftPlaceholder.EditorCanBind)
        {
            BindPlaceholder(leftPlaceholder);
            return true;
        }

        var leftCuke = left.GetComponentInChildren<CukeViewmodelEquippable>(true);
        if (leftCuke == null || !leftCuke.EditorCanBind)
            return false;

        BindCuke(leftCuke);
        return true;
    }

    private static bool TryBindFromPlayerInventory()
    {
        var inventory = PlayerSingleton<PlayerInventory>.Instance;
        if (inventory == null)
            return false;

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var type = inventory.GetType();

        foreach (var memberName in new[] { "equippable", "Equippable", "_equippable" })
        {
            object equippable = type.GetField(memberName, flags)?.GetValue(inventory)
                ?? type.GetProperty(memberName, flags)?.GetValue(inventory);
            if (equippable == null)
                continue;

            if (equippable is PlaceholderAvatarWeaponEquippable placeholder && placeholder.EditorCanBind)
            {
                BindPlaceholder(placeholder);
                return true;
            }

            if (equippable is CukeViewmodelEquippable cuke && cuke.EditorCanBind)
            {
                BindCuke(cuke);
                return true;
            }
        }

        return false;
    }

    private static bool TryBindFromScene()
    {
        var placeholders = UnityEngine.Object.FindObjectsOfType<PlaceholderAvatarWeaponEquippable>();
        foreach (var weapon in placeholders)
        {
            if (!weapon.isActiveAndEnabled || !weapon.gameObject.activeInHierarchy)
                continue;

            BindPlaceholder(weapon);
            return true;
        }

        var cukes = UnityEngine.Object.FindObjectsOfType<CukeViewmodelEquippable>();
        foreach (var weapon in cukes)
        {
            if (!weapon.isActiveAndEnabled || !weapon.gameObject.activeInHierarchy)
                continue;

            BindCuke(weapon);
            return true;
        }

        return false;
    }

    private static void ClearBinding()
    {
        _boundPlaceholder = null;
        _boundCuke = null;
        _bindStatus = "No weapon bound";
    }

    private static void ResetProfile()
    {
        ClearSessionStateForBoundWeapon();

        if (_boundPlaceholder != null)
        {
            _editProfile = _boundPlaceholder.GetViewmodelProfile().Clone();
            _previewAim = _boundPlaceholder.EditorLiveAimAmount;
        }
        else if (_boundCuke != null)
        {
            _editProfile = _boundCuke.GetProfile().Clone();
            _previewAim = 0f;
        }

        _mapRaisePreviewBlend = 0f;
        ApplyEditProfile();
    }

    private static void ApplyEditProfile()
    {
        if (_boundPlaceholder != null)
            _boundPlaceholder.EditorApplyProfile(_editProfile, applyAvatarOffsets: true);
        else if (_boundCuke != null)
            _boundCuke.EditorApplyProfile(_editProfile);
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
        Singleton<HUD>.Instance?.SetCrosshairVisible(false);
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

    private static float GetDefaultCameraFov()
    {
        var settings = Singleton<Settings>.Instance;
        return settings != null ? settings.CameraFOV : 75f;
    }

    private static void UpdatePreviewFov()
    {
        var camera = PlayerSingleton<PlayerCamera>.Instance;
        if (camera == null)
            return;

        PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(_previewFov, 0f);
        _fovOverridden = true;
    }

    private static void StopPreviewFov()
    {
        if (!_fovOverridden)
            return;

        PlayerSingleton<PlayerCamera>.Instance?.StopFOVOverride(0.2f);
        _fovOverridden = false;
    }

    private static void UpdateAnimPlayback()
    {
        _animPlayTime += Time.unscaledDeltaTime;
        var normalized = _animClip.Duration <= 0f ? 0f : Mathf.Clamp01(_animPlayTime / _animClip.Duration);
        _animScrub = normalized;

        if (_animClip.Keyframes.Count > 0 && _animClip.TrySample(normalized, out var sample))
            ApplyAnimSample(sample);
        else
            SampleActiveClipPose(normalized);

        if (normalized >= 1f)
            _animPlaying = false;
    }

    private static void ApplyAnimSample(ViewmodelAnimKeyframe sample, bool refreshClipPose = true)
    {
        _previewAim = sample.Aim;
        _editProfile = sample.ToProfile(_editProfile.PresentationMode, _editProfile.Label);
        ApplyEditProfile();
        if (refreshClipPose && _activeEditClip != null)
            ViewmodelAvatarBoneHelper.SampleClip(_activeEditClip, sample.Time * _animClip.Duration);
    }

    internal static void DrawGui()
    {
        if (!_isOpen)
            return;

        ImGuiSkinHelper.EnsureInitialized();
        ImGuiSkinHelper.DrawOverlay();

        if (_panelPosition.x < 0f)
            _panelPosition = new Vector2(Screen.width * 0.5f - PanelWidth * 0.5f, 30f);

        var rect = new Rect(_panelPosition.x, _panelPosition.y, PanelWidth, PanelHeight);
        var titleBar = new Rect(rect.x, rect.y, rect.width, 26f);
        HandlePanelDrag(titleBar, ref rect);
        _panelPosition = rect.position;

        GUI.Box(rect, string.Empty, ImGuiSkinHelper.PanelStyle);

        var headerRect = new Rect(rect.x + 12f, rect.y + 6f, rect.width - 24f, 20f);
        GUI.Label(headerRect, $"Viewmodel Editor — {_bindStatus}  (drag title bar)", ImGuiSkinHelper.LabelStyle);

        var tabRect = new Rect(rect.x + 12f, rect.y + 28f, rect.width - 24f, 24f);
        GUILayout.BeginArea(tabRect);
        GUILayout.BeginHorizontal();
        var nextTab = _activeTab;
        if (GUILayout.Toggle(_activeTab == 0, "Static Pose", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(22f)))
            nextTab = 0;
        if (GUILayout.Toggle(_activeTab == 1, "Animations", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(22f)))
            nextTab = 1;

        if (nextTab != _activeTab)
        {
            _activeTab = nextTab;
            RefreshAnimationPose();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        var scrollOuter = new Rect(rect.x + 10f, rect.y + 58f, rect.width - 20f, rect.height - 118f);
        var contentHeight = _activeTab == 0 ? 1180f : 820f;
        var contentRect = new Rect(0f, 0f, scrollOuter.width - 24f, contentHeight);
        _scroll = GUI.BeginScrollView(scrollOuter, _scroll, contentRect);
        GUILayout.BeginArea(contentRect);

        if (_activeTab == 0)
            DrawStaticTab();
        else
            DrawAnimationsTab();

        GUILayout.EndArea();
        GUI.EndScrollView();

        DrawFooter(new Rect(rect.x + 10f, rect.y + rect.height - 48f, rect.width - 20f, 36f));
    }

    private static void DrawStaticTab()
    {
        GUILayout.Label("Preview", ImGuiSkinHelper.LabelStyle);
        _previewFov = ViewmodelEditorFields.Slider("Camera FOV (preview only)", _previewFov, 40f, 100f);

        if (_editProfile.IsFloating)
            DrawFloatingFields();
        else
            DrawAvatarHandsFields();

        GUILayout.Space(8f);
        GUILayout.Label("Third Person", ImGuiSkinHelper.LabelStyle);
        _editProfile.ThirdPersonPosition = ViewmodelEditorFields.Vector3Field("Position", _editProfile.ThirdPersonPosition, -1f, 1f);
        _editProfile.ThirdPersonEuler = ViewmodelEditorFields.Vector3Field("Euler", _editProfile.ThirdPersonEuler, -180f, 180f);
        _editProfile.ThirdPersonWorldScale = ViewmodelEditorFields.Slider("World scale", _editProfile.ThirdPersonWorldScale, 0.1f, 2f);
    }

    private static void HandlePanelDrag(Rect titleBar, ref Rect panelRect)
    {
        var evt = Event.current;
        if (evt.type == EventType.MouseDown && evt.button == 0 && titleBar.Contains(evt.mousePosition))
        {
            _draggingPanel = true;
            _dragOffset = evt.mousePosition - panelRect.position;
            evt.Use();
        }

        if (_draggingPanel && evt.type == EventType.MouseDrag)
        {
            panelRect.position = evt.mousePosition - _dragOffset;
            evt.Use();
        }

        if (evt.type == EventType.MouseUp)
            _draggingPanel = false;
    }

    private static void DrawAvatarHandsFields()
    {
        _previewAim = ViewmodelEditorFields.Slider("Preview aim (0=hip, 1=ADS)", _previewAim, 0f, 1f);

        GUILayout.Space(6f);
        GUILayout.Label("Hip", ImGuiSkinHelper.LabelStyle);
        _editProfile.HipPosition = ViewmodelEditorFields.Vector3Field("Position", _editProfile.HipPosition, -0.5f, 0.5f);
        _editProfile.HipEuler = ViewmodelEditorFields.Vector3Field("Euler", _editProfile.HipEuler, -180f, 180f);
        _editProfile.UniformScale = ViewmodelEditorFields.Slider("Uniform scale", _editProfile.UniformScale, 0.1f, 3f);

        GUILayout.Space(6f);
        GUILayout.Label("ADS", ImGuiSkinHelper.LabelStyle);
        _editProfile.AimEuler = ViewmodelEditorFields.Vector3Field("Euler offset", _editProfile.AimEuler, -45f, 45f);
        _editProfile.AimPositionOffset = ViewmodelEditorFields.Vector3Field("Position offset", _editProfile.AimPositionOffset, -0.2f, 0.2f);

        GUILayout.Space(6f);
        GUILayout.Label("Arm bones (applied after PumpShotgun/weapon animator)", ImGuiSkinHelper.LabelStyle);
        _editProfile.LeftHandOffset = ViewmodelEditorFields.Vector3Field("Left hand pos", _editProfile.LeftHandOffset, -0.35f, 0.35f);
        _editProfile.LeftHandEuler = ViewmodelEditorFields.Vector3Field("Left hand euler", _editProfile.LeftHandEuler, -90f, 90f);
        _editProfile.RightHandOffset = ViewmodelEditorFields.Vector3Field("Right hand pos", _editProfile.RightHandOffset, -0.35f, 0.35f);
        _editProfile.RightHandEuler = ViewmodelEditorFields.Vector3Field("Right hand euler", _editProfile.RightHandEuler, -90f, 90f);
        _editProfile.LeftForeArmOffset = ViewmodelEditorFields.Vector3Field("Left forearm pos", _editProfile.LeftForeArmOffset, -0.35f, 0.35f);
        _editProfile.LeftForeArmEuler = ViewmodelEditorFields.Vector3Field("Left forearm euler", _editProfile.LeftForeArmEuler, -90f, 90f);
        _editProfile.RightForeArmOffset = ViewmodelEditorFields.Vector3Field("Right forearm pos", _editProfile.RightForeArmOffset, -0.35f, 0.35f);
        _editProfile.RightForeArmEuler = ViewmodelEditorFields.Vector3Field("Right forearm euler", _editProfile.RightForeArmEuler, -90f, 90f);

        GUILayout.Label(ViewmodelAvatarBoneHelper.ResolvedBoneStatus, ImGuiSkinHelper.LabelStyle);
    }

    private static void DrawFloatingFields()
    {
        GUILayout.Space(6f);
        GUILayout.Label("Equippable local", ImGuiSkinHelper.LabelStyle);
        _editProfile.EquippableLocalPosition = ViewmodelEditorFields.Vector3Field("Position", _editProfile.EquippableLocalPosition, -1f, 1f);
        _editProfile.EquippableLocalEuler = ViewmodelEditorFields.Vector3Field("Euler", _editProfile.EquippableLocalEuler, -360f, 360f);

        GUILayout.Space(6f);
        GUILayout.Label("Model local", ImGuiSkinHelper.LabelStyle);
        _editProfile.ModelLocalPosition = ViewmodelEditorFields.Vector3Field("Position", _editProfile.ModelLocalPosition, -1f, 1f);
        _editProfile.ModelLocalEuler = ViewmodelEditorFields.Vector3Field("Euler", _editProfile.ModelLocalEuler, -360f, 360f);
        _editProfile.ModelUniformScale = ViewmodelEditorFields.Slider("Uniform scale", _editProfile.ModelUniformScale, 0.1f, 4f);

        if (SupportsMapRaisePreview())
        {
            GUILayout.Space(6f);
            GUILayout.Label("Map raise pose", ImGuiSkinHelper.LabelStyle);
            _mapRaisePreviewBlend = ViewmodelEditorFields.Slider("Preview blend", _mapRaisePreviewBlend, 0f, 1f);
            _editProfile.MapRaiseEquippablePosition = ViewmodelEditorFields.Vector3Field("Position", _editProfile.MapRaiseEquippablePosition, -1f, 1f);
            _editProfile.MapRaiseEquippableEuler = ViewmodelEditorFields.Vector3Field("Euler", _editProfile.MapRaiseEquippableEuler, -360f, 360f);
        }
    }

    private static bool SupportsMapRaisePreview()
    {
        if (_boundCuke != null)
            return _boundCuke.SupportsMapRaisePoseEditor;
        return _boundPlaceholder != null && _boundPlaceholder.SupportsMapRaisePoseEditor;
    }

    private static void DrawAnimationsTab()
    {
        GUILayout.Label("Animation clip", ImGuiSkinHelper.LabelStyle);

        GUILayout.BeginHorizontal();
        for (var i = 0; i < _animPresets.Length; i++)
        {
            if (GUILayout.Toggle(_animPresetIndex == i, _animPresets[i], ImGuiSkinHelper.ButtonStyle, GUILayout.Height(20f)))
            {
                if (_animPresetIndex != i)
                {
                    SaveAnimPreset(_animPresetIndex);
                    _animPresetIndex = i;
                    LoadAnimPreset(i);
                }
            }
        }
        GUILayout.EndHorizontal();

        _animClip.Duration = ViewmodelEditorFields.Slider("Duration (sec)", _animClip.Duration, 0.05f, 3f);
        if (!string.IsNullOrEmpty(_animClip.SourceClipName))
            GUILayout.Label($"Source clip: {_animClip.SourceClipName}", ImGuiSkinHelper.LabelStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_animPlaying ? "Stop" : "Play preview", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(24f)))
        {
            if (_animPlaying)
                _animPlaying = false;
            else
            {
                _animPlaying = true;
                _animPlayTime = 0f;
            }
        }

        if (GUILayout.Button("Capture keyframe", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(24f)))
            CaptureKeyframe();

        if (GUILayout.Button("Update selected", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(24f)) && _selectedKeyframe >= 0)
            CaptureKeyframe();

        if (GUILayout.Button("Delete selected", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(24f)))
            DeleteSelectedKeyframe();
        GUILayout.EndHorizontal();

        var timelineRect = GUILayoutUtility.GetRect(PanelWidth - 48f, 72f);
        var keyTimes = GetKeyframeTimes();
        var scrubBefore = _animScrub;
        _animScrub = ViewmodelTimelineGui.Draw(timelineRect, _animClip.Duration, _animScrub, keyTimes, ref _selectedKeyframe);

        if (!_animPlaying)
        {
            if (_selectedKeyframe >= 0 && _selectedKeyframe < _animClip.Keyframes.Count
                && !Mathf.Approximately(scrubBefore, _animScrub))
            {
                var moved = _animClip.Keyframes[_selectedKeyframe];
                moved.Time = _animScrub;
                _animClip.Keyframes[_selectedKeyframe] = moved;
            }

            if (_animClip.Keyframes.Count > 0 && _animClip.TrySample(_animScrub, out var scrubSample))
                ApplyAnimSample(scrubSample);
            else
                SampleActiveClipPose(_animScrub);
        }

        GUILayout.Space(8f);
        GUILayout.Label($"Keyframes ({_animClip.Keyframes.Count})", ImGuiSkinHelper.LabelStyle);
        for (var i = 0; i < _animClip.Keyframes.Count; i++)
        {
            var keyframe = _animClip.Keyframes[i];
            var selected = _selectedKeyframe == i;
            if (GUILayout.Toggle(selected, FormatKeyframeSummary(keyframe), ImGuiSkinHelper.ButtonStyle))
            {
                _selectedKeyframe = i;
                _animScrub = keyframe.Time;
            }
        }

        if (GUILayout.Button("Copy animator curve notes", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(24f)))
        {
            var controllerName = _editProfile.Label + "Animator";
            CopyToClipboard(ViewmodelAnimatorBuilder.ToCurveExportSnippet(controllerName, _animClip));
        }
    }

    private static List<float> GetKeyframeTimes()
    {
        var times = new List<float>(_animClip.Keyframes.Count);
        foreach (var keyframe in _animClip.Keyframes)
            times.Add(keyframe.Time);
        return times;
    }

    private static void SaveAnimPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= _animPresets.Length)
            return;

        _animClipLibrary[_animPresets[presetIndex]] = CloneAnimClip(_animClip);
    }

    private static void LoadAnimPreset(int presetIndex, bool samplePose = true)
    {
        _animPresetIndex = presetIndex;
        _animClip.Name = _animPresets[presetIndex];
        _selectedKeyframe = -1;
        _animScrub = 0f;
        _activeSourceClip = null;
        _activeEditClip = null;

        if (_animClipLibrary.TryGetValue(_animClip.Name, out var saved))
        {
            _animClip = CloneAnimClip(saved);
            if (_boundPlaceholder != null && !string.IsNullOrEmpty(_animClip.SourceClipName))
            {
                _activeSourceClip = FindClipByName(_animClip.SourceClipName)
                    ?? ViewmodelAnimatorClipResolver.ResolveSourceClip(_boundPlaceholder, _animClip.Name);
                if (_activeSourceClip != null)
                    _activeEditClip = ViewmodelAnimatorClipResolver.CreateEditableCopy(_activeSourceClip);
            }
        }
        else if (_boundPlaceholder != null)
        {
            _activeSourceClip = ViewmodelAnimatorClipResolver.ResolveSourceClip(_boundPlaceholder, _animClip.Name);
            _activeEditClip = _activeSourceClip != null
                ? ViewmodelAnimatorClipResolver.CreateEditableCopy(_activeSourceClip)
                : null;

            _animClip = new ViewmodelAnimClip
            {
                Name = _animClip.Name,
                SourceClipName = _activeSourceClip != null ? _activeSourceClip.name : string.Empty,
                Duration = _activeSourceClip != null ? _activeSourceClip.length : 0.5f,
            };
        }
        else
        {
            _animClip = new ViewmodelAnimClip { Name = _animClip.Name, Duration = 0.5f };
        }

        if (samplePose && _activeTab == 1)
            SampleActiveClipPose(_animScrub);
    }

    internal static void ClearSession(MonoBehaviour weapon)
    {
        if (weapon == null)
            return;

        var key = weapon.GetInstanceID();
        _sessionProfiles.Remove(key);
        _sessionAim.Remove(key);
        _sessionMapRaise.Remove(key);

        if (_lastSessionWeaponId == key)
            _lastSessionWeaponId = 0;
    }

    private static void SaveSessionState()
    {
        if (_boundPlaceholder != null)
            WriteSessionState(_boundPlaceholder.GetInstanceID());
        if (_boundCuke != null)
            WriteSessionState(_boundCuke.GetInstanceID());
    }

    private static void WriteSessionState(int key)
    {
        _sessionProfiles[key] = _editProfile.Clone();
        _sessionAim[key] = _previewAim;
        _sessionMapRaise[key] = _mapRaisePreviewBlend;
        _lastSessionWeaponId = key;
    }

    private static void ClearSessionStateForBoundWeapon()
    {
        if (_boundPlaceholder != null)
            ClearSession(_boundPlaceholder);
        if (_boundCuke != null)
            ClearSession(_boundCuke);
    }

    private static bool TryGetSessionProfile(MonoBehaviour weapon, out ViewmodelProfile profile)
    {
        profile = default;
        if (weapon == null)
            return false;

        return _sessionProfiles.TryGetValue(weapon.GetInstanceID(), out profile);
    }

    private static bool TryGetSessionAim(MonoBehaviour weapon, out float aim)
    {
        aim = 0f;
        if (weapon == null)
            return false;

        return _sessionAim.TryGetValue(weapon.GetInstanceID(), out aim);
    }

    private static int GetBoundWeaponId()
    {
        if (_boundPlaceholder != null)
            return _boundPlaceholder.GetInstanceID();
        if (_boundCuke != null)
            return _boundCuke.GetInstanceID();
        return 0;
    }

    private static AnimationClip FindClipByName(string clipName)
    {
        var animator = ViewmodelAvatarBoneHelper.Animator;
        if (animator?.runtimeAnimatorController == null || string.IsNullOrEmpty(clipName))
            return null;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return clip;
        }

        return null;
    }

    private static string FormatKeyframeSummary(ViewmodelAnimKeyframe keyframe) =>
        $"t={keyframe.Time:0.###} aim={keyframe.Aim:0.##} " +
        $"hip={keyframe.HipPosition} LHand={keyframe.LeftHandOffset} RHand={keyframe.RightHandOffset} " +
        $"LFore={keyframe.LeftForeArmOffset} RFore={keyframe.RightForeArmOffset}";

    private static ViewmodelAnimClip CloneAnimClip(ViewmodelAnimClip source)
    {
        var clone = new ViewmodelAnimClip
        {
            Name = source.Name,
            SourceClipName = source.SourceClipName,
            Duration = source.Duration,
        };
        clone.Keyframes.AddRange(source.Keyframes);
        return clone;
    }

    private static void CaptureKeyframe()
    {
        var keyframe = ViewmodelAnimKeyframe.FromProfile(_editProfile, _animScrub, _previewAim);
        for (var i = 0; i < _animClip.Keyframes.Count; i++)
        {
            if (!Mathf.Approximately(_animClip.Keyframes[i].Time, _animScrub))
                continue;

            _animClip.Keyframes[i] = keyframe;
            _selectedKeyframe = i;
            return;
        }

        _animClip.Keyframes.Add(keyframe);
        _selectedKeyframe = _animClip.Keyframes.Count - 1;
    }

    private static void DeleteSelectedKeyframe()
    {
        if (_selectedKeyframe < 0 || _selectedKeyframe >= _animClip.Keyframes.Count)
            return;

        _animClip.Keyframes.RemoveAt(_selectedKeyframe);
        _selectedKeyframe = -1;
    }

    private static void DrawFooter(Rect rect)
    {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(28f)))
            ResetProfile();

        if (GUILayout.Button("Copy static C#", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(28f)))
            CopyToClipboard(_editProfile.ToCSharpSnippet(_editProfile.Label));

        if (GUILayout.Button("Copy anim C#", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(28f)))
            CopyToClipboard(_animClip.ToCSharpSnippet(_editProfile.Label + _animClip.Name));

        if (GUILayout.Button("Close (F5)", ImGuiSkinHelper.ButtonStyle, GUILayout.Height(28f)))
            Close();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private static void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
        MelonLogger.Msg("Viewmodel editor: copied C# snippet to clipboard.");
    }

    private static Transform GetHandTransform(object avatar, string memberName)
    {
        var type = avatar.GetType();
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        if (type.GetField(memberName, flags)?.GetValue(avatar) is Transform fieldTransform)
            return fieldTransform;

        if (type.GetProperty(memberName, flags)?.GetValue(avatar) is Transform propertyTransform)
            return propertyTransform;

        return null;
    }
}

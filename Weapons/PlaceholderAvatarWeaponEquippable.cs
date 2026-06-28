#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI;
#else
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
#endif

using System.Collections;
using ScheduleOne.AvatarFramework.Equipping;
using UnityEngine;
using UnityEngine.Rendering;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public abstract class PlaceholderAvatarWeaponEquippable : Equippable_AvatarViewmodel
{
#if IL2CPP
    protected PlaceholderAvatarWeaponEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    protected const float AimFovReduction = 8f;
    protected const float AimDuration = 0.2f;

    protected readonly struct PlaceholderVisuals
    {
        public PlaceholderVisuals(
            Vector3 viewmodelPosition,
            Vector3 viewmodelScale,
            Vector3 thirdPersonPosition,
            float thirdPersonThickness,
            float thirdPersonLength,
            Color barColor,
            string viewmodelName,
            string thirdPersonName)
        {
            ViewmodelPosition = viewmodelPosition;
            ViewmodelScale = viewmodelScale;
            ThirdPersonPosition = thirdPersonPosition;
            ThirdPersonThickness = thirdPersonThickness;
            ThirdPersonLength = thirdPersonLength;
            BarColor = barColor;
            ViewmodelName = viewmodelName;
            ThirdPersonName = thirdPersonName;
        }

        public Vector3 ViewmodelPosition { get; }
        public Vector3 ViewmodelScale { get; }
        public Vector3 ThirdPersonPosition { get; }
        public float ThirdPersonThickness { get; }
        public float ThirdPersonLength { get; }
        public Color BarColor { get; }
        public string ViewmodelName { get; }
        public string ThirdPersonName { get; }
    }

    protected void RefreshViewmodel()
    {
        if (_viewmodel != null)
        {
            UnityEngine.Object.Destroy(_viewmodel);
            _viewmodel = null;
        }

        EnsureViewmodel();
        UpdateViewmodelAim();
    }

    protected void RefreshThirdPersonModel()
    {
        if (_thirdPersonParent == null)
            return;

        DestroyThirdPersonModel();
        EnsureThirdPersonModel(_thirdPersonParent);
    }

    internal static PlaceholderAvatarWeaponEquippable ActiveEquipped { get; private set; }

    private GameObject _viewmodel;
    private GameObject _thirdPersonModel;
    private Transform _thirdPersonParent;
    private Vector3 _viewmodelHipLocalPosition;
    protected IntegerItemInstance WeaponItem;
    protected string[] FireAnimTriggers;

    internal string[] EditorFireAnimTriggers => FireAnimTriggers;

    internal virtual string EditorReloadStartAnimTrigger => string.Empty;

    internal virtual string EditorReloadIndividualAnimTrigger => string.Empty;

    internal virtual string EditorReloadEndAnimTrigger => string.Empty;

    internal virtual string EditorCockAnimTrigger => string.Empty;
    private bool _templateApplied;
    protected float TimeSinceFire = 1000f;
    protected float TimeEquipped;
    private float _aim;
    private float _aimVelocity;
    private bool _aimStarted;
    private bool _fovOverridden;
    private Coroutine _thirdPersonSetupRoutine;
    private float _mapPoseBlend;

    protected abstract string TemplateItemId { get; }
    protected abstract PlaceholderVisuals Visuals { get; }
    protected abstract float InitialTimeSinceFire { get; }
    protected abstract void ApplyWeaponTemplateSettings(Equippable_RangedWeapon template);
    protected abstract void UpdateWeaponReload();
    protected abstract void TryStartWeaponReload();
    protected abstract bool CanWeaponFire();
    protected abstract void FireWeapon();
    protected abstract bool KeepHandsVisibleDuringReload { get; }
    protected virtual bool UsesAvatarHands => true;
    protected virtual bool SupportsMapRaisePose => false;
    protected virtual float HandsFreeModelScale => HandsFreeViewmodelSettings.ModelUniformScale;
    protected virtual bool UsesAimInput => true;

    protected virtual float? GetScopedFov() => null;

    internal virtual ViewmodelProfile GetViewmodelProfile() => ViewmodelProfile.DefaultPlaceholderBar;

    private ViewmodelProfile ActiveProfile => ViewmodelEditor.GetEffectiveProfile(this);

    internal bool EditorCanBind =>
        (isActiveAndEnabled && gameObject.activeInHierarchy) || ViewmodelEditor.IsBoundTo(this);

    internal bool SupportsMapRaisePoseEditor => SupportsMapRaisePose;

    internal void EditorEnsureVisible()
    {
        gameObject.SetActive(true);
        EnsureViewmodel();

        if (_viewmodel != null)
        {
            _viewmodel.SetActive(true);
            foreach (var renderer in _viewmodel.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = true;
        }

        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar != null)
        {
            avatar.SetVisibility(true);
            avatar.Animator?.SetFloat("Aim", GetViewmodelAimAmount());
        }
    }

    internal void EditorApplyProfile(ViewmodelProfile profile, bool applyAvatarOffsets)
    {
        EditorEnsureVisible();

        if (UsesAvatarHands)
        {
            EnsureViewmodel();
            if (_viewmodel != null)
            {
                WeaponGlbLoader.ApplyViewmodelProfile(_viewmodel, profile);
                _viewmodelHipLocalPosition = profile.HipPosition;
            }

            UpdateViewmodelAim();
            return;
        }

        var blend = ViewmodelEditor.GetMapRaisePreview(this);
        profile.GetFloatingEquippablePose(SupportsMapRaisePose ? blend : 0f, out var pos, out var rotation);
        ApplyFloatingEquippablePose(pos, rotation);

        if (_viewmodel != null)
            profile.ApplyFloatingModelTransform(_viewmodel.transform);
    }

    protected virtual Quaternion GetViewmodelAimRotation(float aim) =>
        ActiveProfile.GetAimRotation(aim);

    protected virtual Vector3 GetViewmodelAimPositionOffset(float aim) =>
        ActiveProfile.GetAimPositionOffset(aim);

    protected float GetViewmodelAimAmount() =>
        ViewmodelEditor.IsBoundTo(this) ? ViewmodelEditor.PreviewAim : _aim;

    internal float EditorLiveAimAmount => _aim;

    internal void EditorSyncLiveViewmodel(ref ViewmodelProfile profile)
    {
        if (UsesAvatarHands)
        {
            EnsureViewmodel();
            if (_viewmodel == null)
                return;

            var t = _viewmodel.transform;
            var aim = _aim;
            if (aim <= 0.01f)
            {
                profile.HipPosition = t.localPosition;
                profile.HipEuler = t.localRotation.eulerAngles;
            }
            else
            {
                profile.HipPosition = t.localPosition - profile.GetAimPositionOffset(aim);
            }

            profile.UniformScale = t.localScale.x;
            _viewmodelHipLocalPosition = profile.HipPosition;
            return;
        }

        profile.EquippableLocalPosition = transform.localPosition;
        profile.EquippableLocalEuler = transform.localEulerAngles;
        if (_viewmodel != null)
        {
            profile.ModelLocalPosition = _viewmodel.transform.localPosition;
            profile.ModelLocalEuler = _viewmodel.transform.localRotation.eulerAngles;
            profile.ModelUniformScale = _viewmodel.transform.localScale.x;
        }
    }

    private void ApplyAvatarHandOffsets(ViewmodelProfile profile)
    {
        ViewmodelAvatarBoneHelper.ApplyBoneOverrides(profile);
    }

    private static bool ProfileHasBoneOverrides(ViewmodelProfile profile) =>
        profile.LeftHandOffset != Vector3.zero || profile.LeftHandEuler != Vector3.zero
        || profile.RightHandOffset != Vector3.zero || profile.RightHandEuler != Vector3.zero
        || profile.LeftForeArmOffset != Vector3.zero || profile.LeftForeArmEuler != Vector3.zero
        || profile.RightForeArmOffset != Vector3.zero || profile.RightForeArmEuler != Vector3.zero;

    protected GameObject CreateGlbViewmodel(string embeddedResourceName, string relativePathFromModDir, string objectName)
    {
        var model = WeaponGlbLoader.LoadViewmodel(embeddedResourceName, relativePathFromModDir);
        if (model == null)
            return null;

        model.name = objectName;
        if (UsesAvatarHands)
        {
            WeaponGlbLoader.ApplyViewmodelProfile(model, ActiveProfile);
            return model;
        }

        ApplyFloatingModelTransform(model);
        return model;
    }

    protected void ApplyFloatingModelTransform(GameObject model)
    {
        var profile = ActiveProfile;
        profile.ApplyFloatingModelTransform(model.transform);
        LayerUtility.SetLayerRecursively(model, LayerMask.NameToLayer("Viewmodel"));
        _viewmodelHipLocalPosition = model.transform.localPosition;
    }

    protected void ApplyHandsFreeModelTransform(GameObject model) => ApplyFloatingModelTransform(model);

    protected void ApplyHandsFreeEquippableTransform()
    {
        var profile = ActiveProfile;
        localScale = Vector3.one;
        ApplyFloatingEquippablePose(profile.EquippableLocalPosition, Quaternion.Euler(profile.EquippableLocalEuler));
    }

    private void ApplyFloatingEquippablePose(Vector3 position, Quaternion rotation)
    {
        localPosition = position;
        localEulerAngles = rotation.eulerAngles;
        localScale = Vector3.one;
        transform.localPosition = position;
        transform.localRotation = rotation;
        transform.localScale = Vector3.one;
    }

    private void ApplyViewmodelLayerAndShadows()
    {
        LayerUtility.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Viewmodel"));
        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                renderer.enabled = false;
            else
                renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    private void RegisterEquippedItem(ItemInstance item)
    {
        itemInstance = item;
        PlayerSingleton<PlayerInventory>.Instance.SetEquippable(this);
        PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
    }

    private void EquipWithAvatarHands(ItemInstance item)
    {
#if IL2CPP
        transform.SetParent(Singleton<ViewmodelAvatar>.Instance.RightHandContainer);
        if (AnimatorController != null)
        {
            Singleton<ViewmodelAvatar>.Instance.SetAnimatorController(AnimatorController);
            Singleton<ViewmodelAvatar>.Instance.SetVisibility(true);
            Singleton<ViewmodelAvatar>.Instance.SetOffset(ViewmodelAvatarOffset);
            Singleton<ViewmodelAvatar>.Instance.SetRotationOffset(ViewmodelAvatarRotationOffset);
        }

        RegisterEquippedItem(item);
        transform.localPosition = localPosition;
        transform.localEulerAngles = localEulerAngles;
        transform.localScale = localScale;
        ApplyViewmodelLayerAndShadows();
        PlayEquipAnimation();
#else
        base.Equip(item);
#endif
    }

    private void EquipHandsFree(ItemInstance item)
    {
        ApplyHandsFreeEquippableTransform();

        var mount = ViewmodelMountHelper.ResolveParent();
        if (mount != null)
            transform.SetParent(mount, false);

        Singleton<ViewmodelAvatar>.Instance.SetVisibility(false);

#if IL2CPP
        RegisterEquippedItem(item);
        transform.localPosition = localPosition;
        transform.localEulerAngles = localEulerAngles;
        transform.localScale = localScale;
        ApplyViewmodelLayerAndShadows();
        PlayEquipAnimation();
#else
        itemInstance = item;
        PlayerSingleton<PlayerInventory>.Instance.SetEquippable(this);
        PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
        transform.localPosition = localPosition;
        transform.localEulerAngles = localEulerAngles;
        transform.localScale = localScale;
        ApplyViewmodelLayerAndShadows();
        PlayEquipAnimation();
#endif
    }

    protected GameObject CreateGlbThirdPersonModel(
        Transform parent,
        string embeddedResourceName,
        string relativePathFromModDir,
        string objectName)
    {
        var model = WeaponGlbLoader.LoadThirdPersonModel(embeddedResourceName, relativePathFromModDir);
        if (model == null)
            return null;

        model.name = objectName;
        WeaponGlbLoader.AttachProfiledThirdPersonModel(model, parent, ActiveProfile);
        return model;
    }

    public override void Equip(ItemInstance item)
    {
        ActiveEquipped = this;
        gameObject.SetActive(true);
        ApplyTemplateSettings();
        WeaponItem = item as IntegerItemInstance;

        if (UsesAvatarHands)
        {
            EnsureViewmodel();
            EquipWithAvatarHands(item);
        }
        else
        {
            EquipHandsFree(item);
            EnsureViewmodel();
            FinalizeFloatingViewmodel();
        }

        OnWeaponEquipped();
        if (UsesAvatarHands)
        {
            ViewmodelAvatarHandHelper.CaptureBaseline();
            ApplyAvatarHandOffsets(GetViewmodelProfile());
        }
        else
        {
            _mapPoseBlend = 0f;
        }

        TimeSinceFire = InitialTimeSinceFire;
        TimeEquipped = 0f;
        _aim = 0f;
        _aimStarted = false;
        UpdateViewmodelAim();

        Singleton<HUD>.Instance.SetCrosshairVisible(false);
        Singleton<InputPromptsCanvas>.Instance.LoadModule("gun");
    }

    protected virtual void OnWeaponEquipped() { }

    public override void Unequip()
    {
        if (ActiveEquipped == this)
            ActiveEquipped = null;

        ViewmodelEditor.ClearSession(this);
        ViewmodelAvatarHandHelper.Reset();
        OnStopAim();
        StopAim();
        PlayUnequipAnimation();

        Singleton<HUD>.Instance.SetCrosshairVisible(true);
        Singleton<InputPromptsCanvas>.Instance.UnloadModule();
        Singleton<HUD>.Instance.HideFirearmReticle();

        if (!UsesAvatarHands)
            Singleton<ViewmodelAvatar>.Instance.SetVisibility(false);

#if IL2CPP
        Singleton<ViewmodelAvatar>.Instance.SetVisibility(false);
        PlayerSingleton<PlayerInventory>.Instance.SetEquippable(null);
        PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
        UnityEngine.Object.Destroy(gameObject);
#else
        base.Unequip();
#endif
    }

    protected override void PlayEquipAnimation()
    {
        if (!UsesAvatarHands)
        {
            Player.Local.SendEquippable_Networked(string.Empty);

            if (_thirdPersonSetupRoutine != null)
                StopCoroutine(_thirdPersonSetupRoutine);
            _thirdPersonSetupRoutine = StartCoroutine(SetupThirdPersonPlaceholder());
            return;
        }

        if (AvatarEquippable != null)
            Player.Local.SendEquippable_Networked(AvatarEquippable.AssetPath);

        if (_thirdPersonSetupRoutine != null)
            StopCoroutine(_thirdPersonSetupRoutine);
        _thirdPersonSetupRoutine = StartCoroutine(SetupThirdPersonPlaceholder());

        if (!string.IsNullOrEmpty(EquipTrigger))
            Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(EquipTrigger);
    }

    protected override void PlayUnequipAnimation()
    {
        if (_thirdPersonSetupRoutine != null)
        {
            StopCoroutine(_thirdPersonSetupRoutine);
            _thirdPersonSetupRoutine = null;
        }

        DestroyThirdPersonModel();
        _thirdPersonParent = null;

        if (AvatarEquippable != null)
            Player.Local.SendEquippable_Networked(string.Empty);
    }

#if IL2CPP
    public
#else
    protected
#endif
    override void Update()
    {
#if !IL2CPP
        base.Update();
#endif

        var editorBound = ViewmodelEditor.IsBoundTo(this);

        if (!editorBound && (Time.timeScale == 0f || Singleton<PauseMenu>.Instance.IsPaused))
            return;

        if (NukeMapOverlay.IsOpen)
        {
            if (!UsesAvatarHands)
                ApplyFloatingPresentation();
            return;
        }

        if (isActiveAndEnabled)
            ActiveEquipped = this;

        if (editorBound)
        {
            EditorEnsureVisible();
            UpdateViewmodelAim();
            return;
        }

        TimeEquipped += Time.deltaTime;
        TimeSinceFire += Time.deltaTime;
        UpdateWeaponAccuracy();
        UpdateWeaponReload();
        UpdateWeaponInput();
        UpdateAim();
    }

    protected virtual void UpdateWeaponAccuracy() { }

    protected virtual float GetAimFovReduction() => AimFovReduction;

    protected virtual float GetReticleSpreadAngle() => 0f;

    protected virtual bool UsesFirearmReticle => true;

    protected virtual void UpdateWeaponInput()
    {
        if (GameInput.IsTyping)
            return;

        if (GameInput.GetButtonDown(GameInput.ButtonCode.Reload))
            TryStartWeaponReload();

        if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && CanWeaponFire())
            FireWeapon();
    }

    private void LateUpdate()
    {
        if (UsesAvatarHands && KeepHandsVisibleDuringReload)
            Singleton<ViewmodelAvatar>.Instance.SetVisibility(true);
        else if (!UsesAvatarHands)
            Singleton<ViewmodelAvatar>.Instance.SetVisibility(false);
        else if (ViewmodelEditor.IsBoundTo(this))
            Singleton<ViewmodelAvatar>.Instance.SetVisibility(true);

        if (!UsesAvatarHands)
            ApplyFloatingPresentation();

        UpdateThirdPersonModelVisibility();
        ApplyScopedFovOverride();
    }

    private void ApplyFloatingPresentation()
    {
        var profile = ActiveProfile;
        if (!profile.IsFloating)
            return;

        if (ViewmodelEditor.IsBoundTo(this))
        {
            var previewBlend = ViewmodelEditor.GetMapRaisePreview(this);
            profile.GetFloatingEquippablePose(SupportsMapRaisePose ? previewBlend : 0f, out var previewPos, out var previewRot);
            ApplyFloatingEquippablePose(previewPos, previewRot);
            if (_viewmodel != null)
                profile.ApplyFloatingModelTransform(_viewmodel.transform);
            return;
        }

        var targetBlend = NukeMapOverlay.IsOpen && SupportsMapRaisePose ? 1f : 0f;
        var delta = Time.unscaledDeltaTime * HandsFreeViewmodelSettings.MapPoseLerpSpeed;
        _mapPoseBlend = Mathf.MoveTowards(_mapPoseBlend, targetBlend, delta);

        profile.GetFloatingEquippablePose(SupportsMapRaisePose ? _mapPoseBlend : 0f, out var pos, out var rotation);
        ApplyFloatingEquippablePose(pos, rotation);

        if (_viewmodel != null)
            profile.ApplyFloatingModelTransform(_viewmodel.transform);
    }

    private void UpdateThirdPersonModelVisibility()
    {
        if (_thirdPersonModel == null)
            return;

        var show = ShouldShowThirdPersonModel();
        if (_thirdPersonModel.activeSelf != show)
            _thirdPersonModel.SetActive(show);
    }

    protected virtual bool ShouldShowThirdPersonModel()
    {
        if (!UsesAvatarHands)
            return true;

        var player = Player.Local;
        if (player == null)
            return false;

        var property = player.GetType().GetProperty(
            "ThirdPersonMeshesVisibleToLocalPlayer",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return property != null && (bool)property.GetValue(player);
    }

    private void ApplyTemplateSettings()
    {
        if (_templateApplied)
            return;

        var templateEquippable = Registry.GetItem(TemplateItemId)?.Equippable as Equippable_RangedWeapon;
        if (templateEquippable == null)
            return;

        AnimatorController = templateEquippable.AnimatorController;
        ViewmodelAvatarOffset = templateEquippable.ViewmodelAvatarOffset;
        ViewmodelAvatarRotationOffset = templateEquippable.ViewmodelAvatarRotationOffset;
        localPosition = templateEquippable.localPosition;
        localEulerAngles = templateEquippable.localEulerAngles;
        localScale = templateEquippable.localScale;
        EquipTime = templateEquippable.EquipTime;
        EquipTrigger = templateEquippable.EquipTrigger;
        AvatarEquippable = templateEquippable.AvatarEquippable;
        FireAnimTriggers = templateEquippable.FireAnimTriggers;
        ApplyWeaponTemplateSettings(templateEquippable);

        _templateApplied = true;
    }

    private IEnumerator SetupThirdPersonPlaceholder()
    {
        for (var i = 0; i < 30; i++)
        {
            Transform attachParent = null;

            if (UsesAvatarHands)
            {
                var avatarEquippable = Player.Local?.Avatar?.CurrentEquippable;
                if (avatarEquippable != null)
                {
                    HideAvatarEquippableMeshes(avatarEquippable);
                    attachParent = avatarEquippable.transform;
                }
            }
            else
            {
                attachParent = Player.Local?.Avatar?.RightHandContainer;
            }

            if (attachParent != null)
            {
                EnsureThirdPersonModel(attachParent);
                _thirdPersonSetupRoutine = null;
                yield break;
            }

            yield return null;
        }

        _thirdPersonSetupRoutine = null;
    }

    private static void HideAvatarEquippableMeshes(AvatarEquippable avatarEquippable)
    {
        foreach (var renderer in avatarEquippable.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = false;
    }

    protected virtual bool KeepAimedWhileUsing() => false;

    protected virtual float GetAimHoldFireCooldown() => 0f;

    protected virtual bool WantsAimInput()
    {
        var holdingScope = GameInput.GetButton(GameInput.ButtonCode.SecondaryClick);
        return GetScopedFov().HasValue ? holdingScope : holdingScope || KeepAimedWhileUsing();
    }

    private void UpdateAim()
    {
        if (!UsesAimInput)
            return;

        var scopedFov = GetScopedFov();
        var wantsAim = WantsAimInput();

        if (wantsAim)
            _aim = Mathf.SmoothDamp(_aim, 1f, ref _aimVelocity, AimDuration / 2f);
        else if (TimeSinceFire > GetAimHoldFireCooldown())
            _aim = Mathf.SmoothDamp(_aim, 0f, ref _aimVelocity, AimDuration / 2f);

        if (wantsAim && !_aimStarted)
        {
            _aimStarted = true;
            _fovOverridden = true;
            PlayerSingleton<PlayerMovement>.Instance.AddSprintBlocker("Aiming");
            Player.Local.SendEquippableMessage_Networked("Raise", UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        }
        else if (!wantsAim && _aimStarted)
        {
            StopAim();
        }

        if (_fovOverridden)
        {
            var fovBlend = _aim;
            float fov;
            if (scopedFov.HasValue)
                fov = Mathf.Lerp(Singleton<Settings>.Instance.CameraFOV, scopedFov.Value, fovBlend);
            else
                fov = Singleton<Settings>.Instance.CameraFOV - (fovBlend * GetAimFovReduction());

            var fovDuration = scopedFov.HasValue ? 0f : AimDuration;
            PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(fov, fovDuration);
        }

        UpdateViewmodelAim();
        UpdateFirearmReticle(_aim);
        OnAimAmountChanged(_aim);
    }

    private void ApplyScopedFovOverride()
    {
        var scopedFov = GetScopedFov();
        if (!scopedFov.HasValue || !GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) || _aim < 0.1f)
            return;

        var fov = Mathf.Lerp(Singleton<Settings>.Instance.CameraFOV, scopedFov.Value, _aim);
        PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(fov, 0f);
    }

    protected virtual void OnAimAmountChanged(float aim) { }

    private void UpdateFirearmReticle(float aim)
    {
        if (!UsesFirearmReticle)
            return;

        if (aim > 0.5f)
        {
            Singleton<HUD>.Instance.SetFirearmReticle(GetReticleSpreadAngle());
            Singleton<HUD>.Instance.ShowFirearmReticle();
            return;
        }

        Singleton<HUD>.Instance.HideFirearmReticle();
    }

    protected float AimAmount => _aim;

    protected bool IsEquipAnimDone => TimeEquipped >= EquipTime;

    private void UpdateViewmodelAim()
    {
        var avatar = Singleton<ViewmodelAvatar>.Instance;
        var aim = GetViewmodelAimAmount();

        if (UsesAvatarHands && avatar != null && avatar.Animator != null)
            avatar.Animator.SetFloat("Aim", aim);

        if (_viewmodel != null && UsesAvatarHands)
        {
            var profile = ActiveProfile;
            _viewmodelHipLocalPosition = profile.HipPosition;
            _viewmodel.transform.localRotation = GetViewmodelAimRotation(aim);
            _viewmodel.transform.localPosition = profile.HipPosition + GetViewmodelAimPositionOffset(aim);
            _viewmodel.transform.localScale = Vector3.one * profile.UniformScale;
        }
    }

    private void StopAim()
    {
        if (!_aimStarted)
            return;

        PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(AimDuration);
        PlayerSingleton<PlayerMovement>.Instance.RemoveSprintBlocker("Aiming");
        Player.Local.SendEquippableMessage_Networked("Lower", UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        _aimStarted = false;
        _fovOverridden = false;
        OnStopAim();
    }

    protected virtual void OnStopAim() { }

    protected void PlayFireAnimation()
    {
        if (!UsesAvatarHands || FireAnimTriggers == null || FireAnimTriggers.Length == 0)
            return;

        var trigger = FireAnimTriggers[UnityEngine.Random.Range(0, FireAnimTriggers.Length)];
        Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(trigger);
    }

    protected Coroutine StartWeaponCoroutine(IEnumerator routine) => StartCoroutine(routine);

    protected void StopWeaponCoroutine(Coroutine routine)
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    private void EnsureViewmodel()
    {
        if (_viewmodel != null)
            return;

        _viewmodel = CreateViewmodel();
        if (_viewmodel == null)
            return;

        _viewmodel.transform.SetParent(transform, false);
        _viewmodelHipLocalPosition = _viewmodel.transform.localPosition;
    }

    private void FinalizeFloatingViewmodel()
    {
        if (_viewmodel == null || UsesAvatarHands)
            return;

        _viewmodel.SetActive(true);
        ApplyFloatingPresentation();

        foreach (var renderer in _viewmodel.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = true;
    }

    protected virtual GameObject CreateViewmodel()
    {
        var visuals = Visuals;
        return CreateBarModel(
            visuals.ViewmodelName,
            visuals.ViewmodelPosition,
            visuals.ViewmodelScale,
            Quaternion.Euler(0f, 90f, 0f),
            visuals.BarColor,
            LayerMask.NameToLayer("Viewmodel"));
    }

    private void EnsureThirdPersonModel(Transform parent)
    {
        if (_thirdPersonModel != null)
            return;

        _thirdPersonParent = parent;
        _thirdPersonModel = CreateThirdPersonModel(parent);
    }

    protected virtual GameObject CreateThirdPersonModel(Transform parent)
    {
        var visuals = Visuals;
        var model = CreateBarModel(
            visuals.ThirdPersonName,
            visuals.ThirdPersonPosition,
            Vector3.one,
            Quaternion.identity,
            visuals.BarColor,
            LayerMask.NameToLayer("Default"));
        model.transform.SetParent(parent, false);

        var parentScale = parent.lossyScale;
        model.transform.localScale = new Vector3(
            visuals.ThirdPersonThickness / Mathf.Max(parentScale.x, 0.001f),
            visuals.ThirdPersonThickness / Mathf.Max(parentScale.y, 0.001f),
            visuals.ThirdPersonLength / Mathf.Max(parentScale.z, 0.001f));

        return model;
    }

    private void DestroyThirdPersonModel()
    {
        if (_thirdPersonModel == null)
            return;

        UnityEngine.Object.Destroy(_thirdPersonModel);
        _thirdPersonModel = null;
    }

    private static GameObject CreateBarModel(
        string name,
        Vector3 localPosition,
        Vector3 localScale,
        Quaternion localRotation,
        Color color,
        int layer)
    {
        return ProceduralVisualFactory.CreateEllipsoid(
            name,
            null,
            localPosition,
            localRotation,
            localScale,
            color,
            layer);
    }
}

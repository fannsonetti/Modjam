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

using S1API.Items;
using UnityEngine;
using UnityEngine.Rendering;
using MoreWeapons.Utils;
using GameItemInstance = ScheduleOne.ItemFramework.ItemInstance;

namespace MoreWeapons.Weapons;

/// <summary>
/// Hands-free first-person items on the vanilla cuke / viewmodel path (equipContainer + visible arms).
/// </summary>
public abstract class CukeViewmodelEquippable : Equippable_Viewmodel
{
#if IL2CPP
    protected CukeViewmodelEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    private GameObject _viewmodel;
    private bool _templateApplied;
    protected float TimeSinceEquip;

    protected virtual bool SupportsMapRaisePose => false;

    internal static CukeViewmodelEquippable ActiveEquipped { get; private set; }

    internal abstract ViewmodelProfile GetProfile();

    private ViewmodelProfile ActiveProfile => ViewmodelEditor.GetEffectiveProfile(this);

    internal bool EditorCanBind =>
        (isActiveAndEnabled && gameObject.activeInHierarchy) || ViewmodelEditor.IsBoundTo(this);

    internal bool SupportsMapRaisePoseEditor => SupportsMapRaisePose;

    internal void EditorApplyProfile(ViewmodelProfile profile)
    {
        var blend = SupportsMapRaisePose ? ViewmodelEditor.GetMapRaisePreview(this) : 0f;
        profile.GetFloatingEquippablePose(blend, out var pos, out var rotation);
        transform.localPosition = pos;
        transform.localRotation = rotation;
        localPosition = pos;
        localEulerAngles = rotation.eulerAngles;

        if (_viewmodel != null)
            profile.ApplyFloatingModelTransform(_viewmodel.transform);
    }

    internal void EditorSyncLiveViewmodel(ref ViewmodelProfile profile)
    {
        profile.EquippableLocalPosition = transform.localPosition;
        profile.EquippableLocalEuler = transform.localEulerAngles;
        EnsureViewmodel();
        if (_viewmodel == null)
            return;

        profile.ModelLocalPosition = _viewmodel.transform.localPosition;
        profile.ModelLocalEuler = _viewmodel.transform.localRotation.eulerAngles;
        profile.ModelUniformScale = _viewmodel.transform.localScale.x;
    }

    /// <summary>
    /// Bakes profile pose onto the registry prefab so inventory / slot previews use equippable rotation.
    /// </summary>
    internal void PrepareRegistryPrefab()
    {
        _templateApplied = false;
        ApplyTemplateSettings();
        ApplyEquippableTransform();
        EnsureViewmodel();
        FinalizeViewmodel();
    }

    protected abstract GameObject CreateViewmodel();

    protected virtual void OnCukeEquipped(GameItemInstance item) { }

    protected virtual void OnCukeUpdate() { }

    protected virtual void Awake()
    {
        ApplyTemplateSettings();
        ApplyEquippableTransform();
    }

    public override void Equip(GameItemInstance item)
    {
        ActiveEquipped = this;
        gameObject.SetActive(true);
        ApplyTemplateSettings();

        EnsureViewmodel();
        Singleton<ViewmodelAvatar>.Instance.SetVisibility(true);
        FinalizeViewmodel();

#if IL2CPP
        RegisterEquippedItem(item);
        ApplyEquippableTransform();
        ApplyViewmodelLayerAndShadows();
        PlayEquipAnimation();
#else
        base.Equip(item);
#endif

        TimeSinceEquip = 0f;
        _mapPoseBlend = 0f;
        OnCukeEquipped(item);

        Singleton<HUD>.Instance.SetCrosshairVisible(false);
        Singleton<InputPromptsCanvas>.Instance.LoadModule("gun");
    }

    public override void Unequip()
    {
        if (ActiveEquipped == this)
            ActiveEquipped = null;

        ViewmodelEditor.ClearSession(this);
        DestroyViewmodel();

#if IL2CPP
        PlayerSingleton<PlayerInventory>.Instance.SetEquippable(null);
        PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
        UnityEngine.Object.Destroy(gameObject);
#else
        base.Unequip();
#endif

        Singleton<HUD>.Instance.SetCrosshairVisible(true);
        Singleton<InputPromptsCanvas>.Instance.UnloadModule();
    }

    protected override void PlayEquipAnimation()
    {
        Player.Local.SendEquippable_Networked(string.Empty);
    }

    protected override void PlayUnequipAnimation()
    {
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

        if (editorBound)
            return;

        TimeSinceEquip += Time.deltaTime;

        if (SupportsMapRaisePose)
            UpdateMapRaisePose();

        OnCukeUpdate();
    }

    protected GameObject CreateGlbViewmodel(string embeddedResourceName, string relativePathFromModDir, string objectName)
    {
        var model = WeaponGlbLoader.LoadViewmodel(embeddedResourceName, relativePathFromModDir);
        if (model == null)
            return null;

        model.name = objectName;
        ApplyModelTransform(model);
        return model;
    }

    protected GameObject CreateFallbackBar(string name, Vector3 scale, Color color)
    {
        var profile = ActiveProfile;
        return ProceduralVisualFactory.CreateEllipsoid(
            name,
            null,
            profile.ModelLocalPosition,
            Quaternion.Euler(profile.ModelLocalEuler),
            scale,
            color,
            LayerMask.NameToLayer("Viewmodel"));
    }

    protected bool CanUsePrimaryInput()
    {
        if (GameInput.IsTyping)
            return false;

        var camera = PlayerSingleton<PlayerCamera>.Instance;
        return camera == null || camera.activeUIElementCount == 0;
    }

    private float _mapPoseBlend;

    private void ApplyTemplateSettings()
    {
        if (_templateApplied)
            return;

        AvatarEquippable = null;

        var profile = ActiveProfile;
        localPosition = profile.EquippableLocalPosition;
        localEulerAngles = profile.EquippableLocalEuler;
        localScale = Vector3.one;

        var cukeEquippable = Registry.GetItem(Core.GrenadeTemplateItemId)?.Equippable as Equippable_Viewmodel;
        if (cukeEquippable != null && profile.EquippableLocalPosition == Vector3.zero)
        {
            localPosition = cukeEquippable.localPosition;
            localEulerAngles = cukeEquippable.localEulerAngles;
            localScale = cukeEquippable.localScale;
        }

        _templateApplied = true;
    }

    private void RegisterEquippedItem(GameItemInstance item)
    {
        itemInstance = item;
        PlayerSingleton<PlayerInventory>.Instance.SetEquippable(this);
        PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
    }

    private void ApplyEquippableTransform()
    {
        transform.localPosition = localPosition;
        transform.localEulerAngles = localEulerAngles;
        transform.localScale = localScale;
    }

    private void ApplyViewmodelLayerAndShadows()
    {
        LayerUtility.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Viewmodel"));
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                renderer.enabled = false;
            else
                renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    private void EnsureViewmodel()
    {
        if (_viewmodel != null)
            return;

        _viewmodel = TryAdoptExistingViewmodelChild();
        if (_viewmodel == null)
            _viewmodel = CreateViewmodel();

        if (_viewmodel == null)
            return;

        _viewmodel.transform.SetParent(transform, false);
        ApplyModelTransform(_viewmodel);
    }

    private GameObject TryAdoptExistingViewmodelChild()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            if (child.name.EndsWith("Viewmodel"))
                return child;
        }

        return null;
    }

    private void FinalizeViewmodel()
    {
        if (_viewmodel == null)
            return;

        _viewmodel.SetActive(true);
        ApplyModelTransform(_viewmodel);
        ApplyViewmodelLayerAndShadows();

        foreach (var renderer in _viewmodel.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = true;
    }

    private void ApplyModelTransform(GameObject model)
    {
        ActiveProfile.ApplyFloatingModelTransform(model.transform);
        LayerUtility.SetLayerRecursively(model, LayerMask.NameToLayer("Viewmodel"));
    }

    private void DestroyViewmodel()
    {
        if (_viewmodel == null)
            return;

        UnityEngine.Object.Destroy(_viewmodel);
        _viewmodel = null;
    }

    private void UpdateMapRaisePose()
    {
        if (ViewmodelEditor.IsBoundTo(this))
            return;

        var profile = ActiveProfile;
        var targetBlend = NukeMapOverlay.IsOpen ? 1f : 0f;
        var delta = Time.unscaledDeltaTime * HandsFreeViewmodelSettings.MapPoseLerpSpeed;
        _mapPoseBlend = Mathf.MoveTowards(_mapPoseBlend, targetBlend, delta);

        profile.GetFloatingEquippablePose(_mapPoseBlend, out var pos, out var rotation);
        transform.localPosition = pos;
        transform.localRotation = rotation;
        localPosition = pos;
        localEulerAngles = rotation.eulerAngles;
    }
}

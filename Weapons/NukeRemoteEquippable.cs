#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
#endif

using UnityEngine;
using MoreWeapons.Utils;
using GameItemInstance = ScheduleOne.ItemFramework.ItemInstance;

namespace MoreWeapons.Weapons;

public sealed class NukeRemoteEquippable : CukeViewmodelEquippable
{
#if IL2CPP
    public NukeRemoteEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    private IntegerItemInstance _weaponItem;
    private float _timeSinceUse;

    protected override bool SupportsMapRaisePose => true;

    internal override ViewmodelProfile GetProfile() => WeaponViewmodelProfiles.Ndt;

    protected override GameObject CreateViewmodel()
    {
        return CreateGlbViewmodel("MoreWeapons.Assets.Models.rdu.glb", "Assets/Models/rdu.glb", "NdtViewmodel")
            ?? CreateFallbackBar("NdtViewmodel", new Vector3(0.08f, 0.04f, 0.12f), new Color(0.85f, 0.2f, 0.15f));
    }

    protected override void OnCukeEquipped(GameItemInstance item)
    {
        _weaponItem = item as IntegerItemInstance;
        _timeSinceUse = 0.5f;
    }

    protected override void OnCukeUpdate()
    {
        _timeSinceUse += Time.deltaTime;

        if (!CanUsePrimaryInput() || NukeMapOverlay.IsOpen)
            return;

        if (!GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick)
            || _weaponItem == null
            || _weaponItem.Value <= 0
            || _timeSinceUse < 0.5f
            || TimeSinceEquip < 0.4f)
            return;

        _timeSinceUse = 0f;
        NukeMapOverlay.Open(
            OnStrikeConfirmed,
            HasChargesRemaining);
    }

    private bool HasChargesRemaining() => _weaponItem != null && _weaponItem.Value > 0;

    private void OnStrikeConfirmed(Vector3 point, bool dealDamage)
    {
        if (_weaponItem == null || _weaponItem.Value <= 0)
            return;

        _weaponItem.ChangeValue(-1);
        NukeStrikeController.OrderStrike(point, dealDamage);
    }
}

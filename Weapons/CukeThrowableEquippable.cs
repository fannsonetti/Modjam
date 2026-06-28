#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
#endif

using ScheduleOne.Storage;
using MoreWeapons.Utils;
using GameItemInstance = ScheduleOne.ItemFramework.ItemInstance;

namespace MoreWeapons.Weapons;

public abstract class CukeThrowableEquippable : CukeViewmodelEquippable
{
#if IL2CPP
    protected CukeThrowableEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    protected const float ThrowCooldown = 0.35f;
    private const float EquipAnimTime = 0.4f;

    protected StorableItemInstance StorableItem;
    protected float TimeSinceThrow = ThrowCooldown;

    protected bool CanThrow =>
        StorableItem != null
        && StorableItem.Quantity > 0
        && TimeSinceThrow >= ThrowCooldown
        && TimeSinceEquip >= EquipAnimTime
        && !NukeMapOverlay.IsOpen;

    protected override void OnCukeEquipped(GameItemInstance item)
    {
        StorableItem = item as StorableItemInstance;
        TimeSinceThrow = ThrowCooldown;
    }

    protected override void OnCukeUpdate()
    {
        TimeSinceThrow += UnityEngine.Time.deltaTime;

        if (!CanUsePrimaryInput())
            return;

        if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && CanThrow)
            Throw(ThrowableWeaponUtility.PrimaryThrowSpeed);
        else if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) && CanThrow)
            Throw(ThrowableWeaponUtility.SoftThrowSpeed);
    }

    protected abstract void Throw(float throwSpeed);

    protected void ConsumeOne()
    {
        if (StorableItem == null)
            return;

        StorableItem.ChangeQuantity(-1);
        if (StorableItem.Quantity <= 0)
            Unequip();
    }
}

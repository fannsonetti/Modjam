#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
#else
using ScheduleOne;
using ScheduleOne.ItemFramework;
#endif

using UnityEngine;
using MoreWeapons.Utils;
using GameItemInstance = ScheduleOne.ItemFramework.ItemInstance;

namespace MoreWeapons.Weapons;

public sealed class MagazineEquippable : CukeViewmodelEquippable
{
#if IL2CPP
    public MagazineEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    private string _magazineItemId;

    internal override ViewmodelProfile GetProfile() => WeaponViewmodelProfiles.Magazine;

    public override void Equip(GameItemInstance item)
    {
        _magazineItemId = item?.ID;
        base.Equip(item);
    }

    protected override GameObject CreateViewmodel()
    {
        if (MagazineGlbCatalog.TryGet(_magazineItemId, out var glb))
        {
            return CreateGlbViewmodel(glb.EmbeddedResourceName, glb.RelativePathFromModDir, "MagazineViewmodel")
                   ?? CreateFallbackBar("MagazinePlaceholder", new Vector3(0.04f, 0.04f, 0.08f), new Color(0.45f, 0.45f, 0.48f));
        }

        return CreateFallbackBar("MagazinePlaceholder", new Vector3(0.04f, 0.04f, 0.08f), new Color(0.45f, 0.45f, 0.48f));
    }
}

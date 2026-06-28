#if IL2CPP
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Levelling;
#else
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
#endif

namespace MoreWeapons.Utils;

internal static class WeaponRanks
{
    // Rank tiers are locked to 1, 3, and 5 only.
    internal static readonly FullRank Hustler1 = new(ERank.Hustler, 1);
    internal static readonly FullRank Hustler3 = new(ERank.Hustler, 3);
    internal static readonly FullRank Enforcer3 = new(ERank.Enforcer, 3);
    internal static readonly FullRank ShotCaller3 = new(ERank.Shot_Caller, 3);
    internal static readonly FullRank BlockBoss3 = new(ERank.Block_Boss, 3);
    internal static readonly FullRank BlockBoss5 = new(ERank.Block_Boss, 5);
    internal static readonly FullRank Underlord5 = new(ERank.Underlord, 5);
    internal static readonly FullRank Baron5 = new(ERank.Baron, 5);
    internal static readonly FullRank Kingpin5 = new(ERank.Kingpin, 5);

    internal static void ApplyRankGate(StorableItemDefinition definition, FullRank rank)
    {
        if (definition == null)
            return;

        definition.RequiresLevelToPurchase = true;
        definition.RequiredRank = rank;
    }
}

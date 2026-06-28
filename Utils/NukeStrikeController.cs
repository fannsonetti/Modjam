using MelonLoader;
using System.Collections;
using UnityEngine;
using MoreWeapons.NPCs;
using MoreWeapons.Weapons;

namespace MoreWeapons.Utils;

internal static class NukeStrikeController
{
    internal static void OrderStrike(Vector3 groundTarget, bool dealDamage)
    {
        MelonCoroutines.Start(StrikeRoutine(groundTarget, dealDamage));
    }

    private static IEnumerator StrikeRoutine(Vector3 groundTarget, bool dealDamage)
    {
        var delay = Mathf.Max(5f, Core.NukeStrikeDelay);
        NukeStrikeContact.Instance?.SendStrikeEta(delay);

        yield return new WaitForSeconds(delay);
        Launch(groundTarget, dealDamage);
    }

    internal static void Launch(Vector3 groundTarget, bool dealDamage)
    {
        var planeObject = new GameObject("NukeBomberPlane");
        var plane = planeObject.AddComponent<NukeBomberPlane>();
        plane.BeginStrike(groundTarget, dealDamage);
    }
}

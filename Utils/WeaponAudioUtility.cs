#if IL2CPP
using Il2CppScheduleOne.Audio;
using Il2CppScheduleOne.Equipping;
#else
using ScheduleOne.Audio;
using ScheduleOne.Equipping;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreWeapons.Utils;

internal static class WeaponAudioUtility
{
    internal static void PlayPumpCockSound(Equippable_RangedWeapon template, float volumeMultiplier = 1f)
    {
        if (template == null)
            return;

        var instance = Object.Instantiate(template);
        var ranged = instance.GetComponent<Equippable_RangedWeapon>() ?? instance;
        instance.gameObject.SetActive(true);

        ranged.onCockStart?.Invoke();

        foreach (var audio in instance.GetComponentsInChildren<AudioSourceController>(true))
        {
            if (audio == null || audio == ranged.FireSound || audio == ranged.EmptySound)
                continue;

            if (volumeMultiplier != 1f)
                audio.VolumeMultiplier = volumeMultiplier;

            audio.Play();
        }

        Object.Destroy(instance.gameObject, 2f);
    }
}

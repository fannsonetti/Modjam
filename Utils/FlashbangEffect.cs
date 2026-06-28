#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
#endif

using MelonLoader;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class FlashbangEffect
{
    private const float MaxRadius = 14f;
    private const float MaxBlindDuration = 5f;
    private const float FullFlashAngleDegrees = 75f;
    private const float PartialFlashAngleDegrees = 165f;
    private const float MinPlayerStrength = 0.008f;

    internal static void Burst(Vector3 position)
    {
        ApplyToPlayer(Player.Local, position);

        foreach (var npc in NPCManager.NPCRegistry)
            ApplyToNpc(npc, position);
    }

    private static void ApplyToPlayer(Player player, Vector3 flashPosition)
    {
        if (player == null)
            return;

        var camera = PlayerSingleton<PlayerCamera>.Instance;
        if (camera == null)
            return;

        var eyePosition = camera.transform.position;
        var strength = CalculatePlayerStrength(eyePosition, camera.transform.forward, flashPosition);
        if (strength <= MinPlayerStrength)
            return;

        FlashbangScreenEffect.Play(strength);

        if (!player.IsOwner)
            return;

        player.Disoriented = true;
        if (player.Avatar?.Eyes != null)
        {
            player.Avatar.Eyes.leftEye.AngleOffset = new Vector2(20f, 10f);
            player.Avatar.Eyes.rightEye.AngleOffset = new Vector2(-20f, -10f);
        }

        camera.SmoothLookSmoother.AddOverride(Mathf.Lerp(0.35f, 0.9f, strength), 1, "flashbang");
        MelonCoroutines.Start(ClearPlayerFlash(player, camera, MaxBlindDuration * strength));
    }

    private static void ApplyToNpc(NPC npc, Vector3 flashPosition)
    {
        if (npc == null || !npc.gameObject.activeInHierarchy || npc.Health == null || npc.Health.IsDead)
            return;

        var eyePosition = npc.transform.position + Vector3.up * 1.55f;
        if (!HasLineOfSight(eyePosition, flashPosition))
            return;

        var forward = npc.Avatar != null ? npc.Avatar.transform.forward : npc.transform.forward;
        var strength = CalculateNpcStrength(eyePosition, forward, flashPosition);
        if (strength <= 0.03f)
            return;

        if (npc.Movement != null)
            npc.Movement.Disoriented = true;

        if (npc.Avatar?.EmotionManager != null)
            npc.Avatar.EmotionManager.AddEmotionOverride("Concerned", "flashbang");

        MelonCoroutines.Start(ClearNpcFlash(npc, 2.5f + strength * 4f));
    }

    private static float CalculatePlayerStrength(Vector3 eyePosition, Vector3 lookDirection, Vector3 flashPosition)
    {
        var toFlash = flashPosition - eyePosition;
        var distance = toFlash.magnitude;
        if (distance > MaxRadius)
            return 0f;

        if (!HasLineOfSight(eyePosition, flashPosition))
            return 0f;

        var distanceFactor = 1f - Mathf.Clamp01(distance / MaxRadius);
        var angle = Vector3.Angle(lookDirection, toFlash.normalized);

        float angleFactor;
        if (angle <= FullFlashAngleDegrees)
            angleFactor = 1f;
        else if (angle <= PartialFlashAngleDegrees)
            angleFactor = 1f - (angle - FullFlashAngleDegrees) / (PartialFlashAngleDegrees - FullFlashAngleDegrees);
        else
            angleFactor = 0.4f;

        var strength = distanceFactor * distanceFactor * Mathf.Max(angleFactor, 0.4f);
        if (distance <= 6f)
            strength = Mathf.Max(strength, 0.55f * (1f - distance / MaxRadius));

        // If the burst is anywhere in front of the player, guarantee a partial flash.
        if (angle <= PartialFlashAngleDegrees && distance <= MaxRadius * 0.85f)
            strength = Mathf.Max(strength, 0.22f * distanceFactor);

        return strength;
    }

    private static float CalculateNpcStrength(Vector3 eyePosition, Vector3 lookDirection, Vector3 flashPosition)
    {
        var toFlash = flashPosition - eyePosition;
        var distance = toFlash.magnitude;
        if (distance > MaxRadius)
            return 0f;

        var distanceFactor = 1f - Mathf.Clamp01(distance / MaxRadius);
        var angle = Vector3.Angle(lookDirection, toFlash.normalized);
        var angleFactor = 1f - Mathf.Clamp01(angle / PartialFlashAngleDegrees);
        return distanceFactor * distanceFactor * angleFactor;
    }

    private static bool HasLineOfSight(Vector3 eyePosition, Vector3 flashPosition)
    {
        var direction = flashPosition - eyePosition;
        var distance = direction.magnitude;
        if (distance < 0.05f)
            return true;

        // Stop short of the burst point so the surface the flash sits on doesn't count as blocking.
        var checkDistance = Mathf.Max(0f, distance - 0.2f);
        if (checkDistance < 0.05f)
            return true;

        if (!Physics.Raycast(
                eyePosition,
                direction.normalized,
                out var hit,
                checkDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            return true;

        return (hit.point - flashPosition).sqrMagnitude <= 3f * 3f;
    }

    private static System.Collections.IEnumerator ClearPlayerFlash(Player player, PlayerCamera camera, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (player == null)
            yield break;

        player.Disoriented = false;
        if (player.Avatar?.Eyes != null)
        {
            player.Avatar.Eyes.leftEye.AngleOffset = Vector2.zero;
            player.Avatar.Eyes.rightEye.AngleOffset = Vector2.zero;
        }

        if (camera != null)
            camera.SmoothLookSmoother.RemoveOverride("flashbang");
    }

    private static System.Collections.IEnumerator ClearNpcFlash(NPC npc, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (npc == null)
            yield break;

        if (npc.Movement != null)
            npc.Movement.Disoriented = false;

        if (npc.Avatar?.EmotionManager != null)
            npc.Avatar.EmotionManager.RemoveEmotionOverride("flashbang");
    }
}

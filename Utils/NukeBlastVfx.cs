using System.Collections;
using MelonLoader;
using UnityEngine;

namespace MoreWeapons.Utils;

/// <summary>
/// Immediate nuke detonation visuals — the base game explosion VFX does not scale with radius,
/// so this provides a massive shockwave / fireball separate from CreateExplosion.
/// </summary>
internal static class NukeBlastVfx
{
    internal static void Spawn(Vector3 groundPoint, float visualRadius)
    {
        var root = new GameObject("NukeBlastVfx");
        root.transform.position = groundPoint;
        MelonCoroutines.Start(PlayBlast(root.transform, visualRadius));
        ApplyScreenShake(visualRadius);
    }

    private static void ApplyScreenShake(float visualRadius)
    {
#if IL2CPP
        var camera = Il2CppScheduleOne.DevUtilities.PlayerSingleton<Il2CppScheduleOne.PlayerScripts.PlayerCamera>.Instance;
#else
        var camera = ScheduleOne.DevUtilities.PlayerSingleton<ScheduleOne.PlayerScripts.PlayerCamera>.Instance;
#endif
        camera?.StartCameraShake(Mathf.Clamp(visualRadius / 40f, 0.8f, 2.5f), 1.2f);
    }

    private static IEnumerator PlayBlast(Transform root, float visualRadius)
    {
        SpawnFlashBurst(root, visualRadius);
        yield return null;

        for (var ring = 0; ring < 4; ring++)
        {
            var delay = ring * 0.22f;
            var maxSize = Mathf.Lerp(visualRadius * 0.14f, visualRadius * 0.6f, ring / 3f);
            var duration = Mathf.Lerp(2.8f, 4.8f, ring / 3f);
            MelonCoroutines.Start(SpawnParticleRing(root.position, maxSize, duration, delay, ring));
        }

        MelonCoroutines.Start(SpawnParticleRing(root.position, visualRadius * 0.42f, 3.2f, 0.05f, -1, fire: true));
        MelonCoroutines.Start(SpawnParticleRing(root.position, visualRadius * 0.38f, 4.8f, 0.35f, -2, smoke: true));

        yield return new WaitForSeconds(Mathf.Max(12f, visualRadius * 0.08f));
        if (root != null)
            UnityEngine.Object.Destroy(root.gameObject);
    }

    private static void SpawnFlashBurst(Transform root, float visualRadius)
    {
        var flash = new GameObject("NukeFlashBurst");
        flash.transform.SetParent(root, false);
        flash.transform.localPosition = Vector3.up * (visualRadius * 0.04f);

        var ps = flash.AddComponent<ParticleSystem>();
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var flashMaterial = VfxFlipbookCatalog.GetMaterial(VfxFlipbookId.Explosion02);
        var usingFlipbook = flashMaterial != null;
        renderer.material = usingFlipbook
            ? flashMaterial
            : ParticleMaterialHelper.CreateSoftParticleMaterial(new Color(1f, 0.95f, 0.75f, 1f));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.5f;
        main.startLifetime = 2.5f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(visualRadius * 0.14f, visualRadius * 0.32f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.98f, 0.88f, 1f),
            new Color(1f, 0.45f, 0.05f, 0.85f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 120;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = visualRadius * 0.08f;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        if (usingFlipbook)
            VfxFlipbookCatalog.ApplyTextureSheetAnimation(ps, VfxFlipbookId.Explosion02, 1.2f);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = visualRadius * 0.35f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.EaseInOut(0f, 0.6f, 1f, 2.8f));

        ps.Play();
        UnityEngine.Object.Destroy(flash, 5f);
    }

    private static IEnumerator SpawnParticleRing(
        Vector3 origin,
        float maxSize,
        float duration,
        float delay,
        int ringIndex,
        bool fire = false,
        bool smoke = false)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        var ring = new GameObject(fire ? "NukeFireParticleRing" : smoke ? "NukeSmokeParticleRing" : "NukeShockParticleRing");
        ring.name = fire ? "NukeFireRing" : smoke ? "NukeSmokeRing" : "NukeShockRing";
        ring.transform.position = origin;

        Color baseColor;
        if (fire)
            baseColor = new Color(1f, 0.55f, 0.12f, 0.75f);
        else if (smoke)
            baseColor = new Color(0.18f, 0.17f, 0.16f, 0.78f);
        else
            baseColor = new Color(1f, 0.82f, 0.35f, 0.55f - ringIndex * 0.06f);

        var ps = ring.AddComponent<ParticleSystem>();
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        VfxFlipbookId flipbookId = VfxFlipbookId.None;
        Material ringMaterial = null;
        var usingFlipbook = false;
        if (smoke)
        {
            flipbookId = VfxFlipbookId.WispySmoke03;
            ringMaterial = VfxFlipbookCatalog.GetMaterial(flipbookId);
            usingFlipbook = ringMaterial != null;
            if (!usingFlipbook)
                ringMaterial = ParticleMaterialHelper.CreateProceduralSmokeParticleMaterial(baseColor, false);
        }
        else if (fire)
        {
            flipbookId = VfxFlipbookId.FireBall02;
            ringMaterial = VfxFlipbookCatalog.GetMaterial(flipbookId);
            usingFlipbook = ringMaterial != null;
            if (!usingFlipbook)
                ringMaterial = ParticleMaterialHelper.CreateSoftParticleMaterial(baseColor);
        }
        else
        {
            flipbookId = VfxFlipbookId.Explosion01NoFire;
            ringMaterial = VfxFlipbookCatalog.GetMaterial(flipbookId);
            usingFlipbook = ringMaterial != null;
            if (!usingFlipbook)
                ringMaterial = ParticleMaterialHelper.CreateSoftParticleMaterial(baseColor);
        }

        renderer.material = ringMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 0.45f;
        renderer.sortingFudge = fire ? 0.2f : 0f;

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = duration;
        main.startLifetime = duration;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(maxSize * 0.022f, maxSize * (fire ? 0.048f : smoke ? 0.075f : 0.03f));
        main.startColor = baseColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = Mathf.Clamp(Mathf.RoundToInt(maxSize * (smoke ? 1.4f : 1.15f)), 48, 220);

        var shape = ps.shape;
        shape.enabled = false;

        if (usingFlipbook)
            VfxFlipbookCatalog.ApplyTextureSheetAnimation(ps, flipbookId, smoke ? 1f : 1.15f);

        var emission = ps.emission;
        emission.enabled = false;

        ApplyRingFade(ps, baseColor, smoke);
        ps.Play();

        var particleCount = main.maxParticles;
        var startRadius = maxSize * (fire ? 0.08f : smoke ? 0.12f : 0.04f);
        var radialSpeed = (maxSize - startRadius) / duration * (smoke ? 0.45f : 0.7f);
        var startY = fire ? 1.2f : smoke ? 1.2f : 0.15f;
        var endY = fire ? 5f : smoke ? 4.5f : 0.45f;
        var upwardSpeed = (endY - startY) / duration;
        var sizeMin = maxSize * (fire ? 0.022f : smoke ? 0.04f : 0.01f);
        var sizeMax = maxSize * (fire ? 0.048f : smoke ? 0.085f : 0.026f);

        for (var i = 0; i < particleCount; i++)
        {
            var angle = ((float)i / particleCount) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.035f, 0.035f);
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            var radius = startRadius + UnityEngine.Random.Range(-startRadius * 0.12f, startRadius * 0.12f);
            var emit = new ParticleSystem.EmitParams
            {
                position = origin + direction * radius + Vector3.up * (startY + UnityEngine.Random.Range(-0.08f, 0.08f)),
                velocity = direction * radialSpeed + Vector3.up * upwardSpeed,
                startLifetime = duration,
                startSize = UnityEngine.Random.Range(sizeMin, sizeMax),
                startColor = baseColor,
                rotation = angle
            };
            ps.Emit(emit, 1);
        }

        if (ring != null)
        {
            var destroyDelay = smoke ? duration + 3.5f : duration + 1.2f;
            UnityEngine.Object.Destroy(ring, destroyDelay);
        }

        yield return new WaitForSeconds((smoke ? duration + 3.5f : duration + 1.2f));
    }

    private static void ApplyRingFade(ParticleSystem ps, Color color, bool smoke)
    {
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();

        if (smoke)
        {
            var darkGrey = new Color(color.r, color.g, color.b, 1f);
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(darkGrey, 0f),
                    new GradientColorKey(darkGrey, 0.55f),
                    new GradientColorKey(Color.Lerp(darkGrey, Color.black, 0.35f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(color.a * 0.96f, 0.35f),
                    new GradientAlphaKey(color.a * 0.82f, 0.55f),
                    new GradientAlphaKey(color.a * 0.58f, 0.72f),
                    new GradientAlphaKey(color.a * 0.32f, 0.86f),
                    new GradientAlphaKey(color.a * 0.12f, 0.95f),
                    new GradientAlphaKey(0f, 1f)
                });
        }
        else
        {
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(color.a * 0.4f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                });
        }

        colorOverLifetime.color = gradient;
    }
}

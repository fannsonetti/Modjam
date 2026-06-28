using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace MoreWeapons.Utils;

/// <summary>
/// Phased nuclear mushroom cloud. Real-world stages are time-compressed for gameplay:
/// fireball (~0-2s) → cap (~2-10s) → stem (~10-60s) → flatten (~1-10min).
/// </summary>
public sealed class MushroomCloudEffect : MonoBehaviour
{
#if IL2CPP
    public MushroomCloudEffect(System.IntPtr ptr) : base(ptr) { }
#endif

    private const float ParticleVisualScale = 0.72f;
    private const float ShapeVisualScale = 0.68f;
    private const float EmissionScale = 0.55f;
    private const float HorizontalVelocityScale = 0.42f;
    private const float RadialMotionScale = 0.45f;
    private const float SwirlMotionScale = 0.35f;
    private const float TurbulenceDriftScale = 0.45f;
    private const float SpinSpeedScale = 0.28f;

    // Compressed game-time phase boundaries — scaled from total duration.
    private float FireballEnd => _fireballEnd;
    private float HeadEnd => _headEnd;
    private float StemEnd => _stemEnd;

    private float CapTargetHeight => 62f * _scale;
    private float StemRadius => 13f * _scale;

    private float _duration;
    private float _scale;
    private float _elapsed;
    private float _fireballEnd;
    private float _headEnd;
    private float _stemEnd;
    private float _lifetimeScale = 2.75f;
    private int _phase;
    private Light _light;
    private Transform _capAnchor;
    private Transform _stemAnchor;
    private readonly List<EmitterRuntime> _emitters = new();
    private bool _windDownStarted;
    private float _windDownElapsed;
    private float _fadeMultiplier = 1f;
    private const float WindDownFadeSeconds = 32f;

    public static void Spawn(Vector3 point, float duration, float radius)
    {
        MelonCoroutines.Start(SpawnDeferred(point, duration, radius));
    }

    private static IEnumerator SpawnDeferred(Vector3 point, float duration, float radius)
    {
        yield return null;

        var root = new GameObject("MushroomCloud");
        root.transform.position = point;
        var effect = root.AddComponent<MushroomCloudEffect>();
        effect.Initialize(duration, radius);
    }

    private void Initialize(float duration, float radius)
    {
        _duration = Mathf.Max(duration, 60f);
        _scale = Mathf.Clamp(radius / 22f, 3.4f, 7f);
        _fireballEnd = _duration * 0.04f;
        _headEnd = _duration * 0.14f;
        _stemEnd = _duration * 0.52f;

        _stemAnchor = new GameObject("StemAnchor").transform;
        _stemAnchor.SetParent(transform, false);

        _capAnchor = new GameObject("CapAnchor").transform;
        _capAnchor.SetParent(transform, false);

        CreateFlashLight();
        EnterPhase1();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        UpdateAnchors();
        UpdateLight();
        UpdateParticleMotion(Time.deltaTime);
        AdvancePhases();
        UpdateWindDown();
    }

    private void UpdateWindDown()
    {
        if (!_windDownStarted && _elapsed >= _duration)
            BeginWindDown();

        if (!_windDownStarted)
            return;

        _windDownElapsed += Time.deltaTime;
        _fadeMultiplier = 1f - SmoothFadeCurve(Mathf.Clamp01(_windDownElapsed / WindDownFadeSeconds));

        if (_fadeMultiplier <= 0.005f && AllParticlesDead())
            Destroy(gameObject);
    }

    private void BeginWindDown()
    {
        _windDownStarted = true;
        _windDownElapsed = 0f;

        for (var i = 0; i < _emitters.Count; i++)
        {
            var ps = _emitters[i].System;
            if (ps == null)
                continue;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private bool AllParticlesDead()
    {
        for (var i = 0; i < _emitters.Count; i++)
        {
            var ps = _emitters[i].System;
            if (ps != null && ps.particleCount > 0)
                return false;
        }

        return true;
    }

    private void AdvancePhases()
    {
        if (_phase < 1 && _elapsed >= FireballEnd)
            EnterPhase2();
        if (_phase < 2 && _elapsed >= HeadEnd)
            EnterPhase3();
        if (_phase < 3 && _elapsed >= StemEnd)
            EnterPhase4();
    }

    private void UpdateAnchors()
    {
        var capHeight = CapTargetHeight;

        if (_elapsed < FireballEnd)
        {
            var t = EaseOutQuad(_elapsed / FireballEnd);
            _capAnchor.localPosition = Vector3.up * (capHeight * 0.35f * t);
        }
        else if (_elapsed < HeadEnd)
        {
            var t = EaseOutCubic((_elapsed - FireballEnd) / (HeadEnd - FireballEnd));
            _capAnchor.localPosition = Vector3.up * Mathf.Lerp(capHeight * 0.35f, capHeight * 0.82f, t);
        }
        else if (_elapsed < StemEnd)
        {
            var t = EaseOutQuad((_elapsed - HeadEnd) / (StemEnd - HeadEnd));
            _capAnchor.localPosition = Vector3.up * Mathf.Lerp(capHeight * 0.82f, capHeight, t);
        }
        else
        {
            _capAnchor.localPosition = Vector3.up * capHeight;
        }

        _stemAnchor.localPosition = Vector3.up * (_capAnchor.localPosition.y * 0.45f);
    }

    private void UpdateLight()
    {
        if (_light == null)
            return;

        if (_elapsed < FireballEnd)
        {
            var flash = 1f - (_elapsed / FireballEnd);
            _light.intensity = Mathf.Lerp(6f, 28f, flash);
            _light.range = 40f * _scale;
            _light.color = Color.Lerp(new Color(1f, 0.95f, 0.85f), new Color(1f, 0.65f, 0.25f), 1f - flash);
        }
        else if (_elapsed < HeadEnd)
        {
            var t = (_elapsed - FireballEnd) / (HeadEnd - FireballEnd);
            _light.intensity = Mathf.Lerp(22f, 10f, t);
            _light.range = Mathf.Lerp(38f, 52f, t) * _scale;
            _light.color = new Color(1f, 0.55f, 0.2f);
        }
        else if (_elapsed < StemEnd)
        {
            var t = (_elapsed - HeadEnd) / (StemEnd - HeadEnd);
            _light.intensity = Mathf.Lerp(10f, 3f, t);
            _light.range = 45f * _scale;
        }
        else
        {
            var t = Mathf.Clamp01((_elapsed - StemEnd) / (_duration - StemEnd));
            _light.intensity = Mathf.Lerp(3f, 0f, t);
        }
    }

    // ── Phase 1: Fireball (0–1.6s) ──────────────────────────────────────────
    private void EnterPhase1()
    {
        _phase = 1;

        SpawnEmitter(new EmitterConfig
        {
            Name = "P1_FireballCore",
            Parent = transform,
            LocalPosition = Vector3.up * (0.5f * _scale),
            StartColor = new Color(1f, 0.98f, 0.9f, 1f),
            EndColor = new Color(0.18f, 0.17f, 0.16f, 0.82f),
            MinSize = 14f * _scale,
            MaxSize = 30f * _scale,
            Lifetime = 3.4f,
            MaxParticles = 340,
            Shape = ParticleSystemShapeType.Sphere,
            Burst = 150,
            Upward = 18f * _scale,
            Outward = 9f * _scale,
            SizeGrow = 2.9f,
            Gravity = -0.08f,
            Turbulence = 3.2f,
            HotCore = true,
            Motion = SmokeMotionProfile.Fireball,
            Buoyancy = 1.8f * _scale,
            SwirlStrength = 1.3f * _scale
        });

        SpawnEmitter(new EmitterConfig
        {
            Name = "P1_GroundFlash",
            Parent = transform,
            LocalPosition = Vector3.up * (0.15f * _scale),
            StartColor = new Color(1f, 0.78f, 0.25f, 0.95f),
            EndColor = new Color(0.26f, 0.22f, 0.18f, 0f),
            MinSize = 8f * _scale,
            MaxSize = 18f * _scale,
            Lifetime = 3.2f,
            MaxParticles = 220,
            Shape = ParticleSystemShapeType.Circle,
            Burst = 110,
            Upward = 2f * _scale,
            Outward = 30f * _scale,
            SizeGrow = 4.2f,
            Turbulence = 1.8f,
            HotCore = true,
            Flipbook = VfxFlipbookId.Explosion02,
            Motion = SmokeMotionProfile.GroundWave,
            SwirlStrength = 0.8f * _scale
        });

        SpawnEmitter(new EmitterConfig
        {
            Name = "P1_RisePlasma",
            Parent = _capAnchor,
            StartColor = new Color(1f, 0.82f, 0.28f, 1f),
            EndColor = new Color(0.38f, 0.32f, 0.27f, 0.72f),
            MinSize = 9f * _scale,
            MaxSize = 18f * _scale,
            Lifetime = 3.6f,
            MaxParticles = 240,
            Shape = ParticleSystemShapeType.Sphere,
            Rate = 68f,
            EmitSeconds = 2f,
            Upward = 13f * _scale,
            Outward = 4f * _scale,
            SizeGrow = 2.2f,
            Gravity = -0.05f,
            Turbulence = 2.8f,
            HotCore = true,
            Motion = SmokeMotionProfile.Stem,
            Buoyancy = 1.6f * _scale,
            SwirlStrength = 1.8f * _scale
        });
    }

    // ── Phase 2: Mushroom head / vortex cap (1.6–7.5s) ────────────────────
    private void EnterPhase2()
    {
        _phase = 2;

        SpawnEmitter(new EmitterConfig
        {
            Name = "P2_CapCore",
            Parent = _capAnchor,
            StartColor = new Color(1f, 0.7f, 0.18f, 0.95f),
            EndColor = new Color(0.14f, 0.13f, 0.12f, 0.78f),
            MinSize = 18f * _scale,
            MaxSize = 36f * _scale,
            Lifetime = 13f,
            MaxParticles = 520,
            Shape = ParticleSystemShapeType.Hemisphere,
            Rate = 42f,
            EmitSeconds = (_duration - FireballEnd) * 0.35f,
            Upward = 1.2f * _scale,
            Outward = 8f * _scale,
            SizeGrow = 2.6f,
            Turbulence = 3.8f,
            HotCore = true,
            Motion = SmokeMotionProfile.CapBillow,
            Buoyancy = 1.3f * _scale,
            RollStrength = 3.2f * _scale,
            SwirlStrength = 2.4f * _scale
        });

        // Toroidal roll — particles pushed outward then curl (cold air pressing down on rising heat).
        SpawnEmitter(new EmitterConfig
        {
            Name = "P2_CapRoll",
            Parent = _capAnchor,
            LocalPosition = Vector3.up * (1.5f * _scale),
            StartColor = new Color(0.92f, 0.5f, 0.14f, 0.9f),
            EndColor = new Color(0.12f, 0.11f, 0.1f, 0.62f),
            MinSize = 16f * _scale,
            MaxSize = 34f * _scale,
            Lifetime = 15f,
            MaxParticles = 600,
            Shape = ParticleSystemShapeType.Circle,
            ShapeRadius = 15f * _scale,
            Rate = 40f,
            EmitSeconds = (_duration - FireballEnd) * 0.4f,
            Upward = -0.8f * _scale,
            Outward = 11f * _scale,
            SizeGrow = 3f,
            Turbulence = 4.2f,
            HotCore = true,
            Motion = SmokeMotionProfile.CapRoll,
            Buoyancy = 0.45f * _scale,
            RollStrength = 5f * _scale,
            SwirlStrength = 4f * _scale
        });

        SpawnEmitter(new EmitterConfig
        {
            Name = "P2_VortexRing",
            Parent = _capAnchor,
            LocalPosition = Vector3.up * (0.8f * _scale),
            StartColor = new Color(1f, 0.48f, 0.1f, 0.85f),
            EndColor = new Color(0.18f, 0.17f, 0.16f, 0.08f),
            MinSize = 10f * _scale,
            MaxSize = 22f * _scale,
            Lifetime = 10f,
            MaxParticles = 360,
            Shape = ParticleSystemShapeType.Circle,
            ShapeRadius = 18f * _scale,
            Burst = 90,
            Rate = 20f,
            EmitSeconds = 5.5f,
            Upward = 0.5f * _scale,
            Outward = 13f * _scale,
            SizeGrow = 3.4f,
            Turbulence = 4.5f,
            HotCore = true,
            Motion = SmokeMotionProfile.CapRoll,
            RollStrength = 5.6f * _scale,
            SwirlStrength = 4.8f * _scale
        });

        // Early chimney as the fireball punches upward and begins to leave a vacuum trail.
        SpawnEmitter(new EmitterConfig
        {
            Name = "P2_EarlyStem",
            Parent = transform,
            LocalPosition = Vector3.up * (0.2f * _scale),
            StartColor = new Color(0.82f, 0.56f, 0.28f, 0.82f),
            EndColor = new Color(0.24f, 0.23f, 0.22f, 0.58f),
            MinSize = 9f * _scale,
            MaxSize = 18f * _scale,
            Lifetime = 11f,
            MaxParticles = 360,
            Shape = ParticleSystemShapeType.Circle,
            ShapeRadius = StemRadius * 1.6f,
            Rate = 42f,
            EmitSeconds = (_duration - FireballEnd) * 0.25f,
            Upward = 9f * _scale,
            Outward = 2.4f * _scale,
            SizeGrow = 2.2f,
            Turbulence = 2.6f,
            HotCore = true,
            Motion = SmokeMotionProfile.Stem,
            Buoyancy = 1.2f * _scale,
            SwirlStrength = 2.8f * _scale
        });
    }

    // ── Phase 3: Stem / chimney suction (starts ~1.6s, peaks 7.5–22s) ─────
    private void EnterPhase3()
    {
        _phase = 3;

        SpawnEmitter(new EmitterConfig
        {
            Name = "P3_FireStem",
            Parent = _stemAnchor,
            StartColor = new Color(1f, 0.62f, 0.16f, 0.95f),
            EndColor = new Color(0.25f, 0.23f, 0.21f, 0.64f),
            MinSize = 11f * _scale,
            MaxSize = 23f * _scale,
            Lifetime = 14f,
            MaxParticles = 520,
            Shape = ParticleSystemShapeType.Cone,
            ShapeRadius = StemRadius,
            ConeAngle = 6f,
            Rate = 58f,
            EmitSeconds = (_duration - HeadEnd) * 0.55f,
            Upward = 10f * _scale,
            Outward = 1.8f * _scale,
            SizeGrow = 2.2f,
            Turbulence = 3.4f,
            HotCore = true,
            Motion = SmokeMotionProfile.Stem,
            Buoyancy = 1f * _scale,
            SwirlStrength = 3.5f * _scale
        });

        SpawnEmitter(new EmitterConfig
        {
            Name = "P3_DebrisStem",
            Parent = transform,
            LocalPosition = Vector3.up * (0.3f * _scale),
            StartColor = new Color(0.58f, 0.47f, 0.34f, 0.9f),
            EndColor = new Color(0.18f, 0.18f, 0.17f, 0.72f),
            MinSize = 10f * _scale,
            MaxSize = 25f * _scale,
            Lifetime = 18f,
            MaxParticles = 620,
            Shape = ParticleSystemShapeType.Circle,
            ShapeRadius = StemRadius * 1.8f,
            Rate = 70f,
            EmitSeconds = (_duration - HeadEnd) * 0.65f,
            Upward = 10f * _scale,
            Outward = 3f * _scale,
            SizeGrow = 2f,
            Gravity = 0.03f,
            Turbulence = 3.6f,
            Flipbook = VfxFlipbookId.WispySmoke01,
            Motion = SmokeMotionProfile.Stem,
            Buoyancy = 0.8f * _scale,
            SwirlStrength = 3.2f * _scale
        });

        SpawnEmitter(new EmitterConfig
        {
            Name = "P3_DustSuction",
            Parent = transform,
            StartColor = new Color(0.64f, 0.57f, 0.46f, 0.78f),
            EndColor = new Color(0.2f, 0.2f, 0.19f, 0.48f),
            MinSize = 12f * _scale,
            MaxSize = 28f * _scale,
            Lifetime = 18f,
            MaxParticles = 520,
            Shape = ParticleSystemShapeType.Circle,
            ShapeRadius = StemRadius * 2.2f,
            Rate = 45f,
            EmitSeconds = (_duration - HeadEnd) * 0.7f,
            Upward = 7f * _scale,
            Outward = 1.6f * _scale,
            SizeGrow = 2.4f,
            Turbulence = 3f,
            Flipbook = VfxFlipbookId.WispySmoke01,
            Motion = SmokeMotionProfile.Stem,
            Buoyancy = 0.65f * _scale,
            SwirlStrength = 2.4f * _scale
        });
    }

    // ── Phase 4: Flattening at tropopause (22s – end) ─────────────────────
    private void EnterPhase4()
    {
        _phase = 4;

        SpawnEmitter(new EmitterConfig
        {
            Name = "P4_CapSpread",
            Parent = _capAnchor,
            StartColor = new Color(0.34f, 0.32f, 0.28f, 0.92f),
            EndColor = new Color(0.08f, 0.08f, 0.08f, 0.58f),
            MinSize = 28f * _scale,
            MaxSize = 52f * _scale,
            Lifetime = 28f,
            MaxParticles = 650,
            Shape = ParticleSystemShapeType.Hemisphere,
            Rate = 32f,
            EmitSeconds = (_duration - StemEnd) + 20f,
            Upward = 0.25f * _scale,
            Outward = 5f * _scale,
            SizeGrow = 2.3f,
            Turbulence = 2.4f,
            Motion = SmokeMotionProfile.Drift,
            RollStrength = 1.6f * _scale,
            SwirlStrength = 2.2f * _scale
        });

        SpawnEmitter(new EmitterConfig
        {
            Name = "P4_HorizontalDrift",
            Parent = _capAnchor,
            LocalPosition = Vector3.up * (2f * _scale),
            StartColor = new Color(0.28f, 0.26f, 0.24f, 0.82f),
            EndColor = new Color(0.07f, 0.07f, 0.07f, 0f),
            MinSize = 24f * _scale,
            MaxSize = 46f * _scale,
            Lifetime = 32f,
            MaxParticles = 520,
            Shape = ParticleSystemShapeType.Circle,
            ShapeRadius = 8f * _scale,
            Rate = 24f,
            EmitSeconds = (_duration - StemEnd) + 20f,
            Upward = -0.35f * _scale,
            Outward = 5.5f * _scale,
            SizeGrow = 2.35f,
            Gravity = 0.015f,
            Turbulence = 1.8f,
            Motion = SmokeMotionProfile.Drift,
            RollStrength = 1.4f * _scale,
            SwirlStrength = 1.8f * _scale
        });

        SpawnEmitter(new EmitterConfig
        {
            Name = "P4_StemDissipate",
            Parent = _stemAnchor,
            StartColor = new Color(0.26f, 0.24f, 0.22f, 0.78f),
            EndColor = new Color(0.08f, 0.08f, 0.08f, 0f),
            MinSize = 16f * _scale,
            MaxSize = 32f * _scale,
            Lifetime = 24f,
            MaxParticles = 360,
            Shape = ParticleSystemShapeType.Cone,
            ShapeRadius = StemRadius * 1.4f,
            ConeAngle = 10f,
            Rate = 18f,
            EmitSeconds = (_duration - StemEnd) * 0.85f,
            Upward = 4f * _scale,
            Outward = 3f * _scale,
            SizeGrow = 2.4f,
            Turbulence = 1.6f,
            Motion = SmokeMotionProfile.Stem,
            Buoyancy = 0.3f * _scale,
            SwirlStrength = 1.4f * _scale
        });
    }

    private void UpdateParticleMotion(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        var time = Time.time;
        for (var emitterIndex = 0; emitterIndex < _emitters.Count; emitterIndex++)
        {
            var runtime = _emitters[emitterIndex];
            if (runtime.System == null)
                continue;

            var count = runtime.System.GetParticles(runtime.Particles);
            if (count == 0)
                continue;

            var config = runtime.Config;
            var center = runtime.EmitterTransform != null ? runtime.EmitterTransform.position : transform.position;
            var fieldRadius = Mathf.Max(config.ShapeRadius * ShapeVisualScale, config.MaxSize * ParticleVisualScale)
                              + config.MaxSize * ParticleVisualScale * Mathf.Max(config.SizeGrow, 1f);

            for (var i = 0; i < count; i++)
            {
                var particle = runtime.Particles[i];
                var age = Mathf.Clamp01(1f - particle.remainingLifetime / Mathf.Max(particle.startLifetime, 0.01f));
                var rel = particle.position - center;
                var flat = new Vector3(rel.x, 0f, rel.z);
                var radialDistance = flat.magnitude;
                var radial = radialDistance > 0.01f ? flat / radialDistance : SeededRadial(particle.randomSeed);
                var tangent = new Vector3(-radial.z, 0f, radial.x);
                var rim = Mathf.Clamp01(radialDistance / Mathf.Max(fieldRadius, 1f));
                var curl = CurlNoise(particle.position, runtime.Seed, time);
                var acceleration = CalculateMotionAcceleration(config, age, rim, radial, tangent, curl);

                particle.velocity += acceleration * deltaTime;
                particle.velocity *= Mathf.Pow(Mathf.Lerp(0.992f, 0.965f, Mathf.Clamp01(age + config.Turbulence * 0.06f)), deltaTime * 60f);
                particle.position += new Vector3(curl.y, curl.z * 0.45f, -curl.x)
                                     * (config.Turbulence * 0.08f * TurbulenceDriftScale * deltaTime);

                if (_windDownStarted && _fadeMultiplier < 1f)
                {
                    var color = particle.startColor;
                    color.a = (byte)Mathf.Clamp(color.a * _fadeMultiplier, 0f, 255f);
                    particle.startColor = color;
                }

                runtime.Particles[i] = particle;
            }

            runtime.System.SetParticles(runtime.Particles, count);
        }
    }

    private static Vector3 CalculateMotionAcceleration(
        EmitterConfig config,
        float age,
        float rim,
        Vector3 radial,
        Vector3 tangent,
        Vector3 curl)
    {
        var pulse = SmoothPulse(age);
        var acceleration = new Vector3(curl.x, Mathf.Abs(curl.y) * 0.35f, curl.z) * config.Turbulence;

        switch (config.Motion)
        {
            case SmokeMotionProfile.Fireball:
                acceleration += radial * (config.Outward * RadialMotionScale * 0.32f * (1f - age));
                acceleration += Vector3.up * (config.Buoyancy * (1f - age * 0.45f));
                acceleration += tangent * (config.SwirlStrength * SwirlMotionScale * (0.25f + Mathf.Abs(curl.x)));
                break;

            case SmokeMotionProfile.GroundWave:
                acceleration += radial * (config.Outward * RadialMotionScale * 0.24f * (1f - age * 0.25f));
                acceleration += tangent * (config.SwirlStrength * SwirlMotionScale * curl.z);
                acceleration += Vector3.up * (config.Buoyancy * 0.12f);
                break;

            case SmokeMotionProfile.CapBillow:
                acceleration += radial * (config.Outward * RadialMotionScale * (0.12f + 0.16f * (1f - age)));
                acceleration += tangent * (config.SwirlStrength * SwirlMotionScale * (0.45f + Mathf.Abs(curl.z)) * (1f - age * 0.35f));
                acceleration += Vector3.up * (config.Buoyancy * (1f - rim * 0.45f));
                acceleration += Vector3.down * (config.RollStrength * rim * pulse * 0.42f);
                acceleration -= radial * (config.RollStrength * rim * pulse * 0.12f);
                break;

            case SmokeMotionProfile.CapRoll:
                acceleration += radial * (config.Outward * RadialMotionScale * 0.14f * (1f - age * 0.3f));
                acceleration += tangent * (config.SwirlStrength * SwirlMotionScale * (0.7f + Mathf.Abs(curl.x)) * pulse);
                acceleration += Vector3.up * (config.Buoyancy * (1f - rim));
                acceleration += Vector3.down * (config.RollStrength * (0.25f + rim) * pulse * 0.55f);
                acceleration -= radial * (config.RollStrength * rim * pulse * 0.18f);
                break;

            case SmokeMotionProfile.Stem:
                acceleration += Vector3.up * (config.Buoyancy * (1f - age * 0.3f) + config.Upward * 0.04f);
                acceleration += radial * (config.Outward * RadialMotionScale * 0.08f * (0.35f + age));
                acceleration += tangent * (config.SwirlStrength * SwirlMotionScale * (0.35f + Mathf.Abs(curl.y)) * (1f - rim * 0.35f));
                break;

            case SmokeMotionProfile.Drift:
                acceleration += radial * (config.Outward * RadialMotionScale * 0.09f * (1f - age * 0.45f));
                acceleration += tangent * (config.SwirlStrength * SwirlMotionScale * curl.x);
                acceleration += Vector3.down * (config.RollStrength * rim * 0.08f);
                break;
        }

        return acceleration;
    }

    private static Vector3 SeededRadial(uint seed)
    {
        var angle = (seed % 4096u) / 4096f * Mathf.PI * 2f;
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    private static Vector3 CurlNoise(Vector3 position, float seed, float time)
    {
        var x = (position.x + seed) * 0.012f;
        var y = (position.y - seed) * 0.012f;
        var z = (position.z + seed * 0.37f) * 0.012f;
        var drift = time * 0.045f;

        return new Vector3(
            Mathf.PerlinNoise(y + drift, z) - 0.5f,
            Mathf.PerlinNoise(z - drift, x) - 0.5f,
            Mathf.PerlinNoise(x, y + drift) - 0.5f) * 2f;
    }

    private void CreateFlashLight()
    {
        var lightObject = new GameObject("NukeLight");
        lightObject.transform.SetParent(transform, false);
        _light = lightObject.AddComponent<Light>();
        _light.type = LightType.Point;
        _light.shadows = LightShadows.None;
        _light.intensity = 52f;
        _light.range = 75f * _scale;
        _light.color = new Color(1f, 0.95f, 0.8f);
    }

    private void SpawnEmitter(EmitterConfig config)
    {
        var parent = config.Parent != null ? config.Parent : transform;
        var go = new GameObject(config.Name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = config.LocalPosition;

        var ps = go.AddComponent<ParticleSystem>();
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var flipbook = ResolveFlipbook(config);
        var flipbookMaterial = VfxFlipbookCatalog.GetMaterial(flipbook);
        var usingFlipbook = flipbookMaterial != null;
        renderer.material = usingFlipbook
            ? flipbookMaterial
            : ParticleMaterialHelper.CreateProceduralSmokeParticleMaterial(config.StartColor, config.HotCore);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 1.45f;
        renderer.sortingFudge = config.HotCore ? 0.15f : 0f;

        var main = ps.main;
        var minSize = config.MinSize * ParticleVisualScale;
        var maxSize = config.MaxSize * ParticleVisualScale;
        main.loop = config.EmitSeconds > 0f && config.Rate > 0f;
        main.playOnAwake = false;
        main.duration = Mathf.Max(config.EmitSeconds, 1f);
        main.startLifetime = config.Lifetime * _lifetimeScale;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new ParticleSystem.MinMaxGradient(config.StartColor, config.EndColor);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = config.Gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = Mathf.Max(1, Mathf.RoundToInt(config.MaxParticles * EmissionScale));

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = config.Shape;
        shape.radius = config.ShapeRadius > 0f
            ? config.ShapeRadius * ShapeVisualScale
            : Mathf.Max(minSize * 0.35f, 0.4f);
        if (config.ConeAngle > 0f)
            shape.angle = config.ConeAngle;

        var textureSheet = ps.textureSheetAnimation;
        if (usingFlipbook)
            VfxFlipbookCatalog.ApplyTextureSheetAnimation(ps, flipbook);
        else
        {
            textureSheet.enabled = true;
            textureSheet.mode = ParticleSystemAnimationMode.Grid;
            textureSheet.numTilesX = 4;
            textureSheet.numTilesY = 2;
            textureSheet.animation = ParticleSystemAnimationType.WholeSheet;
            textureSheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f);
            textureSheet.startFrame = new ParticleSystem.MinMaxCurve(0f, 1f);
        }

        if (config.Upward != 0f || config.Outward != 0f)
        {
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, config.Upward),
                new Keyframe(0.35f, config.Upward * 0.55f),
                new Keyframe(1f, config.Upward * 0.12f)));
            if (config.Outward != 0f)
            {
                velocity.x = new ParticleSystem.MinMaxCurve(-config.Outward * HorizontalVelocityScale, config.Outward * HorizontalVelocityScale);
                velocity.z = new ParticleSystem.MinMaxCurve(-config.Outward * HorizontalVelocityScale, config.Outward * HorizontalVelocityScale);
            }

            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = Mathf.Max(Mathf.Abs(config.Upward) + Mathf.Abs(config.Outward) * HorizontalVelocityScale, 12f * _scale) * 1.2f;
            limit.dampen = 0.18f;
        }

        if (config.SizeGrow > 1f)
        {
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.38f),
                new Keyframe(0.18f, 1.18f),
                new Keyframe(0.72f, config.SizeGrow),
                new Keyframe(1f, config.SizeGrow * 0.82f)));
        }

        if (config.SwirlStrength > 0f)
        {
            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(
                -0.35f * config.SwirlStrength * SpinSpeedScale,
                0.35f * config.SwirlStrength * SpinSpeedScale);
        }

        if (config.Turbulence > 0f)
        {
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = config.Turbulence;
            noise.frequency = 0.22f;
            noise.scrollSpeed = 0.14f;
            noise.damping = true;
        }

        ApplyColorGradient(ps, config.StartColor, config.EndColor, config.HotCore);

        var emission = ps.emission;
        if (config.Burst > 0)
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Min(Mathf.RoundToInt(config.Burst * EmissionScale), 600)) });
        if (config.Rate > 0f)
            emission.rateOverTime = config.Rate * EmissionScale;

        ps.Play();
        _emitters.Add(new EmitterRuntime
        {
            System = ps,
            EmitterTransform = go.transform,
            Config = config,
            Particles = new ParticleSystem.Particle[config.MaxParticles],
            Seed = UnityEngine.Random.Range(0f, 1000f)
        });
    }

    private static void ApplyColorGradient(ParticleSystem ps, Color start, Color end, bool hotCore)
    {
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        var ember = hotCore
            ? Color.Lerp(new Color(1f, 0.42f, 0.06f, start.a), start, 0.35f)
            : Color.Lerp(start, end, 0.35f);
        var coolingSmoke = Color.Lerp(start, end, 0.62f);
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(start, 0f),
                new GradientColorKey(ember, 0.18f),
                new GradientColorKey(coolingSmoke, 0.55f),
                new GradientColorKey(end, 1f)
            },
            new[]
            {
                new GradientAlphaKey(start.a, 0f),
                new GradientAlphaKey(Mathf.Max(start.a * 0.96f, end.a), 0.3f),
                new GradientAlphaKey(Mathf.Max(end.a, 0.68f), 0.58f),
                new GradientAlphaKey(Mathf.Max(end.a * 0.55f, 0.35f), 0.78f),
                new GradientAlphaKey(Mathf.Max(end.a * 0.28f, 0.12f), 0.9f),
                new GradientAlphaKey(Mathf.Max(end.a * 0.08f, 0.02f), 0.97f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float SmoothPulse(float t) => Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
    private static float SmoothFadeCurve(float t) => t * t * (3f - 2f * t);

    private sealed class EmitterRuntime
    {
        public ParticleSystem System;
        public Transform EmitterTransform;
        public EmitterConfig Config;
        public ParticleSystem.Particle[] Particles;
        public float Seed;
    }

    private enum SmokeMotionProfile
    {
        Fireball,
        GroundWave,
        CapBillow,
        CapRoll,
        Stem,
        Drift
    }

    private struct EmitterConfig
    {
        public string Name;
        public Transform Parent;
        public Vector3 LocalPosition;
        public Color StartColor;
        public Color EndColor;
        public float MinSize;
        public float MaxSize;
        public float Lifetime;
        public int MaxParticles;
        public ParticleSystemShapeType Shape;
        public float ShapeRadius;
        public float ConeAngle;
        public int Burst;
        public float Rate;
        public float EmitSeconds;
        public float Upward;
        public float Outward;
        public float SizeGrow;
        public float Gravity;
        public float Turbulence;
        public bool HotCore;
        public SmokeMotionProfile Motion;
        public float Buoyancy;
        public float RollStrength;
        public float SwirlStrength;
        public VfxFlipbookId Flipbook;
    }

    private static VfxFlipbookId ResolveFlipbook(EmitterConfig config)
    {
        if (config.Flipbook != VfxFlipbookId.None)
            return config.Flipbook;

        return config.HotCore ? VfxFlipbookId.FireBall02 : VfxFlipbookId.WispySmoke03;
    }
}

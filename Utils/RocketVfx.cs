using UnityEngine;

namespace MoreWeapons.Utils;

public sealed class RocketTrailEffect : MonoBehaviour
{
    private float _spawnTimer;
    private float _shakePhase;
    private Material _fireMaterial;
    private Material _smokeMaterial;
    private float _spawnTime;

    private void Awake()
    {
        _spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _spawnTime < Core.RocketMotorDelay)
            return;

        _shakePhase += Time.deltaTime * 21f;
        _spawnTimer += Time.deltaTime;

        var nextInterval = UnityEngine.Random.Range(0.012f, 0.028f);
        if (_spawnTimer < nextInterval)
            return;

        _spawnTimer = 0f;

        var shake = GetExhaustShake();
        var exhaust = transform.position - transform.forward * 0.22f + shake;

        SpawnFlipbookPuff(exhaust, fire: true, shake);

        if (UnityEngine.Random.value < 0.22f)
            return;

        var puffCount = UnityEngine.Random.Range(1, 4);
        for (var i = 0; i < puffCount; i++)
        {
            var back = UnityEngine.Random.Range(0.08f, 0.42f);
            var lateral = transform.right * UnityEngine.Random.Range(-0.12f, 0.12f)
                          + transform.up * UnityEngine.Random.Range(-0.08f, 0.08f);
            var puffShake = shake * UnityEngine.Random.Range(0.4f, 1.2f);
            SpawnFlipbookPuff(exhaust - transform.forward * back + lateral + puffShake, fire: false, puffShake, i);
        }
    }

    private Vector3 GetExhaustShake()
    {
        return transform.right * (Mathf.Sin(_shakePhase * 1.35f) * 0.048f)
               + transform.up * (Mathf.Cos(_shakePhase * 1.85f) * 0.036f);
    }

    private void SpawnFlipbookPuff(Vector3 position, bool fire, Vector3 shakeOffset, int seed = 0)
    {
        EnsureMaterials();
        var flipbook = fire ? VfxFlipbookId.Explosion01Light : VfxFlipbookId.Explosion01LightNoFire;
        var material = fire ? _fireMaterial : _smokeMaterial;
        if (material == null)
            return;

        var puff = new GameObject(fire ? "RocketFirePuff" : "RocketSmokePuff");
        puff.transform.position = position;
        puff.transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up)
                                  * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-18f, 18f));

        var ps = puff.AddComponent<ParticleSystem>();
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = fire ? 2.4f : 1.6f;

        var duration = fire
            ? UnityEngine.Random.Range(0.18f, 0.28f)
            : UnityEngine.Random.Range(0.45f, 0.72f);
        var size = fire
            ? UnityEngine.Random.Range(1.1f, 1.8f)
            : UnityEngine.Random.Range(0.8f, 1.4f);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = duration;
        main.startLifetime = duration;
        main.startSpeed = UnityEngine.Random.Range(0.4f, 1.4f);
        main.startSize = size;
        main.startColor = fire
            ? new Color(1f, 0.72f, 0.22f, 0.92f)
            : new Color(0.16f, 0.16f, 0.17f, 0.82f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = fire ? 3 : 4;
        main.gravityModifier = fire ? 0f : 0.08f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = shakeOffset.x * 2.5f;
        velocity.y = shakeOffset.y * 2.5f + (fire ? 0.2f : 0.05f);
        velocity.z = shakeOffset.z * 2.5f;

        var emission = ps.emission;
        emission.enabled = false;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        var startAlpha = fire ? 0.92f : 0.82f;
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(main.startColor.color, 0f),
                new GradientColorKey(main.startColor.color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(startAlpha * 0.55f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.EaseInOut(0f, 0.55f, 1f, fire ? 1.35f : 1.4f));

        VfxFlipbookCatalog.ApplyTextureSheetAnimation(ps, flipbook, fire ? 1.35f : 1f);

        var emitCount = fire ? UnityEngine.Random.Range(1, 3) : UnityEngine.Random.Range(1, 4);
        for (var i = 0; i < emitCount; i++)
        {
            var emit = new ParticleSystem.EmitParams
            {
                position = position + UnityEngine.Random.insideUnitSphere * (fire ? 0.06f : 0.12f),
                startLifetime = duration * UnityEngine.Random.Range(0.85f, 1.05f),
                startSize = size * UnityEngine.Random.Range(0.85f, 1.15f),
                startColor = main.startColor.color,
                rotation = UnityEngine.Random.Range(0f, Mathf.PI * 2f)
            };
            ps.Emit(emit, 1);
        }

        UnityEngine.Object.Destroy(puff, duration + 0.35f);
    }

    private void EnsureMaterials()
    {
        if (_fireMaterial == null)
            _fireMaterial = VfxFlipbookCatalog.GetMaterial(VfxFlipbookId.Explosion01Light);
        if (_smokeMaterial == null)
            _smokeMaterial = VfxFlipbookCatalog.GetMaterial(VfxFlipbookId.Explosion01LightNoFire);
    }
}

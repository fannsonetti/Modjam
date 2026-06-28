using UnityEngine;



namespace MoreWeapons.Utils;



public sealed class SmokeCloudEffect : MonoBehaviour

{

#if IL2CPP

    public SmokeCloudEffect(System.IntPtr ptr) : base(ptr) { }

#endif



    private float _duration;

    private float _radius;

    private float _elapsed;

    private float _fadeOutStart;

    private float _maxParticleLifetime;

    private bool _stoppedEmitting;

    private ParticleSystem _burst;

    private ParticleSystem _haze;



    public static void Spawn(Vector3 point, float duration, float radius)

    {

        var root = new GameObject("SmokeCloud");

        root.transform.position = point + Vector3.up * 0.04f;



        var effect = root.AddComponent<SmokeCloudEffect>();

        effect.Initialize(duration, radius);

    }



    private void Initialize(float duration, float radius)

    {

        _duration = duration;
        _radius = radius;

        _fadeOutStart = duration * 0.78f;

        _maxParticleLifetime = duration * 1.2f;



        var smokeColor = CreateSmokeColorGradient(0.95f, 0.98f);

        var hazeColor = CreateSmokeColorGradient(0.88f, 0.94f);

        var smokeFlipbook = VfxFlipbookId.WispySmoke01;
        var flipbookMaterial = VfxFlipbookCatalog.GetMaterial(smokeFlipbook);
        var usingFlipbook = flipbookMaterial != null;
        var material = usingFlipbook
            ? flipbookMaterial
            : ParticleMaterialHelper.CreateSoftParticleMaterial(new Color(0.94f, 0.94f, 0.94f, 0.95f));



        _burst = CreateBurstSystem("SmokeBurst", material, smokeColor, usingFlipbook ? smokeFlipbook : VfxFlipbookId.None);

        _haze = CreateHazeSystem("SmokeHaze", material, hazeColor, usingFlipbook ? smokeFlipbook : VfxFlipbookId.None);



        _burst.Emit(Mathf.RoundToInt(18f + _radius * 2.2f));

        _haze.Play();

    }



    private static ParticleSystem.MinMaxGradient CreateSmokeColorGradient(float minAlpha, float maxAlpha)
    {
        return new ParticleSystem.MinMaxGradient(
            new Color(0.70f, 0.70f, 0.70f, minAlpha),
            new Color(0.98f, 0.98f, 0.98f, maxAlpha));
    }



    private void Update()

    {

        _elapsed += Time.deltaTime;



        if (_haze != null && !_stoppedEmitting)

        {

            if (_elapsed >= _fadeOutStart)

            {

                var fadeT = Mathf.InverseLerp(_fadeOutStart, _duration, _elapsed);

                var emission = _haze.emission;

                emission.rateOverTime = Mathf.Lerp(_radius * 5f, 0f, fadeT * fadeT);

            }



            if (_elapsed >= _duration)

                StopEmitting();

        }



        if (_stoppedEmitting && _elapsed >= _duration + _maxParticleLifetime + 2f)

            Destroy(gameObject);

    }



    private void StopEmitting()

    {

        if (_stoppedEmitting)

            return;



        _stoppedEmitting = true;

        if (_haze != null)

        {

            var emission = _haze.emission;

            emission.rateOverTime = 0f;

            _haze.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        }

    }



    private ParticleSystem CreateBurstSystem(string name, Material material, ParticleSystem.MinMaxGradient color, VfxFlipbookId flipbook)

    {

        var go = new GameObject(name);

        go.transform.SetParent(transform, false);



        var ps = go.AddComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);



        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        renderer.material = material;

        renderer.renderMode = ParticleSystemRenderMode.Billboard;



        var main = ps.main;

        main.loop = false;

        main.playOnAwake = false;

        main.duration = 0.35f;

        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5.5f);

        main.startSpeed = new ParticleSystem.MinMaxCurve(_radius * 0.35f, _radius * 0.65f);

        main.startSize = new ParticleSystem.MinMaxCurve(_radius * 0.28f, _radius * 0.42f);

        main.startColor = color;

        main.gravityModifier = 0.04f;

        main.simulationSpace = ParticleSystemSimulationSpace.World;

        main.maxParticles = 512;



        var shape = ps.shape;

        shape.enabled = true;

        shape.shapeType = ParticleSystemShapeType.Sphere;

        shape.radius = _radius * 0.06f;



        var velocity = ps.velocityOverLifetime;

        velocity.enabled = true;

        velocity.y = new ParticleSystem.MinMaxCurve(_radius * 0.08f, _radius * 0.18f);



        ConfigureSmokeLifetime(ps);
        if (flipbook != VfxFlipbookId.None)
            VfxFlipbookCatalog.ApplyTextureSheetAnimation(ps, flipbook);

        var burstEmission = ps.emission;

        burstEmission.enabled = false;



        return ps;

    }



    private ParticleSystem CreateHazeSystem(string name, Material material, ParticleSystem.MinMaxGradient color, VfxFlipbookId flipbook)

    {

        var go = new GameObject(name);

        go.transform.SetParent(transform, false);



        var ps = go.AddComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);



        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        renderer.material = material;

        renderer.renderMode = ParticleSystemRenderMode.Billboard;



        var main = ps.main;

        main.loop = true;

        main.playOnAwake = false;

        main.duration = _duration;

        main.startLifetime = new ParticleSystem.MinMaxCurve(_duration * 0.8f, _duration * 1.15f);

        main.startSpeed = new ParticleSystem.MinMaxCurve(_radius * 0.12f, _radius * 0.38f);

        main.startSize = new ParticleSystem.MinMaxCurve(_radius * 0.32f, _radius * 0.52f);

        main.startColor = color;

        main.gravityModifier = 0.03f;

        main.simulationSpace = ParticleSystemSimulationSpace.World;

        main.maxParticles = 1024;



        var shape = ps.shape;

        shape.enabled = true;

        shape.shapeType = ParticleSystemShapeType.Sphere;

        shape.radius = _radius * 0.22f;



        var velocity = ps.velocityOverLifetime;

        velocity.enabled = true;

        velocity.y = new ParticleSystem.MinMaxCurve(_radius * 0.04f, _radius * 0.12f);



        var noise = ps.noise;

        noise.enabled = true;

        noise.strength = 0.42f;

        noise.frequency = 0.35f;

        noise.scrollSpeed = 0.12f;



        ConfigureSmokeLifetime(ps);
        if (flipbook != VfxFlipbookId.None)
            VfxFlipbookCatalog.ApplyTextureSheetAnimation(ps, flipbook, 0.85f);



        var emission = ps.emission;

        emission.enabled = true;

        emission.rateOverTime = _radius * 5f;



        return ps;

    }



    private static void ConfigureSmokeLifetime(ParticleSystem ps)

    {

        var sizeOverLifetime = ps.sizeOverLifetime;

        sizeOverLifetime.enabled = true;

        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.9f, 1f, 1.2f));



        var colorOverLifetime = ps.colorOverLifetime;

        colorOverLifetime.enabled = true;

        var gradient = new Gradient();

        gradient.SetKeys(

            new[]

            {

                new GradientColorKey(new Color(0.96f, 0.96f, 0.96f), 0f),
                new GradientColorKey(new Color(0.82f, 0.82f, 0.82f), 0.55f),
                new GradientColorKey(new Color(0.68f, 0.68f, 0.68f), 1f)

            },

            new[]

            {

                new GradientAlphaKey(0.95f, 0f),

                new GradientAlphaKey(0.88f, 0.45f),

                new GradientAlphaKey(0.5f, 0.82f),

                new GradientAlphaKey(0f, 1f)

            });

        colorOverLifetime.color = gradient;

    }

}



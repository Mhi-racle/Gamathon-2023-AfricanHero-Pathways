using System.Collections;
using UnityEngine;

/// <summary>
/// The gourd of wisdom. When Ananse reaches it, the gourd rises, spins and glows, then bursts
/// into confetti in the colours of the Ghanaian flag before the win panel appears.
/// </summary>
public class target : MonoBehaviour
{
    [Header("Collect animation")]
    [SerializeField] private float riseHeight = 2f;
    [SerializeField] private float riseDuration = 0.9f;
    [SerializeField] private float spinDegrees = 360f;
    [SerializeField] private float growScale = 1.3f;
    [Tooltip("How much brighter the gourd's light gets as it rises.")]
    [SerializeField] private float glowMultiplier = 4f;
    [SerializeField] private float vanishDuration = 0.15f;
    [Tooltip("Pause after the burst before the win panel opens.")]
    [SerializeField] private float winPanelDelay = 0.6f;

    [Header("Burst")]
    [Tooltip("Optional particle prefab. If empty, a confetti burst is built in code.")]
    [SerializeField] private ParticleSystem burstPrefab;
    [Tooltip("Material for the built-in confetti (Universal Render Pipeline/Particles/Unlit). " +
             "Assign it so the shader is included in builds.")]
    [SerializeField] private Material burstMaterial;
    [SerializeField] private int burstCount = 60;

    private static readonly Color GhanaRed = new Color(0.81f, 0.07f, 0.15f);
    private static readonly Color GhanaGold = new Color(0.99f, 0.82f, 0.09f);
    private static readonly Color GhanaGreen = new Color(0f, 0.42f, 0.25f);

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag(GameConstants.Tags.Player)) return;

        // Too late if the timer has already run out.
        GameManager game = GameManager.Instance;
        if (game == null || !game.IsPlaying) return;

        collected = true;
        game.TargetReached(riseDuration + vanishDuration + winPanelDelay);
        StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        // The idle animation would otherwise pin the gourd in place.
        if (TryGetComponent(out Animator animator)) animator.enabled = false;
        foreach (Collider col in GetComponentsInChildren<Collider>()) col.enabled = false;

        Light glow = GetComponentInChildren<Light>();
        float baseIntensity = glow != null ? glow.intensity : 0f;

        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.rotation;

        for (float t = 0f; t < riseDuration; t += Time.deltaTime)
        {
            float eased = EaseOutCubic(t / riseDuration);
            transform.position = startPosition + Vector3.up * (riseHeight * eased);
            transform.rotation = Quaternion.AngleAxis(spinDegrees * eased, Vector3.up) * startRotation;
            transform.localScale = startScale * Mathf.Lerp(1f, growScale, eased);
            if (glow != null) glow.intensity = baseIntensity * Mathf.Lerp(1f, glowMultiplier, eased);
            yield return null;
        }

        SpawnBurst(transform.position);

        // Pop out of existence as the confetti flies.
        Vector3 peakScale = transform.localScale;
        for (float t = 0f; t < vanishDuration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(peakScale, Vector3.zero, t / vanishDuration);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void SpawnBurst(Vector3 position)
    {
        if (burstPrefab != null)
        {
            ParticleSystem burst = Instantiate(burstPrefab, position, Quaternion.identity);
            burst.Play();
            Destroy(burst.gameObject, burst.main.duration + burst.main.startLifetime.constantMax);
            return;
        }

        Material material = burstMaterial;
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) return; // Stripped from the build: skip rather than show pink squares.
            material = new Material(shader);
        }

        BuildConfetti(position, material);
    }

    /// <summary>Creates a one-shot confetti burst that destroys itself when finished.</summary>
    private void BuildConfetti(Vector3 position, Material material)
    {
        GameObject go = new GameObject("Gourd Burst");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 0.8f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        // Fixed-mode gradient + RandomColor gives each piece exactly red, gold or green.
        Gradient flag = new Gradient { mode = GradientMode.Fixed };
        flag.SetKeys(
            new[] { new GradientColorKey(GhanaRed, 0.33f), new GradientColorKey(GhanaGold, 0.66f), new GradientColorKey(GhanaGreen, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        main.startColor = new ParticleSystem.MinMaxGradient(flag) { mode = ParticleSystemGradientMode.RandomColor };

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        // Shrink to nothing rather than fading, so an opaque material still looks right.
        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystem.RotationOverLifetimeModule spin = ps.rotationOverLifetime;
        spin.enabled = true;
        spin.z = new ParticleSystem.MinMaxCurve(-4f, 4f);

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        ps.Play();
    }

    private static float EaseOutCubic(float t)
    {
        float p = 1f - t;
        return 1f - p * p * p;
    }
}

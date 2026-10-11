using System.Collections;
using UnityEngine;

/// <summary>
/// Animates a maze block turning a quarter turn around the world Y axis.
/// Added to blocks on demand by <see cref="RotateBlock"/> and <see cref="AutoRotation"/>.
/// A block ignores new turns while one is in progress, so taps can't be spammed into
/// half-finished angles.
/// </summary>
public class BlockSpin : MonoBehaviour
{
    private const float QuarterTurn = 90f;
    private const float NudgeAngle = 8f;
    private const float NudgeDuration = 0.18f;

    private Quaternion targetRotation;

    /// <summary>True while a turn or nudge animation is playing.</summary>
    public bool IsBusy { get; private set; }

    /// <summary>Returns the block's <see cref="BlockSpin"/>, adding one the first time.</summary>
    public static BlockSpin For(GameObject block)
    {
        if (!block.TryGetComponent(out BlockSpin spin)) spin = block.AddComponent<BlockSpin>();
        return spin;
    }

    /// <summary>Starts a 90° turn. Returns false if the block is already moving.</summary>
    public bool TryTurn(float duration)
    {
        if (IsBusy) return false;

        targetRotation = Quaternion.AngleAxis(QuarterTurn, Vector3.up) * transform.rotation;
        StartCoroutine(TurnRoutine(duration));
        return true;
    }

    /// <summary>Small wobble that tells the player this block can't turn right now.</summary>
    public void Nudge()
    {
        if (IsBusy) return;

        targetRotation = transform.rotation;
        StartCoroutine(NudgeRoutine());
    }

    /// <summary>
    /// True if <paramref name="point"/> lies over this block's footprint (X/Z only).
    /// Used to stop a block turning while Ananse is standing on it, which could trap him in a wall.
    /// </summary>
    public bool Covers(Vector3 point)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length == 0) return false;

        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);

        return point.x > bounds.min.x && point.x < bounds.max.x
            && point.z > bounds.min.z && point.z < bounds.max.z;
    }

    private IEnumerator TurnRoutine(float duration)
    {
        IsBusy = true;
        Quaternion from = transform.rotation;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.rotation = Quaternion.SlerpUnclamped(from, targetRotation, EaseOutBack(t / duration));
            yield return null;
        }

        // Finish on the exact angle so repeated turns never drift.
        transform.rotation = targetRotation;
        IsBusy = false;
    }

    private IEnumerator NudgeRoutine()
    {
        IsBusy = true;
        Quaternion start = transform.rotation;

        for (float t = 0f; t < NudgeDuration; t += Time.deltaTime)
        {
            float angle = Mathf.Sin(t / NudgeDuration * Mathf.PI * 2f) * NudgeAngle;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.up) * start;
            yield return null;
        }

        transform.rotation = start;
        IsBusy = false;
    }

    private void OnDisable()
    {
        // If the block is disabled mid-animation, snap to where it was heading.
        if (!IsBusy) return;

        StopAllCoroutines();
        transform.rotation = targetRotation;
        IsBusy = false;
    }

    /// <summary>Ease-out with a slight overshoot, so the block "clicks" into place.</summary>
    private static float EaseOutBack(float t)
    {
        const float overshoot = 1.70158f;
        float p = t - 1f;
        return 1f + (overshoot + 1f) * p * p * p + overshoot * p * p;
    }
}

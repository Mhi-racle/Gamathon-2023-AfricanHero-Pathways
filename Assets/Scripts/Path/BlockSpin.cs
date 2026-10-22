using System.Collections;
using UnityEngine;

/// <summary>
/// Animates a maze block turning a quarter turn around the world Y axis.
/// Added to blocks on demand by <see cref="RotateBlock"/> and <see cref="AutoRotation"/>.
/// A block ignores new turns while one is in progress, so taps can't be spammed into
/// half-finished angles.
/// </summary>
/// <remarks>
/// Maze walls are mesh colliders, which are surfaces rather than solid volumes: if a turning wall
/// sweeps over Ananse he ends up inside it and can walk straight out through it. To prevent that,
/// Ananse rides round with the block he is standing on, and any wall that still overlaps him
/// pushes him out of the way.
/// </remarks>
public class BlockSpin : MonoBehaviour
{
    private const float QuarterTurn = 90f;
    // Penetration directions steeper than this (|y| of the unit vector) count as floor contact.
    private const float MaxFloorNormalY = 0.7f;

    private Quaternion targetRotation;
    private Collider[] colliders;

    /// <summary>True while a turn animation is playing.</summary>
    public bool IsBusy { get; private set; }

    /// <summary>Raised when any block finishes a turn, so the walk map around it can be refreshed.</summary>
    public static event System.Action<BlockSpin> TurnFinished;

    /// <summary>
    /// The ground a block can cover in any of its four orientations: a square around its pivot.
    /// Used to refresh only that part of the walk map after it turns.
    /// </summary>
    public Bounds SweptBounds()
    {
        Vector3 pivot = transform.position;
        float reach = 0f;

        foreach (Collider col in colliders)
        {
            if (col == null) continue;

            Bounds b = col.bounds;
            reach = Mathf.Max(reach,
                new Vector2(Mathf.Max(Mathf.Abs(b.min.x - pivot.x), Mathf.Abs(b.max.x - pivot.x)),
                            Mathf.Max(Mathf.Abs(b.min.z - pivot.z), Mathf.Abs(b.max.z - pivot.z))).magnitude);
        }

        return new Bounds(new Vector3(pivot.x, 0f, pivot.z), new Vector3(reach * 2f, 0f, reach * 2f));
    }

    /// <summary>Returns the block's <see cref="BlockSpin"/>, adding one the first time.</summary>
    public static BlockSpin For(GameObject block)
    {
        if (!block.TryGetComponent(out BlockSpin spin)) spin = block.AddComponent<BlockSpin>();
        return spin;
    }

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    /// <summary>Starts a 90° turn. Returns false if the block is already moving.</summary>
    public bool TryTurn(float duration)
    {
        if (IsBusy) return false;

        targetRotation = Quaternion.AngleAxis(QuarterTurn, Vector3.up) * transform.rotation;
        StartCoroutine(TurnRoutine(duration));
        return true;
    }

    private IEnumerator TurnRoutine(float duration)
    {
        IsBusy = true;
        Quaternion from = transform.rotation;
        Quaternion previous = from;
        Transform player = PlayerTransform();
        bool carryPlayer = player != null && IsOverFootprint(player.position);

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.rotation = Quaternion.SlerpUnclamped(from, targetRotation, EaseOutBack(t / duration));
            FollowTurn(player, carryPlayer, previous);
            previous = transform.rotation;
            yield return null;
        }

        // Finish on the exact angle so repeated turns never drift.
        transform.rotation = targetRotation;
        FollowTurn(player, carryPlayer, previous);
        IsBusy = false;
        TurnFinished?.Invoke(this);
    }

    /// <summary>
    /// Moves Ananse to match this frame of the turn: carried round if he is riding the block,
    /// then nudged out of any of the block's walls that overlap him.
    /// </summary>
    private void FollowTurn(Transform player, bool carryPlayer, Quaternion previousRotation)
    {
        if (player == null) return;

        if (carryPlayer)
        {
            // The turn is about the block's pivot, so apply the same rotation step to Ananse.
            Quaternion step = transform.rotation * Quaternion.Inverse(previousRotation);
            Vector3 pivot = transform.position;
            player.SetPositionAndRotation(pivot + step * (player.position - pivot), step * player.rotation);
        }

        PushOutOfWalls(player);

        // The CharacterController must see the teleport, or its next Move() would undo it.
        Physics.SyncTransforms();
    }

    private void PushOutOfWalls(Transform player)
    {
        if (!player.TryGetComponent(out CapsuleCollider body)) return;

        foreach (Collider wall in colliders)
        {
            if (wall == null || wall.isTrigger || !wall.enabled) continue;

            if (Physics.ComputePenetration(body, player.position, player.rotation,
                                           wall, wall.transform.position, wall.transform.rotation,
                                           out Vector3 direction, out float distance))
            {
                // Mostly-vertical overlaps are Ananse touching a floor slab, not a wall.
                if (Mathf.Abs(direction.y) > MaxFloorNormalY) continue;

                // Push sideways only; Ananse always stays on the maze floor.
                player.position += new Vector3(direction.x, 0f, direction.z) * distance;
            }
        }
    }

    /// <summary>True if <paramref name="point"/> lies over this block's footprint (X/Z only).</summary>
    private bool IsOverFootprint(Vector3 point)
    {
        if (colliders.Length == 0) return false;

        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);

        return point.x > bounds.min.x && point.x < bounds.max.x
            && point.z > bounds.min.z && point.z < bounds.max.z;
    }

    private static Transform PlayerTransform()
    {
        GameManager game = GameManager.Instance;
        return game != null && game.player != null ? game.player.transform : null;
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

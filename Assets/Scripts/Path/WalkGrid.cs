using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A map of where Ananse can stand, used to find his route to the gourd. The maze area is divided into small
/// cells, and each cell is tested with a capsule the size of Ananse's own body: if the capsule
/// fits without touching a wall, the cell is walkable. Because the test uses his real shape,
/// the map always agrees with what his CharacterController can actually walk through.
/// After a block turns, only the cells it can cover are tested again.
/// </summary>
/// <remarks>
/// Unity's NavMesh was tried first, but at this maze scale (corridors well under half a metre)
/// its voxelised map kept disagreeing with the real walls.
/// </remarks>
public class WalkGrid : MonoBehaviour
{
    public static WalkGrid Instance { get; private set; }

    /// <summary>Raised after the map is first built and after every refresh.</summary>
    public static event System.Action Rebuilt;

    private const float BoundsPadding = 0.75f;

    // The test capsule starts this far above the floor, so the ground itself never counts as a wall.
    private const float FloorClearance = 0.05f;

    // Slightly slimmer than Ananse, so corridors he can squeeze through stay connected.
    private const float RadiusFactor = 0.9f;
    private const float MinCellSize = 0.03f;
    private const int MaxCells = 200000;
    private const int OverlapBufferSize = 16;
    private const int NearestSearchCells = 8;

    private static readonly Vector2Int[] Neighbours =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    private readonly Collider[] overlaps = new Collider[OverlapBufferSize];
    private readonly Queue<int> frontier = new Queue<int>();
    private readonly List<int> cellPath = new List<int>();

    private Transform player;
    private float radius;
    private float capsuleTop;
    private float cellSize;
    private Vector3 origin;
    private int width;
    private int depth;
    private bool[] free;
    private int[] cameFrom;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager game = GameManager.Instance;
        if (game == null || game.player == null) return;

        player = game.player.transform;
        MeasurePlayer();
        LayOutGrid();
        RebuildArea(new Bounds(origin, Vector3.zero), everything: true);
        LogSummary();

        BlockSpin.TurnFinished += OnBlockTurned;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogSummary()
    {
        int walkable = 0;
        foreach (bool cell in free) if (cell) walkable++;

        Debug.Log($"WalkGrid: {walkable} of {free.Length} cells walkable ({100f * walkable / free.Length:F0}%), " +
                  $"cell {cellSize:F3} m, Ananse radius {radius:F3} m. " +
                  (IsWalkable(player.position) ? "Ananse's spot is walkable." : "Ananse's spot tests as blocked."));
    }

    private void OnDestroy()
    {
        BlockSpin.TurnFinished -= OnBlockTurned;
        if (Instance == this) Instance = null;
    }

    /// <summary>True if Ananse would fit standing at <paramref name="point"/>.</summary>
    public bool IsWalkable(Vector3 point)
    {
        return free != null && TryGetCell(point, out int cell) && free[cell];
    }

    /// <summary>
    /// Finds a route from <paramref name="from"/> towards <paramref name="to"/> around the walls and
    /// fills <paramref name="waypoints"/> with the points to walk through. If <paramref name="to"/>
    /// can't be reached, the route ends at the reachable spot closest to it.
    /// Returns false if there is nowhere to go.
    /// </summary>
    public bool TryFindPath(Vector3 from, Vector3 to, List<Vector3> waypoints)
    {
        return TryFindPath(from, to, waypoints, out _);
    }

    /// <summary>
    /// As <see cref="TryFindPath(Vector3, Vector3, List{Vector3})"/>, and also reports whether the
    /// route actually reaches <paramref name="to"/> (rather than stopping at the closest reachable spot).
    /// </summary>
    public bool TryFindPath(Vector3 from, Vector3 to, List<Vector3> waypoints, out bool reachesTarget)
    {
        waypoints.Clear();
        reachesTarget = false;
        if (free == null || !TryGetNearestFreeCell(from, out int start)) return false;

        // The target itself may test as blocked (e.g. the gourd sits by a wall), so aim for the nearest free spot.
        int goal = TryGetNearestFreeCell(to, out int goalCell) ? goalCell : -1;
        int end = SearchFrom(start, goal, to);
        reachesTarget = goal >= 0 && end == goal;
        if (end == start) return false;

        BuildCellPath(start, end);
        SmoothIntoWaypoints(waypoints);
        return waypoints.Count > 0;
    }

    private void OnBlockTurned(BlockSpin block)
    {
        Bounds area = block.SweptBounds();
        area.Expand(new Vector3(radius * 2f, 0f, radius * 2f));
        RebuildArea(area, everything: false);
    }

    private void MeasurePlayer()
    {
        float height = 0.55f;
        radius = 0.15f;

        if (player.TryGetComponent(out CharacterController body))
        {
            Vector3 scale = player.lossyScale;
            radius = body.radius * Mathf.Max(scale.x, scale.z);
            height = body.height * scale.y;
        }

        radius *= RadiusFactor;
        capsuleTop = Mathf.Max(FloorClearance + radius * 2f, height);
        cellSize = Mathf.Max(MinCellSize, radius * 0.5f);
    }

    private void LayOutGrid()
    {
        Bounds area = MazeBounds();

        // Keep the grid a sensible size on unusually large levels.
        while ((area.size.x / cellSize + 1f) * (area.size.z / cellSize + 1f) > MaxCells) cellSize *= 1.25f;

        width = Mathf.CeilToInt(area.size.x / cellSize) + 1;
        depth = Mathf.CeilToInt(area.size.z / cellSize) + 1;
        origin = new Vector3(area.min.x, 0f, area.min.z);
        free = new bool[width * depth];
        cameFrom = new int[width * depth];
    }

    /// <summary>Re-tests the cells inside <paramref name="area"/> (or every cell) against the walls.</summary>
    private void RebuildArea(Bounds area, bool everything)
    {
        if (free == null) return;

        // Make sure physics sees the blocks where they are now.
        Physics.SyncTransforms();

        int minX = 0, minZ = 0, maxX = width - 1, maxZ = depth - 1;
        if (!everything)
        {
            minX = Mathf.Max(0, Mathf.FloorToInt((area.min.x - origin.x) / cellSize));
            minZ = Mathf.Max(0, Mathf.FloorToInt((area.min.z - origin.z) / cellSize));
            maxX = Mathf.Min(width - 1, Mathf.CeilToInt((area.max.x - origin.x) / cellSize));
            maxZ = Mathf.Min(depth - 1, Mathf.CeilToInt((area.max.z - origin.z) / cellSize));
        }

        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                free[z * width + x] = BodyFitsAt(CellCentre(x, z));
            }
        }

        Rebuilt?.Invoke();
    }

    private bool BodyFitsAt(Vector3 point)
    {
        Vector3 bottom = new Vector3(point.x, FloorClearance + radius, point.z);
        Vector3 top = new Vector3(point.x, Mathf.Max(bottom.y, capsuleTop - radius), point.z);

        int hits = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, overlaps,
                                                  Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits; i++)
        {
            // Ananse's own colliders don't count.
            if (!overlaps[i].transform.IsChildOf(player)) return false;
        }

        return true;
    }

    /// <summary>
    /// Breadth-first search outwards from <paramref name="start"/>. Returns <paramref name="goal"/>
    /// if reached, otherwise the reached cell closest to <paramref name="target"/>.
    /// </summary>
    private int SearchFrom(int start, int goal, Vector3 target)
    {
        for (int i = 0; i < cameFrom.Length; i++) cameFrom[i] = -1;

        frontier.Clear();
        frontier.Enqueue(start);
        cameFrom[start] = start;

        int closest = start;
        float closestDistance = DistanceSq(start, target);

        while (frontier.Count > 0)
        {
            int cell = frontier.Dequeue();
            if (cell == goal) return goal;

            float distance = DistanceSq(cell, target);
            if (distance < closestDistance)
            {
                closest = cell;
                closestDistance = distance;
            }

            int cx = cell % width;
            int cz = cell / width;
            foreach (Vector2Int step in Neighbours)
            {
                int nx = cx + step.x;
                int nz = cz + step.y;
                if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;

                int next = nz * width + nx;
                if (!free[next] || cameFrom[next] != -1) continue;

                // No cutting diagonally past the corner of a wall.
                if (step.x != 0 && step.y != 0 && (!free[cz * width + nx] || !free[nz * width + cx])) continue;

                cameFrom[next] = cell;
                frontier.Enqueue(next);
            }
        }

        return closest;
    }

    private void BuildCellPath(int start, int end)
    {
        cellPath.Clear();
        for (int cell = end; cell != start; cell = cameFrom[cell]) cellPath.Add(cell);
        cellPath.Add(start);
        cellPath.Reverse();
    }

    /// <summary>Skips cells that can be walked past in a straight line, leaving only the turns.</summary>
    private void SmoothIntoWaypoints(List<Vector3> waypoints)
    {
        int last = cellPath.Count - 1;
        int anchor = 0;

        while (anchor < last)
        {
            int furthest = last;
            while (furthest > anchor + 1 && !StraightLineIsFree(cellPath[anchor], cellPath[furthest])) furthest--;

            waypoints.Add(CellCentre(cellPath[furthest]));
            anchor = furthest;
        }
    }

    private bool StraightLineIsFree(int fromCell, int toCell)
    {
        Vector3 a = CellCentre(fromCell);
        Vector3 b = CellCentre(toCell);
        int samples = Mathf.CeilToInt(Vector3.Distance(a, b) / (cellSize * 0.5f));

        for (int i = 1; i < samples; i++)
        {
            if (!IsWalkable(Vector3.Lerp(a, b, i / (float)samples))) return false;
        }

        return true;
    }

    private bool TryGetNearestFreeCell(Vector3 point, out int cell)
    {
        if (TryGetCell(point, out cell) && free[cell]) return true;

        // Ananse may be pressed against a wall, so his own cell can test as blocked: look nearby.
        int px = Mathf.RoundToInt((point.x - origin.x) / cellSize);
        int pz = Mathf.RoundToInt((point.z - origin.z) / cellSize);
        float best = float.MaxValue;
        cell = -1;

        for (int dz = -NearestSearchCells; dz <= NearestSearchCells; dz++)
        {
            for (int dx = -NearestSearchCells; dx <= NearestSearchCells; dx++)
            {
                int x = px + dx;
                int z = pz + dz;
                if (x < 0 || z < 0 || x >= width || z >= depth || !free[z * width + x]) continue;

                float d = dx * dx + dz * dz;
                if (d < best)
                {
                    best = d;
                    cell = z * width + x;
                }
            }
        }

        return cell >= 0;
    }

    private bool TryGetCell(Vector3 point, out int cell)
    {
        int x = Mathf.RoundToInt((point.x - origin.x) / cellSize);
        int z = Mathf.RoundToInt((point.z - origin.z) / cellSize);
        bool inside = x >= 0 && z >= 0 && x < width && z < depth;
        cell = inside ? z * width + x : -1;
        return inside;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Scene view overlay while playing: green where Ananse fits, red where he doesn't.
    /// Only shown when Gizmos are enabled in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || free == null) return;

        Vector3 size = new Vector3(cellSize * 0.9f, 0.005f, cellSize * 0.9f);
        Color walkable = new Color(0.1f, 0.9f, 0.2f, 0.35f);
        Color blocked = new Color(1f, 0.15f, 0.1f, 0.35f);

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Gizmos.color = free[z * width + x] ? walkable : blocked;
                Gizmos.DrawCube(CellCentre(x, z) + Vector3.up * 0.02f, size);
            }
        }
    }
#endif

    private Vector3 CellCentre(int x, int z) => origin + new Vector3(x * cellSize, 0f, z * cellSize);

    private Vector3 CellCentre(int cell) => CellCentre(cell % width, cell / width);

    private float DistanceSq(int cell, Vector3 target)
    {
        Vector3 d = CellCentre(cell) - target;
        d.y = 0f;
        return d.sqrMagnitude;
    }

    /// <summary>The area covering every block, Ananse and the gourd, plus some padding.</summary>
    private Bounds MazeBounds()
    {
        Bounds area = new Bounds(player.position, Vector3.zero);

        foreach (GameObject block in GameObject.FindGameObjectsWithTag(GameConstants.Tags.Block))
        {
            foreach (Collider col in block.GetComponentsInChildren<Collider>())
            {
                area.Encapsulate(col.bounds);
            }
        }

        foreach (target gourd in FindObjectsByType<target>(FindObjectsSortMode.None))
        {
            area.Encapsulate(gourd.transform.position);
        }

        area.Expand(new Vector3(BoundsPadding * 2f, 0f, BoundsPadding * 2f));
        return area;
    }
}

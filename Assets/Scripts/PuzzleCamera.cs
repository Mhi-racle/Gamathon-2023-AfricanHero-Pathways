using Cinemachine;
using UnityEngine;

/// <summary>
/// Keeps both Ananse and the gourd in view, so the player can always see the route they are
/// building, even in levels where the gourd starts off-screen. At level start it points the
/// scene's Cinemachine camera at a target group of the two, with a Group Composer that pulls back
/// when they are far apart and moves in as Ananse gets closer.
/// </summary>
/// <remarks>
/// Added to every level automatically by <see cref="GameManager"/>; add it to the Game Manager
/// object yourself to tune a single level. Works with the camera's existing Body settings.
/// </remarks>
public class PuzzleCamera : MonoBehaviour
{
    [Tooltip("Pull of the gourd on the shot's centre compared with Ananse (1 = halfway between them).")]
    [SerializeField] private float gourdWeight = 1f;

    [Tooltip("How much space each target needs around it, in metres.")]
    [SerializeField] private float targetRadius = 0.4f;

    [Tooltip("Fraction of the screen the two should fill (smaller = more room around them).")]
    [Range(0.2f, 1f)]
    [SerializeField] private float framingSize = 0.5f;

    [Tooltip("Closest and furthest the camera may move to keep both in frame.")]
    [SerializeField] private float minDistance = 2.5f;
    [SerializeField] private float maxDistance = 15f;

    [Tooltip("Narrowest and widest the camera may zoom once it can't move any further.")]
    [SerializeField] private float minFieldOfView = 25f;
    [SerializeField] private float maxFieldOfView = 60f;

    private void Start()
    {
        GameManager game = GameManager.Instance;
        target gourd = FindAnyObjectByType<target>();
        CinemachineVirtualCamera vcam = FindAnyObjectByType<CinemachineVirtualCamera>();
        if (game == null || game.player == null || gourd == null || vcam == null) return;

        group = new GameObject("Ananse and Gourd (camera targets)").AddComponent<CinemachineTargetGroup>();
        group.AddMember(game.player.transform, 1f, targetRadius);
        // The gourd model bobs in its idle animation; frame its still parent so the camera doesn't bob too.
        Transform gourdAnchor = gourd.transform.parent != null ? gourd.transform.parent : gourd.transform;
        group.AddMember(gourdAnchor, gourdWeight, targetRadius);

        vcam.Follow = group.transform;
        vcam.LookAt = group.transform;

        // Replaces the camera's Aim with one that frames the whole group.
        composer = vcam.AddCinemachineComponent<CinemachineGroupComposer>();
        composer.m_FramingMode = CinemachineGroupComposer.FramingMode.HorizontalAndVertical;
        composer.m_AdjustmentMode = CinemachineGroupComposer.AdjustmentMode.DollyThenZoom;
        ApplySettings();
    }

    private CinemachineTargetGroup group;
    private CinemachineGroupComposer composer;

    /// <summary>Pushes the Inspector values to the camera and target group.</summary>
    private void ApplySettings()
    {
        if (composer != null)
        {
            composer.m_GroupFramingSize = framingSize;
            composer.m_MinimumDistance = minDistance;
            composer.m_MaximumDistance = Mathf.Max(minDistance, maxDistance);
            composer.m_MinimumFOV = minFieldOfView;
            composer.m_MaximumFOV = Mathf.Max(minFieldOfView, maxFieldOfView);
        }

        if (group != null && group.m_Targets.Length >= 2)
        {
            group.m_Targets[0].radius = targetRadius;
            group.m_Targets[1].radius = targetRadius;
            group.m_Targets[1].weight = gourdWeight;
        }
    }

#if UNITY_EDITOR
    // Editor only: changing a value in the Inspector during Play updates the camera straight away,
    // so a level's framing can be tuned live. (Play-mode changes are still lost when you stop.)
    private void OnValidate()
    {
        if (Application.isPlaying) ApplySettings();
    }
#endif
}

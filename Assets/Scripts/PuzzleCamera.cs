using Cinemachine;
using UnityEngine;

/// <summary>How the puzzle camera keeps Ananse and the gourd in view.</summary>
public enum PuzzleCameraStyle
{
    /// <summary>The camera never rotates: it keeps one viewing angle and slides and pulls back to frame both.</summary>
    FixedAngle,

    /// <summary>The camera stays put and turns to look at them (with an optional dead zone and smoothing).</summary>
    LookAtTargets
}

/// <summary>
/// Keeps both Ananse and the gourd in view, so the player can always see the route they are
/// building, even in levels where the gourd starts off-screen. At level start it points the
/// scene's Cinemachine camera at a target group of the two and frames the group, pulling back
/// when they are far apart and moving in as Ananse gets closer.
/// </summary>
/// <remarks>
/// Added to every level automatically by <see cref="GameManager"/>; add it to the Game Manager
/// object yourself to tune a single level. In the Editor, values changed during Play apply
/// immediately, so framing can be tuned live.
/// </remarks>
public class PuzzleCamera : MonoBehaviour
{
    [Tooltip("Fixed Angle: never rotates, just slides and pulls back (calm, readable). " +
             "Look At Targets: turns to follow them.")]
    [SerializeField] private PuzzleCameraStyle style = PuzzleCameraStyle.FixedAngle;

    [Header("Angle (Fixed Angle)")]
    [Tooltip("Keep the Virtual Camera's angle from the scene. Untick to set it with Tilt and Heading below.")]
    [SerializeField] private bool useSceneAngle = true;

    [Tooltip("Degrees the camera looks down: 90 is straight down, 45 is a diagonal view.")]
    [Range(10f, 90f)]
    [SerializeField] private float tilt = 60f;

    [Tooltip("Compass direction the camera faces, in degrees.")]
    [SerializeField] private float heading = 0f;

    [Header("Movement")]
    [Tooltip("How smoothly the camera catches up: 0 follows instantly, higher is softer.")]
    [Range(0f, 5f)]
    [SerializeField] private float smoothing = 0.5f;

    [Tooltip("Look At Targets only: how far (fraction of the screen) the targets may move before the camera turns.")]
    [Range(0f, 0.9f)]
    [SerializeField] private float turnDeadZone = 0.2f;

    [Header("Framing")]
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

    [Tooltip("Narrowest and widest the camera may zoom once it can't move any further. " +
             "Set both the same to stop any zooming.")]
    [SerializeField] private float minFieldOfView = 25f;
    [SerializeField] private float maxFieldOfView = 60f;

    private CinemachineVirtualCamera vcam;
    private CinemachineTargetGroup group;
    private CinemachineGroupComposer composer;
    private CinemachineFramingTransposer framer;

    private void Start()
    {
        GameManager game = GameManager.Instance;
        target gourd = FindAnyObjectByType<target>();
        vcam = FindAnyObjectByType<CinemachineVirtualCamera>();
        if (game == null || game.player == null || gourd == null || vcam == null) return;

        group = new GameObject("Ananse and Gourd (camera targets)").AddComponent<CinemachineTargetGroup>();
        group.AddMember(game.player.transform, 1f, targetRadius);
        // The gourd model bobs in its idle animation; frame its still parent so the camera doesn't bob too.
        Transform gourdAnchor = gourd.transform.parent != null ? gourd.transform.parent : gourd.transform;
        group.AddMember(gourdAnchor, gourdWeight, targetRadius);

        vcam.Follow = group.transform;
        vcam.LookAt = group.transform;

        if (style == PuzzleCameraStyle.FixedAngle)
        {
            // Body frames the group from a fixed direction; with no Aim the camera never rotates.
            framer = vcam.AddCinemachineComponent<CinemachineFramingTransposer>();
            vcam.DestroyCinemachineComponent<CinemachineComposer>();
            framer.m_GroupFramingMode = CinemachineFramingTransposer.FramingMode.HorizontalAndVertical;
            framer.m_AdjustmentMode = CinemachineFramingTransposer.AdjustmentMode.DollyThenZoom;
            framer.m_CameraDistance = minDistance;
        }
        else
        {
            // Aim turns the camera to frame the group; the existing Body keeps it following.
            composer = vcam.AddCinemachineComponent<CinemachineGroupComposer>();
            composer.m_FramingMode = CinemachineGroupComposer.FramingMode.HorizontalAndVertical;
            composer.m_AdjustmentMode = CinemachineGroupComposer.AdjustmentMode.DollyThenZoom;
        }

        ApplySettings();
    }

    /// <summary>Pushes the Inspector values to the camera and target group.</summary>
    private void ApplySettings()
    {
        float maxDist = Mathf.Max(minDistance, maxDistance);
        float maxFov = Mathf.Max(minFieldOfView, maxFieldOfView);

        if (framer != null)
        {
            if (!useSceneAngle) vcam.transform.rotation = Quaternion.Euler(tilt, heading, 0f);

            framer.m_GroupFramingSize = framingSize;
            framer.m_MinimumDistance = minDistance;
            framer.m_MaximumDistance = maxDist;
            framer.m_MinimumFOV = minFieldOfView;
            framer.m_MaximumFOV = maxFov;
            framer.m_XDamping = smoothing;
            framer.m_YDamping = smoothing;
            framer.m_ZDamping = smoothing;
        }

        if (composer != null)
        {
            composer.m_GroupFramingSize = framingSize;
            composer.m_MinimumDistance = minDistance;
            composer.m_MaximumDistance = maxDist;
            composer.m_MinimumFOV = minFieldOfView;
            composer.m_MaximumFOV = maxFov;
            composer.m_HorizontalDamping = smoothing;
            composer.m_VerticalDamping = smoothing;
            composer.m_DeadZoneWidth = turnDeadZone;
            composer.m_DeadZoneHeight = turnDeadZone;
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
    // Switching Style needs a restart of Play to take effect.
    private void OnValidate()
    {
        if (Application.isPlaying) ApplySettings();
    }
#endif
}

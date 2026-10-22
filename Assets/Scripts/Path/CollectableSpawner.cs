using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The guide line to the gourd. It stays hidden while the maze still blocks the way, and appears
/// in gold, tracing the route through the blocks, the moment the player's turns open a clear path
/// (with a light vibration on Android). That is also when Ananse starts walking by himself.
/// </summary>
public class CollectableSpawner : MonoBehaviour
{
    [Tooltip("The gourd the line leads to.")]
    [SerializeField]
    private GameObject Prefab;

    [SerializeField]
    private Transform Player;

    [SerializeField]
    private LineRenderer Path;

    [SerializeField]
    private float pathHeightOffset = 1.25f;

    [SerializeField]
    private float PathUpdateSpeed = 0.25f;

    [Tooltip("Line colour while a clear path to the gourd exists.")]
    [SerializeField] private Color connectedColor = new Color(1f, 0.82f, 0.2f);

    private readonly List<Vector3> route = new List<Vector3>();
    private bool connected;

    private void Start()
    {
        if (Prefab == null || Player == null || Path == null) return;

        Path.startColor = connectedColor;
        Path.endColor = connectedColor;
        Path.positionCount = 0;
        StartCoroutine(DrawPathToGourd());
    }

    private IEnumerator DrawPathToGourd()
    {
        WaitForSeconds wait = new WaitForSeconds(PathUpdateSpeed);

        // Stop once the gourd is collected (it rises, then is destroyed).
        while (Prefab != null)
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Won) break;

            bool clear = WalkGrid.Instance != null
                      && WalkGrid.Instance.TryFindPath(Player.position, Prefab.transform.position, route, out bool reaches)
                      && reaches;

            if (clear) DrawRoute();
            else Path.positionCount = 0;

            // A small "click" when the player's turns first open the way.
            if (clear && !connected) Haptics.Tap();
            connected = clear;

            yield return wait;
        }

        Path.positionCount = 0;
    }

    private void DrawRoute()
    {
        Vector3 lift = Vector3.up * pathHeightOffset;
        Path.positionCount = route.Count + 1;
        Path.SetPosition(0, new Vector3(Player.position.x, 0f, Player.position.z) + lift);

        for (int i = 0; i < route.Count; i++)
        {
            Path.SetPosition(i + 1, route[i] + lift);
        }
    }
}

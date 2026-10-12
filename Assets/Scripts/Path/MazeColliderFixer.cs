using System.Text;
using UnityEngine;

/// <summary>
/// Safety net for maze pieces that were tagged "Block" but never given a collider in the scene
/// (Ananse could walk through them and they couldn't be tapped). At level start, every mesh
/// under a Block that has no collider gets a MeshCollider using its own mesh, so the
/// collision shape matches exactly what the player sees.
/// </summary>
public static class MazeColliderFixer
{
    // Thinner than this (in metres, vertically) is treated as floor and left walkable.
    private const float MinWallHeight = 0.2f;

    /// <summary>Adds missing colliders to Block pieces. Returns how many were added.</summary>
    public static int AddMissingBlockColliders()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag(GameConstants.Tags.Block);
        StringBuilder fixedNames = null;
        int added = 0;

        foreach (GameObject block in blocks)
        {
            // Blocks that already collide anywhere in their hierarchy are left exactly as authored.
            if (block.GetComponentInChildren<Collider>(true) != null) continue;

            foreach (MeshFilter meshFilter in block.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh == null || meshFilter.TryGetComponent(out Collider _)) continue;
                if (mesh.bounds.size.y * meshFilter.transform.lossyScale.y < MinWallHeight) continue;

                // Builds can only cook colliders from meshes with Read/Write enabled on import.
                if (!mesh.isReadable && !Application.isEditor)
                {
                    Debug.LogWarning($"Block '{meshFilter.name}' has no collider and its mesh isn't readable. " +
                                     "Enable Read/Write on the level model's import settings.");
                    continue;
                }

                meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
                added++;
                (fixedNames ??= new StringBuilder()).Append(meshFilter.name).Append(", ");
            }
        }

        if (added > 0)
        {
            Debug.Log($"MazeColliderFixer: added {added} missing collider(s) to: {fixedNames}");
        }

        return added;
    }
}

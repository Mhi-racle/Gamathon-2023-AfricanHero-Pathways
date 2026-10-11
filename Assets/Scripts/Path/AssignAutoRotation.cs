using UnityEngine;

/// <summary>
/// Makes a fixed number of randomly chosen blocks rotate by themselves, so each attempt
/// feels a little different while the difficulty stays the same.
/// </summary>
public class AssignAutoRotation : MonoBehaviour
{
    public GameObject[] blocks;

    [Tooltip("How many of the blocks above should auto-rotate.")]
    [SerializeField] private int autoRotatingCount = 1;

    private void Start()
    {
        if (blocks == null || blocks.Length == 0) return;

        // Partial Fisher-Yates shuffle: the first N entries end up as N distinct random blocks.
        GameObject[] pool = (GameObject[])blocks.Clone();
        int count = Mathf.Min(autoRotatingCount, pool.Length);

        for (int i = 0; i < count; i++)
        {
            int pick = Random.Range(i, pool.Length);
            (pool[i], pool[pick]) = (pool[pick], pool[i]);

            if (pool[i] != null && !pool[i].TryGetComponent(out AutoRotation _))
            {
                pool[i].AddComponent<AutoRotation>();
            }
        }
    }
}

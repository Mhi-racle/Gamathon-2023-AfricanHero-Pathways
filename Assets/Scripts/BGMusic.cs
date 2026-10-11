using UnityEngine;

/// <summary>
/// Menu background music that keeps playing across scene loads. Only the first instance survives.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BGMusic : MonoBehaviour
{
    public static BGMusic instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Looping on the AudioSource replaces the old per-frame "restart if stopped" check.
        AudioSource source = GetComponent<AudioSource>();
        source.loop = true;
        if (!source.isPlaying) source.Play();
    }
}

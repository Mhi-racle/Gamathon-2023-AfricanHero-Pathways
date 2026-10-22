using UnityEngine;

/// <summary>
/// Short vibrations for Android. Unity's Handheld.Vibrate() buzzes for around half a second,
/// which is far too long for a tap, so this calls Android's Vibrator service directly.
/// Does nothing on other platforms or if the device has no vibrator.
/// </summary>
public static class Haptics
{
    /// <summary>A light tick, e.g. when a block turns.</summary>
    public const long TapMilliseconds = 20;

    /// <summary>A firmer pulse, e.g. when the gourd is collected.</summary>
    public const long RewardMilliseconds = 60;

    // Never true. The reference to Handheld.Vibrate below is what makes Unity add the
    // VIBRATE permission to the Android manifest.
#pragma warning disable 0649 // Deliberately never assigned.
    private static bool addVibratePermission;
#pragma warning restore 0649

#if UNITY_ANDROID && !UNITY_EDITOR
    private const int OreoApiLevel = 26;
    private const int DefaultAmplitude = -1;

    private static AndroidJavaObject vibrator;
    private static bool initialised;
    private static int apiLevel;
#endif

    public static void Tap() => Vibrate(TapMilliseconds);

    public static void Reward() => Vibrate(RewardMilliseconds);

    public static void Vibrate(long milliseconds)
    {
        if (addVibratePermission) Handheld.Vibrate();

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (!initialised) Initialise();
            if (vibrator == null) return;

            if (apiLevel >= OreoApiLevel)
            {
                using (AndroidJavaClass effects = new AndroidJavaClass("android.os.VibrationEffect"))
                using (AndroidJavaObject effect = effects.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, DefaultAmplitude))
                {
                    vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }
        catch (System.Exception e)
        {
            // Haptics are a nice extra; never let them break gameplay.
            Debug.LogWarning($"Haptics unavailable: {e.Message}");
            vibrator = null;
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void Initialise()
    {
        initialised = true;

        using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            apiLevel = version.GetStatic<int>("SDK_INT");
        }

        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }

        if (vibrator != null && !vibrator.Call<bool>("hasVibrator")) vibrator = null;
    }
#endif
}

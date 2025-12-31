using UnityEngine;

/// <summary>
/// Forces the game to run in landscape mode at runtime.
/// This ensures landscape orientation even if device settings change.
/// </summary>
public class ScreenOrientationController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Force landscape orientation on start")]
    [SerializeField] private bool forceLandscapeOnStart = true;
    
    [Tooltip("Preferred landscape orientation")]
    [SerializeField] private ScreenOrientation landscapeOrientation = ScreenOrientation.LandscapeLeft;

    private void Start()
    {
        if (forceLandscapeOnStart)
        {
            SetLandscapeOrientation();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Re-apply landscape when app regains focus (in case user changed device orientation)
        if (hasFocus && forceLandscapeOnStart)
        {
            SetLandscapeOrientation();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Re-apply landscape when app resumes
        if (!pauseStatus && forceLandscapeOnStart)
        {
            SetLandscapeOnResume();
        }
    }

    /// <summary>
    /// Sets the screen orientation to landscape
    /// </summary>
    public void SetLandscapeOrientation()
    {
        Screen.orientation = landscapeOrientation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
    }

    /// <summary>
    /// Sets landscape orientation with a small delay (useful when resuming)
    /// </summary>
    private void SetLandscapeOnResume()
    {
        // Small delay to ensure proper orientation after resume
        Invoke(nameof(SetLandscapeOrientation), 0.1f);
    }
}


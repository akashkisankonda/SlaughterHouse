using UnityEngine;

public class GoatBlood : MonoBehaviour
{
    /// <summary>
    /// Enables the specified number of inactive child objects.
    /// </summary>
    /// <param name="count">The number of inactive child objects to enable.</param>
    ///
    public static GoatBlood instance;
    public Transform canvasBloodSplats;

    private void Awake()
    {
        instance = this;
    }
    public void EnableInactiveChildren(int count)
    {
        int enabledCount = 0;

        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
                enabledCount++;

                // Stop when the required number of objects have been enabled
                if (enabledCount >= count)
                    return;
            }
        }
        foreach (Transform child in canvasBloodSplats)
        {
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
                enabledCount++;

                // Stop when the required number of objects have been enabled
                if (enabledCount >= count)
                    return;
            }
        }

        if (enabledCount < count)
        {
            Debug.LogWarning($"Only {enabledCount} inactive child object(s) were available to enable.");
        }
    }

    /// <summary>
    /// Disables all currently active child objects.
    /// </summary>
    public void DisableAllActiveChildren()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }

        foreach (Transform child in canvasBloodSplats)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}

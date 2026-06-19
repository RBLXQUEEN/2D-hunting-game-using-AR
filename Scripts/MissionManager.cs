using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Checks the player's GPS position against a list of MissionZones every 5 seconds.
/// When the player enters a zone, spawns AR characters and shows a mission notification.
/// Battery-efficient: all checks run in a coroutine, nothing happens in Update.
/// </summary>
public class MissionManager : MonoBehaviour
{
    [Header("Mission Data")]
    [Tooltip("All geofenced mission zones. Configure each zone's GPS coords, radius, and prefabs here.")]
    public List<MissionZone> allZones = new List<MissionZone>();

    [Header("UI")]
    [Tooltip("TextMeshPro element that shows mission entry notifications. Will be auto-hidden after a delay.")]
    public TextMeshProUGUI missionNotificationText;

    [Header("AR Reference")]
    [Tooltip("Reference to the scene's ARPlaneManager for realistic surface spawning.")]
    public ARPlaneManager arPlaneManager;

    [Tooltip("How long (seconds) the notification text stays visible after entering a zone.")]
    public float notificationDuration = 4f;

    // Maps each active MissionZone to the list of GameObjects spawned for it.
    private Dictionary<MissionZone, List<GameObject>> _activeSpawns =
        new Dictionary<MissionZone, List<GameObject>>();

    // Zones the player is currently inside (prevents repeated triggering).
    private HashSet<MissionZone> _activeMissions = new HashSet<MissionZone>();

    void Start()
    {
        // Hide notification UI at startup.
        if (missionNotificationText != null)
            missionNotificationText.gameObject.SetActive(false);

        StartCoroutine(WaitForGPSThenBeginChecks());
    }

    // -------------------------------------------------------------------------
    // Initialisation coroutine
    // -------------------------------------------------------------------------

    /// <summary>
    /// Polls until GPSManager is ready, then starts the zone-check loop.
    /// Avoids wasting work while GPS is initialising.
    /// </summary>
    IEnumerator WaitForGPSThenBeginChecks()
    {
        Debug.Log("MissionManager: Waiting for GPS to become ready...");

        while (GPSManager.Instance == null || !GPSManager.Instance.isGPSReady)
            yield return new WaitForSeconds(1f);

        Debug.Log("MissionManager: GPS ready. Zone checks starting.");
        StartCoroutine(ZoneCheckLoop());
    }

    // -------------------------------------------------------------------------
    // Zone checking
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs every 5 seconds. Checks the player's current GPS position against
    /// every uncompleted MissionZone and triggers/clears missions accordingly.
    /// </summary>
    IEnumerator ZoneCheckLoop()
    {
        while (true)
        {
            EvaluateAllZones();
            yield return new WaitForSeconds(5f);
        }
    }

    /// <summary>
    /// Iterates every zone: enters if the player stepped inside, notes the exit
    /// but keeps spawns alive until the mission is explicitly completed.
    /// </summary>
    void EvaluateAllZones()
    {
        float playerLat = GPSManager.Instance.currentLat;
        float playerLon = GPSManager.Instance.currentLon;

        foreach (MissionZone zone in allZones)
        {
            // Skip zones the player already finished.
            if (zone.isCompleted) continue;

            float distance = GetDistanceInMeters(playerLat, playerLon, zone.latitude, zone.longitude);
            bool insideZone = distance <= zone.radiusInMeters;

            if (insideZone && !_activeMissions.Contains(zone))
            {
                TriggerMission(zone);
            }
            // If player leaves the radius, we leave the mission active so characters
            // remain until OnMissionComplete is called (e.g. all characters hunted).
        }
    }

    // -------------------------------------------------------------------------
    // Haversine formula
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the great-circle distance in meters between two GPS coordinates
    /// using the Haversine formula.
    /// </summary>
    public float GetDistanceInMeters(float lat1, float lon1, float lat2, float lon2)
    {
        const float EarthRadiusM = 6371000f;

        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);

        float a = Mathf.Sin(dLat * 0.5f) * Mathf.Sin(dLat * 0.5f)
                + Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2)
                * Mathf.Sin(dLon * 0.5f) * Mathf.Sin(dLon * 0.5f);

        float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f - a));
        return EarthRadiusM * c;
    }

    // -------------------------------------------------------------------------
    // Mission lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Marks a zone as active, spawns its characters in AR space, and shows the
    /// mission notification. Safe to call multiple times — won't retrigger if
    /// the zone is already active.
    /// </summary>
    void TriggerMission(MissionZone zone)
    {
        _activeMissions.Add(zone);
        Debug.Log($"MissionManager: Entered zone '{zone.missionName}' — triggering mission!");

        List<GameObject> spawned = new List<GameObject>();
        _activeSpawns[zone] = spawned;

        if (zone.charactersToSpawn != null)
        {
            foreach (GameObject prefab in zone.charactersToSpawn)
            {
                if (prefab == null) continue;

                Vector3 spawnPos = ChooseARSpawnPosition();
                GameObject character = Instantiate(prefab, spawnPos, Quaternion.identity);
                spawned.Add(character);
            }
        }

        ShowNotification(zone.missionName);
    }

    /// <summary>
    /// Call this once the player has completed the mission (e.g. hunted all
    /// spawned characters). Marks the zone completed and cleans up all spawns.
    /// </summary>
    public void OnMissionComplete(MissionZone zone)
    {
        zone.isCompleted = true;
        _activeMissions.Remove(zone);
        Debug.Log($"MissionManager: Mission '{zone.missionName}' completed!");

        if (_activeSpawns.TryGetValue(zone, out List<GameObject> characters))
        {
            foreach (GameObject go in characters)
            {
                if (go != null) Destroy(go);
            }
            _activeSpawns.Remove(zone);
        }

        ShowNotification($"{zone.missionName} — COMPLETE!");
    }

    // -------------------------------------------------------------------------
    // AR spawn position helper
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a world-space position for spawning a character.
    /// Prefers an AR-detected horizontal plane; falls back to a point in front
    /// of the camera when no planes are available.
    /// </summary>
    Vector3 ChooseARSpawnPosition()
    {
        if (arPlaneManager != null)
        {
            foreach (ARPlane plane in arPlaneManager.trackables)
            {
                if (plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    Vector2 offset = Random.insideUnitCircle * 1.5f;
                    // Place character just above the detected floor plane.
                    return plane.center + new Vector3(offset.x, 0.5f, offset.y);
                }
            }
        }

        // Fallback: 3 metres ahead of the camera at roughly floor level.
        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();
        return cam.position + forward * 3f + Vector3.down * 0.8f;
    }

    // -------------------------------------------------------------------------
    // UI helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Activates the notification text with the given message, then hides it
    /// after notificationDuration seconds.
    /// </summary>
    void ShowNotification(string message)
    {
        if (missionNotificationText == null) return;

        StopCoroutine(nameof(HideNotificationAfterDelay)); // cancel any in-flight hide
        missionNotificationText.gameObject.SetActive(true);
        missionNotificationText.text = $"Mission: {message}";
        StartCoroutine(HideNotificationAfterDelay());
    }

    /// <summary>
    /// Waits for notificationDuration seconds then hides the notification panel.
    /// </summary>
    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        if (missionNotificationText != null)
            missionNotificationText.gameObject.SetActive(false);
    }
}

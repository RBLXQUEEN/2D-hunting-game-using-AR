using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance;

    public float currentLat      { get; private set; }
    public float currentLon      { get; private set; }
    public float currentAccuracy { get; private set; } = float.MaxValue;
    public bool  isGPSReady      { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start() => StartCoroutine(RequestThenStart());

    // ── Permission flow ───────────────────────────────────────────────────────

    IEnumerator RequestThenStart()
    {
#if UNITY_ANDROID
        yield return StartCoroutine(EnsurePermissions());
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.LogWarning("GPSManager: location permission denied");
            yield break;
        }
#endif
        yield return StartCoroutine(StartLocationServices());
    }

#if UNITY_ANDROID
    IEnumerator EnsurePermissions()
    {
        var needed = new List<string>();
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            needed.Add(Permission.FineLocation);
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
            needed.Add(Permission.CoarseLocation);
        if (needed.Count == 0) yield break;

        int pending = needed.Count;
        var cb = new PermissionCallbacks();
        cb.PermissionGranted              += _ => pending--;
        cb.PermissionDenied               += _ => pending--;
        cb.PermissionDeniedAndDontAskAgain += _ => pending--;
        Permission.RequestUserPermissions(needed.ToArray(), cb);
        yield return new WaitUntil(() => pending <= 0);
    }
#endif

    // ── GPS start ─────────────────────────────────────────────────────────────

    IEnumerator StartLocationServices()
    {
        // desiredAccuracyInMeters=1, updateDistanceInMeters=0.5
        // Tells Android to use every available sensor (GPS + WiFi + cell).
        Input.location.Start(1f, 0.5f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Running)
        {
            isGPSReady = true;
            Debug.Log("GPSManager: GPS service running");
        }
        else
        {
            Debug.LogWarning($"GPSManager: GPS service {(maxWait <= 0 ? "timed out" : "failed")} — will rely on network location");
        }

        StartCoroutine(UpdateLoop());
    }

    // ── Main update loop ──────────────────────────────────────────────────────

    IEnumerator UpdateLoop()
    {
        while (true)
        {
            // 1. GPS (works outdoors, poor indoors)
            if (Input.location.status == LocationServiceStatus.Running)
            {
                var d = Input.location.lastData;
                TryApplyFix(d.latitude, d.longitude, d.horizontalAccuracy, "GPS");
            }

#if UNITY_ANDROID
            // 2. Network provider  – WiFi access points + cell towers (works indoors)
            TryNetworkFix("network");

            // 3. Fused provider    – Android's own GPS+WiFi+sensor blend
            TryNetworkFix("fused");
#endif
            yield return new WaitForSeconds(2f);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void TryApplyFix(float lat, float lon, float accuracy, string source)
    {
        if (accuracy < currentAccuracy)
        {
            currentLat      = lat;
            currentLon      = lon;
            currentAccuracy = accuracy;
            isGPSReady      = true;
            Debug.Log($"[{source}] LAT:{lat:F6}  LON:{lon:F6}  ACC:{accuracy:F1}m");
        }
    }

#if UNITY_ANDROID
    void TryNetworkFix(string provider)
    {
        try
        {
            using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var lm       = activity.Call<AndroidJavaObject>("getSystemService", "location");
            if (lm == null) return;

            using var loc = lm.Call<AndroidJavaObject>("getLastKnownLocation", provider);
            if (loc == null) return;

            long  ageMs    = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                             - loc.Call<long>("getTime");
            float accuracy = loc.Call<float>("getAccuracy");
            float lat      = (float)loc.Call<double>("getLatitude");
            float lon      = (float)loc.Call<double>("getLongitude");

            // Discard stale fixes (older than 30 s)
            if (ageMs < 30_000)
                TryApplyFix(lat, lon, accuracy, provider);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"GPSManager [{provider}]: {e.Message}");
        }
    }
#endif

    void OnDestroy() => Input.location.Stop();
}

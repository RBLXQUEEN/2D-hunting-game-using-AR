using System.Collections;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance;

    public float currentLat { get; private set; }
    public float currentLon { get; private set; }
    public bool isGPSReady { get; private set; } = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        StartCoroutine(StartGPS());
    }

    IEnumerator StartGPS() {
        if (!Input.location.isEnabledByUser) {
            Debug.Log("GPS not enabled by user");
            yield break;
        }

        Input.location.Start(10f, 5f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0) {
            Debug.Log("GPS timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed) {
            Debug.Log("GPS failed");
            yield break;
        }

        isGPSReady = true;
        Debug.Log("GPS Ready!");
        StartCoroutine(UpdateLocation());
    }

    IEnumerator UpdateLocation() {
        while (true) {
            if (Input.location.status == LocationServiceStatus.Running) {
                currentLat = Input.location.lastData.latitude;
                currentLon = Input.location.lastData.longitude;
                Debug.Log($"LAT: {currentLat} | LON: {currentLon}");
            }
            yield return new WaitForSeconds(5f);
        }
    }

    void OnDestroy() {
        Input.location.Stop();
    }
}
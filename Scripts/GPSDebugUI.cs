using UnityEngine;
using TMPro;

public class GPSDebugUI : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void Update() {
        if (GPSManager.Instance == null) {
            debugText.text = "GPSManager not found!";
            return;
        }

        if (GPSManager.Instance.isGPSReady) {
            debugText.text = $"GPS: Ready ✓\n" +
                             $"LAT: {GPSManager.Instance.currentLat:F6}\n" +
                             $"LON: {GPSManager.Instance.currentLon:F6}";
        } else {
            debugText.text = "Waiting for GPS...";
        }
    }
}
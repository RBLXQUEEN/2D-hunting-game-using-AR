using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MenuManager : MonoBehaviour
{
    [Header("AR References")]
    public ARSession arSession;
    public GameObject xrOrigin;

    [Header("Canvases")]
    public GameObject mainMenuCanvas;
    public GameObject instructionsCanvas;
    public GameObject missionsCanvas;   // we'll build this next
    public GameObject scoreCanvas;      // shown during actual AR hunting

    void Start() {
        // Disable AR completely at launch
        SetARActive(false);

        // Show only main menu
        ShowOnly(mainMenuCanvas);
    }

    void SetARActive(bool active) {
        arSession.enabled = active;
        xrOrigin.SetActive(active);
    }

    void ShowOnly(GameObject target) {
        mainMenuCanvas.SetActive(target == mainMenuCanvas);
        instructionsCanvas.SetActive(target == instructionsCanvas);
        if (missionsCanvas != null) missionsCanvas.SetActive(target == missionsCanvas);
        scoreCanvas.SetActive(target == scoreCanvas);
    }

    // Hook this to your "Start" button
    public void OnStartPressed() {
        SetARActive(true);
        ShowOnly(scoreCanvas);
    }

    // Hook this to your "Instructions" button
    public void OnInstructionsPressed() {
        ShowOnly(instructionsCanvas);
    }

    // Hook this to your "See Missions" button
    public void OnMissionsPressed() {
        ShowOnly(missionsCanvas);
    }

    // Hook this to "Back" buttons on Instructions/Missions screens
    public void OnBackToMenu() {
        SetARActive(false); // make sure AR turns off if backing out from gameplay
        ShowOnly(mainMenuCanvas);
    }
}   
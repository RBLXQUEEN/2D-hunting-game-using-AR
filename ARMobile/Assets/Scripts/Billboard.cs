using UnityEngine;

public class Billboard : MonoBehaviour {
    void Update() {
        if (Camera.main != null) {
            transform.LookAt(Camera.main.transform);
            // Keep him upright so he doesn't tilt forward
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }
}
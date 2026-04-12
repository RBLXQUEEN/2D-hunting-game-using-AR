using UnityEngine;

public class Billboard : MonoBehaviour {
    void Update() {
        if (Camera.main != null) {
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }
}

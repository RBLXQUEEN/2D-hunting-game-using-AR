using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TargetManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject boyPrefab; 
    public ARPlaneManager planeManager; 

    public GameManager gameManager; 

    [Header("Spawn Settings")]
    [Tooltip("How far from the center point the boy can spawn")]
    public float spawnRadius = 3.0f;
    [Tooltip("How high above the floor the boy should spawn (prevents being buried)")]
    public float heightOffset = 2f;

    [Header("Hardware Fallback")]
    [Tooltip("If no planes are found, spawn anyway after this many seconds")]
    public float fallbackTimer = 3.0f;
    private float timer = 0f;

    private GameObject spawnedBoy;
    private bool isSpawning = false;

    void Update()
    {
        // if game is active
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        //AUTO-SPAWN
        if (spawnedBoy == null && !isSpawning && GameManager.Instance.score<5)
        {
            //a real AR floor
            bool foundARPlane = TryAutoSpawn();
            if (!foundARPlane)
            {
                timer += Time.deltaTime;
                if (timer >= fallbackTimer)
                {
                    SpawnFallbackBoy();
                    timer = 0;
                }
            }
            else
            {
                timer = 0;
            }
        }
        bool isTapping = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) 
                         || Input.GetMouseButtonDown(0);

        if (isTapping)
        {
            Vector3 inputPos = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(inputPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Animal"))
                {
                    HandleHunt(hit.transform.gameObject);
                }
            }
        }
    }
    bool TryAutoSpawn()
    {
        if (planeManager == null || planeManager.trackables.count == 0) return false;

        List<ARPlane> horizontalPlanes = new List<ARPlane>();
        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp || plane.alignment == PlaneAlignment.HorizontalDown)
            {
                horizontalPlanes.Add(plane);
            }
        }

        if (horizontalPlanes.Count > 0)
        {
            isSpawning = true;
            ARPlane randomFloor = horizontalPlanes[Random.Range(0, horizontalPlanes.Count)];
            Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
            //floor center + random offset + height offset
            Vector3 spawnPos = randomFloor.center + new Vector3(randomPoint.x, heightOffset, randomPoint.y);
            
            spawnedBoy = Instantiate(boyPrefab, spawnPos, Quaternion.identity);
            Invoke("ResetSpawnLock", 1.5f);
            return true;
        }
        return false;
    }
    void SpawnFallbackBoy()
    {
        isSpawning = true;
        Debug.Log("AR Plane detection slow. Using Fallback Spawn.");
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        
        Vector3 fallbackPos = Camera.main.transform.position + (Camera.main.transform.forward * 4f);
        fallbackPos.x += randomCircle.x;
        fallbackPos.z += randomCircle.y;
        fallbackPos.y -= 1.2f;

        spawnedBoy = Instantiate(boyPrefab, fallbackPos, Quaternion.identity);
        Invoke("ResetSpawnLock", 1.5f);
    }

    void ResetSpawnLock()
    {
        isSpawning = false;
    }

    void HandleHunt(GameObject target)
    {
        BoxCollider bc = target.GetComponent<BoxCollider>();
        if (bc != null) bc.enabled = false;
        Animator anim = target.GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Death");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore();
        }
        spawnedBoy = null; 
        Destroy(target, 0.6f); 
    }
}

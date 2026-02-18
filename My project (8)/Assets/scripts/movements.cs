using UnityEngine;
using UnityEngine.InputSystem;

public class movements : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float minX = -6f;
    [SerializeField] float maxX = 6f;

    [Header("Shooting")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float laserSpeed = 15f;
    [SerializeField] float fireRate = 0.3f;

    float nextFireTime;
    Vector2 rawInput;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(rawInput.x * moveSpeed, 0f);

        float clampedX = Mathf.Clamp(rb.position.x, minX, maxX);
        rb.position = new Vector2(clampedX, rb.position.y);
    }

    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
    }

    void OnFire()
    {
        if (Time.time < nextFireTime)
            return;

        if (GameManager.instance == null)
            return;

        if (!GameManager.instance.UseShot())
            return;

        nextFireTime = Time.time + fireRate;

        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
        laserRb.linearVelocity = Vector2.up * laserSpeed;
    }
}

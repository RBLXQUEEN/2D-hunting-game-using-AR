using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("Ground Spawn Area")]
    [SerializeField] float minX = -7f;
    [SerializeField] float maxX = 7f;
    [SerializeField] float groundY = -1.2f;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Prey"))
        {
            if (GameManager.instance != null)
                GameManager.instance.AddScore();

            float randomX = Random.Range(minX, maxX);
            collision.transform.position = new Vector2(randomX, groundY);

            Destroy(gameObject);
        }
    }
}

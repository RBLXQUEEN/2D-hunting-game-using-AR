using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;
    private int score = 0;

    [Header("Shots")]
    [SerializeField] TextMeshProUGUI shotsText;
    [SerializeField] int maxShots = 10;
    private int currentShots;

    [Header("Game Over UI")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TextMeshProUGUI finalScoreText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentShots = maxShots;
        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void AddScore()
    {
        score++;
        UpdateUI();
    }

    public bool UseShot()
    {
        if (currentShots <= 0)
            return false;

        currentShots--;
        UpdateUI();

        if (currentShots == 0)
            StartCoroutine(GameOverDelay());

        return true;
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + score;

        if (shotsText != null)
            shotsText.text = "Shots : " + currentShots;
    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(1f);
        GameOver();
    }

    void GameOver()
    {
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score : " + score;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

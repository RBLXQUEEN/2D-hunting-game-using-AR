using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject instructionPanel;
    public GameObject finishPanel;
    public GameObject Score;

    [Header("Game Logic")]
    public TextMeshProUGUI scoreText;
    public int score = 0;
    public bool isGameActive = false;

    void Awake() { Instance = this; }

    void Start() {
        ShowStart();
        Score.SetActive(false);

    }

    public void ShowStart() {
        Time.timeScale = 0; 
        startPanel.SetActive(true);
        instructionPanel.SetActive(false);
        finishPanel.SetActive(false);
    }

    public void ShowInstructions() {
        startPanel.SetActive(false);
        instructionPanel.SetActive(true);
    }

    public void StartGame() {
        instructionPanel.SetActive(false);
        Score.SetActive(true); 
        Time.timeScale = 1; 
        isGameActive = true;
    }

    public void AddScore() {
        score++;
        scoreText.text = "Score: " + score;
        if (score >= 5) {
            Invoke("EndGame",2f);
        }
    }

    void EndGame() {
        isGameActive = false;
        Score.SetActive(false); // Optional: Hide score when finished
        finishPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
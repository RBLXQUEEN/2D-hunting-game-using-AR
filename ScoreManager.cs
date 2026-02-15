using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ScoreText;
    
    int score=0;

    public void AddScore(int additionalscore)
    {
        score += additionalscore;
        ScoreText.text="Score: "+ score.ToString();
        
    }
    
    
}

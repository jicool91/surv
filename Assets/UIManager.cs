using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text scoreText;
    private int score = 0;

    public void AddScore(int points)
    {
        score += points;
        scoreText.text = "Score: " + score;
    }
}

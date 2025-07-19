using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text gearsText;
    public TMP_Text timerText;
    public TMP_Text livesText;
    public TMP_Text enemiesText;

    public GameObject gameOverPanel;
    public TMP_Text gameOverScoreText;

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void UpdateGears(int gears)
    {
        gearsText.text = $"Gears: {gears}";
    }

    public void UpdateTimer(float time)
    {
        timerText.text = $"Time: {Mathf.CeilToInt(time)}";
    }

    public void UpdateLives(int lives)
    {
        livesText.text = $"Lives: {lives}";
    }

    public void UpdateEnemiesCount(int count)
    {
        enemiesText.text = $"Enemies: {count}";
    }

    public void ShowGameOver(int score)
    {
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = $"Game Over\nScore: {score}";
    }
}

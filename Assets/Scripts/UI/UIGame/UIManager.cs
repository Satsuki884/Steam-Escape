using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gearsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text bombCountText;
    [SerializeField] private TMP_Text bombRangeText;
    [SerializeField] private TMP_Text speedText;

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

    public void ShowGameOver(int score)
    {
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = $"Game Over\nScore: {score}";
    }

    public void UpdateBombCount(int count)
    {
        bombCountText.text = $"Bombs: {count}";
    }

    public void UpdateBombRange(int range)
    {
        bombRangeText.text = $"Range: {range}";
    }

    public void UpdateSpeed(float speed)
    {
        speedText.text = $"Speed: {speed}";
    }

}

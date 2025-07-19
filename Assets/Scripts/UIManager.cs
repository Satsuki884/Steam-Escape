using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gearsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text enemiesText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text bombCountText;
    [SerializeField] private TMP_Text bombRangeText;
    [SerializeField] private TMP_Text speedText;

    public void UpdateScore(int score)
    {
        scoreText.text = $"Sc: {score}";
    }

    public void UpdateGears(int gears)
    {
        gearsText.text = $"G: {gears}";
    }

    public void UpdateTimer(float time)
    {
        timerText.text = $"Time: {Mathf.CeilToInt(time)}";
    }

    public void UpdateLives(int lives)
    {
        livesText.text = $"L: {lives}";
    }

    public void UpdateEnemiesCount(int count)
    {
        enemiesText.text = $"E: {count}";
    }

    public void ShowGameOver(int score)
    {
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = $"Game Over\nScore: {score}";
    }

    public void UpdateBombCount(int count)
    {
        bombCountText.text = $"B: {count}";
    }

    public void UpdateBombRange(int range)
    {
        bombRangeText.text = $"R: {range}";
    }

    public void UpdateSpeed(float speed)
    {
        speedText.text = $"Sp: {speed}";
    }

}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelsManager : MonoBehaviour
{
    public Button pauseButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button mainMenuButton;
    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartPauseButton;
    public Button mainMenuPauseButton;

    void Start()
    {
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pauseButton.onClick.AddListener(TogglePausePanel);
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        restartPauseButton.onClick.AddListener(RestartGame);
        mainMenuPauseButton.onClick.AddListener(GoToMainMenu);
    }

    public void TogglePausePanel()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
        Time.timeScale = pausePanel.activeSelf ? 0 : 1;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        // SceneManager.LoadScene("Menu");
        Debug.Log("Main Menu button clicked. Implement scene loading here.");
    }

}

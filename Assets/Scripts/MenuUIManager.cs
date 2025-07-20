using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public Button playButton;
    public Button rulesButton;
    public Button shopButton;
    public Button leaderboardButton;
    public Button infoButton;
    public Button settingsButton;
    public Button exitButton;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        rulesButton.onClick.AddListener(OnRulesClicked);
        shopButton.onClick.AddListener(OnShopClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        infoButton.onClick.AddListener(OnInfoClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    void OnPlayClicked()
    {
        SceneManager.LoadScene("Game");
    }

    void OnRulesClicked()
    {
        Debug.Log("Rules button clicked");
        // Show rules UI
    }

    void OnShopClicked()
    {
        Debug.Log("Shop button clicked");
        // Open shop UI
    }

    void OnLeaderboardClicked()
    {
        Debug.Log("Leaderboard button clicked");
        // Show leaderboard UI
    }

    void OnInfoClicked()
    {
        Debug.Log("Info button clicked");
        // Show info/about UI
    }

    void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        // Open settings UI
    }

    void OnExitClicked()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }
}
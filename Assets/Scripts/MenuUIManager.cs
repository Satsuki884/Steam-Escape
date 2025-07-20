using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button rulesButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Button closeInfo;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private WebViewObject webViewObject;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        rulesButton.onClick.AddListener(OnRulesClicked);
        shopButton.onClick.AddListener(OnShopClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        infoButton.onClick.AddListener(OnInfoClicked);
        closeInfo.onClick.AddListener(OnInfoCloseClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);



        webViewObject = FindObjectOfType<WebViewObject>();

        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
        }
        else
        {
            Debug.LogWarning("WebViewObject не найден в сцене");
        }
        infoPanel.SetActive(false);
        
    }

    void OnPlayClicked()
    {
        SceneManager.LoadScene("Game");
    }

    void OnInfoClicked()
    {
        infoPanel.SetActive(true);

        if (webViewObject != null)
        {
            webViewObject.SetVisibility(true);
        }
    }

    void OnInfoCloseClicked()
    {
        infoPanel.SetActive(false);

        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
        }
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
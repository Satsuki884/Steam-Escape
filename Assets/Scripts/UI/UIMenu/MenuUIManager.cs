using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button rulesButton;
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private Button closeRulesButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeShopButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Button closeLeaderboardButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Button closeInfo;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button exitButton;

    private WebViewObject webViewObject;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        rulesButton.onClick.AddListener(() => rulesPanel.SetActive(true));
        closeRulesButton.onClick.AddListener(() => rulesPanel.SetActive(false));
        shopButton.onClick.AddListener(() => shopPanel.SetActive(true));
        closeShopButton.onClick.AddListener(() => shopPanel.SetActive(false));
        leaderboardButton.onClick.AddListener(() => leaderboardPanel.SetActive(true));
        closeLeaderboardButton.onClick.AddListener(() => leaderboardPanel.SetActive(false));
        infoButton.onClick.AddListener(OnInfoClicked);
        closeInfo.onClick.AddListener(OnInfoCloseClicked);
        settingsButton.onClick.AddListener(() => settingsPanel.SetActive(true));
        closeSettingsButton.onClick.AddListener(() => settingsPanel.SetActive(false));
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
        rulesPanel.SetActive(false);
        shopPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
        settingsPanel.SetActive(false);
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

    void OnExitClicked()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }
}
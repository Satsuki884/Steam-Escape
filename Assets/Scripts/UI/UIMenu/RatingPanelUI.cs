using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class RatingPanelUI : MonoBehaviour
{
    public TMP_Text topScoresText;
    public TMP_Text playerRankText;
    private LeaderboardManager leaderboardManager;

    private void Start()
    {
        leaderboardManager = LeaderboardManager.Instance;
        leaderboardManager.InitializePlayerAccount();
        Show();
    }

    public void Show()
    {
        topScoresText.text = "Loading...";

        // if (string.IsNullOrEmpty(PlayerPrefs.GetString("player_name", "")))
        // {
        //     playerRankText.text = "- - -";
        // }
        // else
        // {
            playerRankText.text = "Loading...";
            StartCoroutine(leaderboardManager.GetCurrentPlayerRank(playerRankText));
        // }

        StartCoroutine(GetTop10Scores());
    }
    private IEnumerator GetTop10Scores()
    {
        yield return leaderboardManager.GetTopScores(10, topScoresText);
    }
}

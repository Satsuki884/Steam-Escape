using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public int gears = 0;

    public int enemiesCount;

    public float gameTime = 240f; // 3 минуты
    private float timer;

    private LevelGenerator mapGen;

    private UIManager uiManager;

    private bool allEnemiesKilled = false;

    private LeaderboardManager leaderboardManager;
    private SaveData save;

    void Start()
    {
        mapGen = FindObjectOfType<LevelGenerator>();
        uiManager = FindObjectOfType<UIManager>();
        save = FindObjectOfType<SaveData>();
        leaderboardManager = FindObjectOfType<LeaderboardManager>();

        timer = gameTime;

        CountEnemies();
        UpdateUI();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        uiManager.UpdateTimer(timer);

        if (timer <= 0)
        {
            GameOver();
        }

        UpdateBombStatsUI();
    }

    void CountEnemies()
    {
        enemiesCount = FindObjectsOfType<IEnemy>().Length;
    }

    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }

    public void AddGears(int amount)
    {
        gears += amount;
        uiManager.UpdateGears(gears);
    }

    public void CollectBonus(BonusType bonus)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;
        AudioManager.Instance.PlaySFX("bonus");

        switch (bonus)
        {
            case BonusType.Speed:
                player.AddSpeedBonus(1f);
                uiManager.UpdateSpeed(player.moveSpeed);
                break;
            case BonusType.Range:
                player.bombRange += 1;
                UpdateBombStatsUI();
                break;
            case BonusType.Health:
                player.CurrentLives = Mathf.Min(player.CurrentLives + 1, player.maxLives);
                uiManager.UpdateLives(player.CurrentLives);
                break;
            case BonusType.Bombs:
                player.maxActiveBombs += 1;
                UpdateBombStatsUI();
                break;
        }
    }

    public void OnEnemyKilled(IEnemy enemy)
    {
        enemiesCount--;

        if (enemiesCount <= 0)
        {
            allEnemiesKilled = true;
            HighlightDestructiblesWithBonusesAndExit();
        }
    }

    void HighlightDestructiblesWithBonusesAndExit()
    {
        foreach (var kvp in mapGen.HiddenObjects)
        {
            Vector2Int pos = kvp.Key;

            if (mapGen.IsDestructible(pos))
            {
                var block = mapGen.MapTiles[pos.x, pos.y];
                if (block != null)
                {
                    var flasher = block.GetComponent<BlockFlasher>();
                    if (flasher == null)
                    {
                        flasher = block.AddComponent<BlockFlasher>();
                    }

                    flasher.StartFlashing(Color.yellow, 0.5f);
                }
            }
        }
    }


    public void OnPlayerFoundExit()
    {

        mapGen.GenerateMap();
        allEnemiesKilled = false;
        Invoke(nameof(CountEnemies), 0.1f);
        timer = gameTime;
    }

    public void GameOver()
    {
        uiManager.ShowGameOver(score);
        SaveData.Instance.AddGears(gears);
        SaveData.Instance.SetScore(score);
        // SubmitScoreToLeaderboard();
    }

    // private void SubmitScoreToLeaderboard()
    // {
    //     StartCoroutine(SubmitScoreCoroutine());
    // }

    // private IEnumerator SubmitScoreCoroutine()
    // {
    //     yield return leaderboardManager.UpdateScore(score);
    // }


    public void UpdateUI()
    {
        uiManager.UpdateScore(score);
        uiManager.UpdateGears(gears);
        uiManager.UpdateLives(FindObjectOfType<PlayerController>().CurrentLives);
        uiManager.UpdateSpeed(FindObjectOfType<PlayerController>().moveSpeed);
    }

    void UpdateBombStatsUI()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            uiManager.UpdateBombCount(player.maxActiveBombs);
            uiManager.UpdateBombRange(player.bombRange);
        }
    }

    public void ResetScore()
    {
        score = 0;
        uiManager.UpdateScore(score);
    }

    public void ResetGears()
    {
        gears = 0;
        uiManager.UpdateGears(gears);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public int gears = 0;

    public int enemiesCount;

    public float gameTime = 180f; // 3 минуты
    private float timer;

    private LevelGenerator mapGen;

    private UIManager uiManager;

    private bool allEnemiesKilled = false;

    void Start()
    {
        mapGen = FindObjectOfType<LevelGenerator>();
        uiManager = FindObjectOfType<UIManager>();

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
        enemiesCount = FindObjectsOfType<Enemy>().Length;
        uiManager.UpdateEnemiesCount(enemiesCount);
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

    public void CollectBonus(GameObject bonus)
    {
        // Пример: увеличиваем дальность взрыва игрока
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.bombRange += 1;
        }
    }

    public void OnEnemyKilled(Enemy enemy)
    {
        enemiesCount--;
        uiManager.UpdateEnemiesCount(enemiesCount);

        if (enemiesCount <= 0)
        {
            allEnemiesKilled = true;
            HighlightDestructiblesWithBonusesAndExit();
        }
    }

    void HighlightDestructiblesWithBonusesAndExit()
    {
        // Можно реализовать визуальное подсвечивание нужных блоков,
        // например менять цвет или материал.
    }

    public void OnPlayerFoundExit()
    {
        // Генерируем новую карту, не завершая игру
        SaveRecord();

        // Можно реализовать плавный переход или эффект
        mapGen.GenerateMap();
        CountEnemies();
        timer = gameTime;
    }

    public void UpdateLives(int lives)
    {
        uiManager.UpdateLives(lives);
    }

    public void GameOver()
    {
        // Сохраняем рекорд
        SaveRecord();

        // Выводим экран конца игры
        uiManager.ShowGameOver(score);

        // Можно перезапустить игру через некоторое время или по кнопке
    }

    void SaveRecord()
    {
        int record = PlayerPrefs.GetInt("HighScore", 0);
        if (score > record)
        {
            PlayerPrefs.SetInt("HighScore", score);
        }
    }

    void UpdateUI()
    {
        uiManager.UpdateScore(score);
        uiManager.UpdateGears(gears);
        uiManager.UpdateEnemiesCount(enemiesCount);
        uiManager.UpdateLives(FindObjectOfType<PlayerController>().maxLives);
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

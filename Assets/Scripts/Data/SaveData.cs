using System.Collections;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    [Header("Player Data Configuration")]
    [SerializeField] private PlayerDataSO _playerDataSO;

    public static SaveData Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public int GetGears()
    {
        Debug.Log("GetGears called, returning: " + (_playerDataSO?.PlayerData.Gears ?? 0));
        return _playerDataSO?.PlayerData.Gears ?? 0;
    }
    public void AddGears(int amount)
    {
        if (_playerDataSO != null)
        {
            Debug.Log("Adding gears: " + amount);
            int currentGears = _playerDataSO.PlayerData.Gears;
            _playerDataSO.PlayerData.Gears = currentGears + amount;
        }
    }

    public bool TakeAwayGears(int amount)
    {
        if (_playerDataSO != null)
        {
            int currentGears = _playerDataSO.PlayerData.Gears;
            if (currentGears < amount)
            {
                Debug.LogWarning("Not enough gears to take away.");
                return false;
            }
            else
            {
                _playerDataSO.PlayerData.Gears = Mathf.Max(0, currentGears - amount);
                return true;
            }

        }
        return false;
    }

    public void SetUsername(string username)
    {
        if (_playerDataSO != null)
        {
            Debug.Log("Setting username: " + username);
            _playerDataSO.PlayerData.Username = username;
        }
    }

    public void SetScore(int score)
    {
        if (_playerDataSO != null)
        {
            if (score >= _playerDataSO.PlayerData.Score)
            {
                _playerDataSO.PlayerData.Score = score;
                SubmitScoreToLeaderboard(score);
            }
        }
    }

    private void SubmitScoreToLeaderboard(int score)
    {
        StartCoroutine(SubmitScoreCoroutine(score));
    }

    private IEnumerator SubmitScoreCoroutine(int score)
    {
        yield return LeaderboardManager.Instance.UpdateScore(score);
    }

    public int GetScore()
    {
        return _playerDataSO?.PlayerData.Score ?? 0;
    }
}

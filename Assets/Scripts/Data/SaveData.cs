using UnityEngine;

public class SaveData : MonoBehaviour
{
    [Header("Player Data Configuration")]
    [SerializeField] private PlayerDataSO _playerDataSO;

    public static SaveData Instance;

    private int _pendingScore = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromPrefs();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SaveToPrefs()
    {
        PlayerPrefs.SetInt("gears", _playerDataSO.PlayerData.Gears);
        PlayerPrefs.SetString("player_name", _playerDataSO.PlayerData.Username);
        PlayerPrefs.SetInt("score", _playerDataSO.PlayerData.Score);
        PlayerPrefs.SetInt("pendingScore", _pendingScore);
        PlayerPrefs.SetInt("isUsernamePending", _playerDataSO.PlayerData.IsUsernamePending ? 1 : 0);
        PlayerPrefs.SetInt("isScorePending", _playerDataSO.PlayerData.IsScorePending ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadFromPrefs()
    {
        if (!PlayerPrefs.HasKey("score")) return; // ничего не сохранялось раньше

        _playerDataSO.PlayerData.Gears = PlayerPrefs.GetInt("gears", 0);
        _playerDataSO.PlayerData.Username = PlayerPrefs.GetString("player_name", "Unknown");
        _playerDataSO.PlayerData.Score = PlayerPrefs.GetInt("score", 0);
        _pendingScore = PlayerPrefs.GetInt("pendingScore", -1);
        _playerDataSO.PlayerData.IsUsernamePending = PlayerPrefs.GetInt("isUsernamePending", 0) == 1;
        _playerDataSO.PlayerData.IsScorePending = PlayerPrefs.GetInt("isScorePending", 0) == 1;
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
            _playerDataSO.PlayerData.Gears += amount;
            SaveToPrefs();
            SaveToPrefs();
        }
    }

    public bool TakeAwayGears(int amount)
    {
        if (_playerDataSO != null)
        {
            if (_playerDataSO.PlayerData.Gears < amount)
                return false;

            _playerDataSO.PlayerData.Gears -= amount;
            SaveToPrefs();
            return true;

        }
        return false;
    }

    public void SetUsername(string username)
    {
        if (_playerDataSO != null)
        {
            _playerDataSO.PlayerData.Username = username;
            SaveToPrefs();
        }
    }

    public void SetScore(int score)
    {
        if (_playerDataSO != null)
        {
            if (score >= _playerDataSO.PlayerData.Score)
            {
                _playerDataSO.PlayerData.Score = score;
                SaveToPrefs();
            }
        }
    }

    public int GetScore()
    {
        return _playerDataSO?.PlayerData.Score ?? 0;
    }

    public bool IsUsernamePending()
    {
        return _playerDataSO != null && _playerDataSO.PlayerData.IsUsernamePending;
    }

    public void SetUsernamePending(bool value)
    {
        if (_playerDataSO != null)
        {
            _playerDataSO.PlayerData.IsUsernamePending = value;
            SaveToPrefs();
        }
    }

    public bool IsScorePending()
    {
        return _playerDataSO != null && _playerDataSO.PlayerData.IsScorePending;
    }

    public void SetScorePending(bool value)
    {
        if (_playerDataSO != null)
        {
            _playerDataSO.PlayerData.IsScorePending = value;
            SaveToPrefs();
        }
    }

    public void SetPendingScore(int score)
    {
        _pendingScore = score;
        SetScorePending(true);
        SaveToPrefs();
        Debug.Log("Pending score set to: " + score);
    }

    public int GetPendingScore()
    {
        return _pendingScore;
    }

    public void ClearPendingScore()
    {
        _pendingScore = -1;
        SetScorePending(false);
        SaveToPrefs();
        Debug.Log("Pending score cleared");
    }
}

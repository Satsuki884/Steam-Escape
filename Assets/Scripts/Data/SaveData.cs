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
            if (score < _playerDataSO.PlayerData.Score)
            {
                return;
            }
            else
            {
                _playerDataSO.PlayerData.Score = score;
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
            _playerDataSO.PlayerData.IsUsernamePending = value;
    }

    public bool IsScorePending()
    {
        return _playerDataSO != null && _playerDataSO.PlayerData.IsScorePending;
    }

    public void SetScorePending(bool value)
    {
        if (_playerDataSO != null)
            _playerDataSO.PlayerData.IsScorePending = value;
    }

    public void SetPendingScore(int score)
    {
        _pendingScore = score;
        SetScorePending(true);
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
        Debug.Log("Pending score cleared");
    }
}

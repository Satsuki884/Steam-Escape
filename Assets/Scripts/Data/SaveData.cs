using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class SaveData : MonoBehaviour
{
    [Header("Player Data Configuration")]
    [SerializeField] private PlayerDataSO _playerDataSO;

    public static SaveData Instance;

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, "save.json");

            LoadFromJson();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SaveToJson(); // save on exit
    }

    #region JSON Save/Load

    public void SaveToJson(bool encrypt = false)
    {
        if (_playerDataSO == null) return;

        string json = JsonUtility.ToJson(_playerDataSO.PlayerData, true);

        if (encrypt)
            json = Encrypt(json);

        File.WriteAllText(savePath, json, Encoding.UTF8);
        Debug.Log("Data saved to: " + savePath);
    }

    public void LoadFromJson(bool decrypt = false)
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found. Creating a new one.");
            return;
        }

        string json = File.ReadAllText(savePath, Encoding.UTF8);

        if (decrypt)
            json = Decrypt(json);

        JsonUtility.FromJsonOverwrite(json, _playerDataSO.PlayerData);
        Debug.Log("Data loaded from: " + savePath);
    }

    #endregion

    #region Simple Encryption

    private string Encrypt(string data)
    {
        byte key = 0xAA;
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] ^= key;
        return System.Convert.ToBase64String(bytes);
    }

    private string Decrypt(string encrypted)
    {
        byte key = 0xAA;
        byte[] bytes = System.Convert.FromBase64String(encrypted);
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] ^= key;
        return Encoding.UTF8.GetString(bytes);
    }

    #endregion
    #region Gears / Score / Username / Colors
    public int GetGears() => _playerDataSO?.PlayerData.Gears ?? 0;

    public void AddGears(int amount)
    {
        if (_playerDataSO != null)
            _playerDataSO.PlayerData.Gears += amount;
    }

    public bool TakeAwayGears(int amount)
    {
        if (_playerDataSO == null) return false;

        int currentGears = _playerDataSO.PlayerData.Gears;
        if (currentGears < amount)
            return false;

        _playerDataSO.PlayerData.Gears = Mathf.Max(0, currentGears - amount);
        return true;
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
                // SubmitScoreToLeaderboard(score);
            }
        }
    }

    public int GetScore() => _playerDataSO?.PlayerData.Score ?? 0;

    public List<string> GetOwnedSkinColors() => _playerDataSO?.PlayerData.OwnedSkinColors ?? new List<string>();

    public void AddToSkinOwnedColor(string colorId)
    {
        if (_playerDataSO != null && !_playerDataSO.PlayerData.OwnedSkinColors.Contains(colorId))
            _playerDataSO.PlayerData.OwnedSkinColors.Add(colorId);
    }

    public List<string> GetOwnedExplosionColors() => _playerDataSO?.PlayerData.OwnedExplosionColors ?? new List<string>();

    public void AddToExplosionOwnedColor(string colorId)
    {
        if (_playerDataSO != null && !_playerDataSO.PlayerData.OwnedExplosionColors.Contains(colorId))
            _playerDataSO.PlayerData.OwnedExplosionColors.Add(colorId);
    }

    public string GetPlayerColorId() => _playerDataSO?.PlayerData.PlayerColorId ?? "";

    public void SetPlayerColorId(string colorId)
    {
        if (_playerDataSO != null)
            _playerDataSO.PlayerData.PlayerColorId = colorId;
    }

    public string GetExplosionColorId() => _playerDataSO?.PlayerData.ExplosionColorId ?? "";

    public void SetExplosionColorId(string colorId)
    {
        if (_playerDataSO != null)
            _playerDataSO.PlayerData.ExplosionColorId = colorId;
    }

    #endregion

    #region Leaderboard

    private void SubmitScoreToLeaderboard(int score)
    {
        StartCoroutine(SubmitScoreCoroutine(score));
    }

    private IEnumerator SubmitScoreCoroutine(int score)
    {
        yield return LeaderboardManager.Instance.UpdateScore(score);
    }

    #endregion
}

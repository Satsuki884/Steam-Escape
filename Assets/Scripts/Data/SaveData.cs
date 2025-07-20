using UnityEngine;

public class SaveData : MonoBehaviour
{
    [Header("Player Data Configuration")]
    [SerializeField] private PlayerDataSO _playerDataSO;

    private static SaveData _instance;
    public static SaveData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveData>();
                if (_instance == null)
                {
                    GameObject saveDataObject = new GameObject("SaveData");
                    _instance = saveDataObject.AddComponent<SaveData>();
                    DontDestroyOnLoad(saveDataObject);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    public int GetGears()
    {
        return _playerDataSO?.PlayerData.GetGears() ?? 0;
    }
    public void AddGears(int amount)
    {
        if (_playerDataSO != null)
        {
            int currentGears = _playerDataSO.PlayerData.GetGears();
            _playerDataSO.PlayerData.SetGears(currentGears + amount);
        }
    }

    public bool TakeAwayGears(int amount)
    {
        if (_playerDataSO != null)
        {
            int currentGears = _playerDataSO.PlayerData.GetGears();
            if (currentGears < amount)
            {
                Debug.LogWarning("Not enough gears to take away.");
                return false;
            }
            else
            {
                _playerDataSO.PlayerData.SetGears(Mathf.Max(0, currentGears - amount));
                return true;
            }

        }
        return false;
    }

    public void SetUsername(string username)
    {
        if (_playerDataSO != null)
        {
            _playerDataSO.PlayerData.SetUsername(username);
        }
    }
}

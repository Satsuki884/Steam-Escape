using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "Data/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    [SerializeField] private PlayerData _playerData;
    public PlayerData PlayerData => _playerData;
}

[System.Serializable]
public class PlayerData
{
    [SerializeField] private int _gears;
    public int Gears
    {
        get { return _gears; }
        set { _gears = value; }
    }
    [SerializeField] private string _username;
    public string Username
    {
        get { return _username; }
        set { _username = value; }
    }

    [SerializeField] private int _score;
    public int Score
    {
        get { return _score; }
        set { _score = value; }
    }

    [SerializeField] private bool _isUsernamePending;
    public bool IsUsernamePending { get => _isUsernamePending; set => _isUsernamePending = value; }

    [SerializeField] private bool _isScorePending;
    public bool IsScorePending { get => _isScorePending; set => _isScorePending = value; }

    //TODO: realize shop with colors for player and explosion
    // [SerializeField] private Color _playerColor;
    // public Color PlayerColor
    // {
    //     get { return _playerColor; }
    //     set { _playerColor = value; }
    // }

    // [SerializeField] private int _explosionColor;
    // public int ExplosionColor
    // {
    //     get { return _explosionColor; }
    //     set { _explosionColor = value; }
    // }
}
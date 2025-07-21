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
}
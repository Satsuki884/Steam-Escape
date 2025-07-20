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
    public void SetGears(int value) { _gears = value; }
    public int GetGears() { return _gears; }
    [SerializeField] private string _username;
    public void SetUsername(string value) { _username = value; }
}
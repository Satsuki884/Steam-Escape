using System.Collections.Generic;
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
    // [SerializeField] private string _username;
    // public string Username
    // {
    //     get { return _username; }
    //     set { _username = value; }
    // }

    [SerializeField] private int _score = 0;
    public int Score
    {
        get { return _score; }
        set { _score = value; }
    }

    [SerializeField] private string _playerColorId;
    public string PlayerColorId
    {
        get => _playerColorId;
        set => _playerColorId = value;
    }

    [SerializeField] private string _explosionColorId;
    public string ExplosionColorId
    {
        get => _explosionColorId;
        set => _explosionColorId = value;
    }

    [SerializeField] private List<string> _ownedSkinColors = new List<string>();
    public List<string> OwnedSkinColors
    {
        get => _ownedSkinColors;
        set => _ownedSkinColors = value;
    }

    [SerializeField] private List<string> _ownedExplosionColors = new List<string>();
    public List<string> OwnedExplosionColors
    {
        get => _ownedExplosionColors;
        set => _ownedExplosionColors = value;
    }


}
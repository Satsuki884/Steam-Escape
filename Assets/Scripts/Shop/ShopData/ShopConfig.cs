using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Data/Shop Config")]
public class ShopConfig : ScriptableObject
{
    [Header("Player Colors")]
    public List<ColorItem> playerColors;

    [Header("Explosion Colors")]
    public List<ColorItem> explosionColors;

    public Color GetPlayerColorById(string id)
    {
        var item = playerColors.FirstOrDefault(c => c.Id == id);
        return item != null ? item.Color : Color.white;
    }

    public Color GetExplosionColorById(string id)
    {
        var item = explosionColors.FirstOrDefault(c => c.Id == id);
        return item != null ? item.Color : Color.white;
    }
}

[System.Serializable]
public class ColorItem
{
    [SerializeField] private string id;
    [SerializeField] private Color color;
    [SerializeField] private int price;

    public string Id => id;
    public Color Color => color;
    public int Price => price;
}

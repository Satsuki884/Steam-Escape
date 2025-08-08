using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private TMP_Text gearsText;
    [Header("References")]
    [SerializeField] private ShopConfig shopConfig;
    [SerializeField] private Transform playerColorsParent;
    [SerializeField] private Transform explosionColorsParent;
    [SerializeField] private ColorItemUI colorItemPrefab;

    private SaveData playerDataSO;

    void Start()
    {
        playerDataSO = SaveData.Instance;
        UpdateGearsText();
        RefreshShop();

    }

    public void UpdateGearsText()
    {
        gearsText.text = $"Gears: {playerDataSO.GetGears()}";
    }

    private void RefreshShop()
    {
        foreach (Transform child in playerColorsParent) Destroy(child.gameObject);
        foreach (Transform child in explosionColorsParent) Destroy(child.gameObject);

        foreach (var item in shopConfig.playerColors)
        {
            string label = GetLabelForColor(item.Color, item.Id, item.Price, isPlayer: true);
            var ui = Instantiate(colorItemPrefab, playerColorsParent);
            ui.Init(item.Id, item.Color, label, (id) => OnColorClicked(id, true));
        }

        // explosion colors
        foreach (var item in shopConfig.explosionColors)
        {
            string label = GetLabelForColor(item.Color, item.Id, item.Price, isPlayer: false);
            var ui = Instantiate(colorItemPrefab, explosionColorsParent);
            ui.Init(item.Id, item.Color, label, (id) => OnColorClicked(id, false));
        }
    }

    private List<string> GetOwnedColors(bool isPlayer)
    {
        return isPlayer ? playerDataSO.GetOwnedSkinColors() : playerDataSO.GetOwnedExplosionColors();
    }

    private string GetCurrentColorId(bool isPlayer)
    {
        return isPlayer ? playerDataSO.GetPlayerColorId() : playerDataSO.GetExplosionColorId();
    }

    private string GetLabelForColor(Color color, string colorId, int price, bool isPlayer)
    {
        var owned = GetOwnedColors(isPlayer);
        string currentId = GetCurrentColorId(isPlayer);

        if (owned.Contains(colorId))
        {
            return (colorId == currentId) ? "USED" : "OWNED";
        }
        else
        {
            return price.ToString();
        }
    }

    private void OnColorClicked(string colorId, bool isPlayer)
    {
        var owned = GetOwnedColors(isPlayer);
        var colorItem = isPlayer
            ? shopConfig.playerColors.FirstOrDefault(c => c.Id == colorId)
            : shopConfig.explosionColors.FirstOrDefault(c => c.Id == colorId);

        if (colorItem == null) return;

        if (owned.Contains(colorId))
        {
            // Просто выбираем
            if (isPlayer)
                playerDataSO.SetPlayerColorId(colorId);
            else
                playerDataSO.SetExplosionColorId(colorId);

            playerDataSO.SaveToJson();
        }
        else
        {
            // Покупка
            if (playerDataSO.GetGears() >= colorItem.Price)
            {
                playerDataSO.TakeAwayGears(colorItem.Price);

                if (isPlayer)
                    playerDataSO.AddToSkinOwnedColor(colorId);
                else
                    playerDataSO.AddToExplosionOwnedColor(colorId);

                if (isPlayer)
                    playerDataSO.SetPlayerColorId(colorId);
                else
                    playerDataSO.SetExplosionColorId(colorId);

                playerDataSO.SaveToJson();
            }
            else
            {
                Debug.Log("Not enough funds!");
                return;
            }
        }

        UpdateGearsText();
        RefreshShop();
    }


}
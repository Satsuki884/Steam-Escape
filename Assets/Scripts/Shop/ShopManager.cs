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
        ClearShop();
        PopulateShop(shopConfig.playerColors, playerColorsParent, true);
        PopulateShop(shopConfig.explosionColors, explosionColorsParent, false);
    }

    private void ClearShop()
    {
        foreach (Transform child in playerColorsParent) Destroy(child.gameObject);
        foreach (Transform child in explosionColorsParent) Destroy(child.gameObject);
    }

    private void PopulateShop(List<ColorItem> colorItems, Transform parent, bool isPlayer)
    {
        foreach (var item in colorItems)
        {
            var owned = GetOwnedColors(isPlayer);
            string currentId = GetCurrentColorId(isPlayer);
            bool isUsed = currentId == item.Id;
            bool isOwned = owned.Contains(item.Id);
            bool canAfford = playerDataSO.GetGears() >= item.Price;

            var ui = Instantiate(colorItemPrefab, parent);

            bool canBuy = !isOwned && canAfford;
            bool canUse = isOwned && !isUsed;

            if (isUsed)
                canUse = true;

            string buyLabel = canBuy ? $"{item.Price} G" : "Not enough";

            ui.Init(
                item.Id,
                item.Color,
                buyLabel,
                id => OnBuyClicked(id, isPlayer),
                id => OnUseClicked(id, isPlayer),
                canBuy,
                canUse,
                isUsed
            );
        }
    }

    private void OnUseClicked(string colorId, bool isPlayer)
    {
        if (isPlayer)
            playerDataSO.SetPlayerColorId(colorId);
        else
            playerDataSO.SetExplosionColorId(colorId);

        playerDataSO.SaveToJson();
        UpdateGearsText();
        RefreshShop();
        Canvas.ForceUpdateCanvases();
    }

    private void OnBuyClicked(string colorId, bool isPlayer)
    {
        var colorItem = isPlayer
            ? shopConfig.playerColors.FirstOrDefault(c => c.Id == colorId)
            : shopConfig.explosionColors.FirstOrDefault(c => c.Id == colorId);

        if (colorItem == null) return;

        if (playerDataSO.GetGears() < colorItem.Price)
        {
            Debug.Log("Not enough funds!");
            return;
        }

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
        UpdateGearsText();
        RefreshShop();
        Canvas.ForceUpdateCanvases();
    }


    private List<string> GetOwnedColors(bool isPlayer) =>
        isPlayer ? playerDataSO.GetOwnedSkinColors() : playerDataSO.GetOwnedExplosionColors();

    private string GetCurrentColorId(bool isPlayer) =>
        isPlayer ? playerDataSO.GetPlayerColorId() : playerDataSO.GetExplosionColorId();

    private string GetLabelForColor(string colorId, int price, bool isPlayer)
    {
        var owned = GetOwnedColors(isPlayer);
        string currentId = GetCurrentColorId(isPlayer);

        if (owned.Contains(colorId))
            return (colorId == currentId) ? "USED" : "USE";

        if (playerDataSO.GetGears() < price)
            return "Not enough";

        return price.ToString();
    }

    private void OnColorClicked(string colorId, bool isPlayer)
    {
        if (TrySelectOwnedColor(colorId, isPlayer)) return;
        TryPurchaseColor(colorId, isPlayer);

        UpdateGearsText();
        RefreshShop();
        Canvas.ForceUpdateCanvases();
    }

    private bool TrySelectOwnedColor(string colorId, bool isPlayer)
    {
        var owned = GetOwnedColors(isPlayer);
        if (!owned.Contains(colorId)) return false;

        if (isPlayer)
            playerDataSO.SetPlayerColorId(colorId);
        else
            playerDataSO.SetExplosionColorId(colorId);

        playerDataSO.SaveToJson();
        return true;
    }

    private void TryPurchaseColor(string colorId, bool isPlayer)
    {
        var colorItem = isPlayer
            ? shopConfig.playerColors.FirstOrDefault(c => c.Id == colorId)
            : shopConfig.explosionColors.FirstOrDefault(c => c.Id == colorId);

        if (colorItem == null) return;

        if (playerDataSO.GetGears() < colorItem.Price)
        {
            Debug.Log("Not enough funds!");
            return;
        }

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


}
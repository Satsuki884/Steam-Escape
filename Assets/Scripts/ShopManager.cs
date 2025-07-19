using UnityEngine;

public class ShopManager : MonoBehaviour {
    public int[] skinPrices;
    public SkinManager skinManager;

    public void BuySkin(int id) {
        if (GearManager.gearsCollected >= skinPrices[id]) {
            GearManager.gearsCollected -= skinPrices[id];
            skinManager.SetSkin(id);
        }
    }
}
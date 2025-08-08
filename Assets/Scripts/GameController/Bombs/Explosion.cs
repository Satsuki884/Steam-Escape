using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private Renderer explosionRenderer;
    [SerializeField] private ShopConfig shopConfig;

    void Start()
    {
        var saveData = SaveData.Instance;
        if (explosionRenderer != null)
        {
            Color explosionColor = shopConfig.GetExplosionColorById(saveData.GetExplosionColorId());
            explosionRenderer.material.color = explosionColor;
        }
        Destroy(gameObject, 0.5f);
    }

}

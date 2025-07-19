using UnityEngine;

public class SkinManager : MonoBehaviour {
    public Material[] skinMaterials;
    public Renderer playerRenderer;

    void Start() {
        int id = PlayerPrefs.GetInt("SkinID", 0);
        playerRenderer.material = skinMaterials[id];
    }

    public void SetSkin(int id) {
        PlayerPrefs.SetInt("SkinID", id);
        playerRenderer.material = skinMaterials[id];
    }
}
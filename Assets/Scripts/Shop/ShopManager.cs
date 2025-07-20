using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private TMP_Text gearsText;
    private SaveData saveData;

    void Start()
    {
        saveData = FindObjectOfType<SaveData>();
        UpdateGearsText();

    }

    public void UpdateGearsText()
    {
        gearsText.text = $"Gears: {saveData.GetGears()}";
    }

}
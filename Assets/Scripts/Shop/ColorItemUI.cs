using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorItemUI : MonoBehaviour
{
    [SerializeField] private Image colorImage;
    // [SerializeField] private TMP_Text labelText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button useButton;

    private string colorId;
    private System.Action<string> onClick;

    public void Init(string id, Color color, string label, System.Action<string> buyCallback, System.Action<string> useCallback, bool canBuy, bool canUse, bool isUsed)
    {
        colorId = id;
        colorImage.color = color;

        buyButton.GetComponentInChildren<TMP_Text>().text = label;
        useButton.GetComponentInChildren<TMP_Text>().text = "USE";

        buyButton.onClick.RemoveAllListeners();
        useButton.onClick.RemoveAllListeners();

        if (canBuy)
        {
            buyButton.gameObject.SetActive(true);
            buyButton.interactable = true;
            buyButton.onClick.AddListener(() => buyCallback?.Invoke(colorId));
        }
        else
        {
            buyButton.interactable = false;
            buyButton.onClick.RemoveAllListeners();
        }

        if (canUse)
        {
            buyButton.gameObject.SetActive(false);
            useButton.gameObject.SetActive(true);
            useButton.interactable = !isUsed;
            useButton.onClick.AddListener(() => useCallback?.Invoke(colorId));
            if (isUsed)
                useButton.GetComponentInChildren<TMP_Text>().text = "USED";
            else
                useButton.GetComponentInChildren<TMP_Text>().text = "USE";
        }
        else
        {
            useButton.gameObject.SetActive(false);
        }
    }


}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorItemUI : MonoBehaviour
{
    [SerializeField] private Image colorImage;
    // [SerializeField] private TMP_Text labelText;
    [SerializeField] private Button actionButton;

    private string colorId;
    private System.Action<string> onClick;

    public void Init(string id, Color color, string label, System.Action<string> clickCallback)
    {
        colorId = id;
        colorImage.color = color;
        actionButton.GetComponentInChildren<TMP_Text>().text = label;
        onClick = clickCallback;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => onClick?.Invoke(colorId));
    }
}

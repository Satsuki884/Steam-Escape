using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController player;
    [Header("UI Buttons")]
    [SerializeField] private Button buttonUp;
    [SerializeField] private Button buttonDown;
    [SerializeField] private Button buttonLeft;
    [SerializeField] private Button buttonRight;
    [SerializeField] private Button buttonBomb;

    void Start()
    {
        // Запускаем ожидание появления игрока
        StartCoroutine(WaitForPlayerAndConnect());
    }

    IEnumerator WaitForPlayerAndConnect()
    {
        // Ждём пока игрок появится в сцене
        while (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            yield return null;
        }

        // Когда нашли — подключаем кнопки
        buttonUp.onClick.AddListener(() => player.SetMoveDirection(Vector2Int.up));
        buttonDown.onClick.AddListener(() => player.SetMoveDirection(Vector2Int.down));
        buttonLeft.onClick.AddListener(() => player.SetMoveDirection(Vector2Int.left));
        buttonRight.onClick.AddListener(() => player.SetMoveDirection(Vector2Int.right));
        buttonBomb.onClick.AddListener(() => player.TryPlaceBomb());
    }
}

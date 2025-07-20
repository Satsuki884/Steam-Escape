using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController player;
    [Header("UI Buttons")]
    [SerializeField] private Button buttonBomb;

    void Start()
    {
        StartCoroutine(WaitForPlayerAndConnect());
    }

    IEnumerator WaitForPlayerAndConnect()
    {
        while (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            yield return null;
        }
        buttonBomb.onClick.AddListener(() => player.TryPlaceBomb());
    }
}

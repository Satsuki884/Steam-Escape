using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoldHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector2Int direction;
    private PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }
        }

        player.SetMoveDirection(direction);
        player.SetUsingUIInput(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }
        }

        player.SetMoveDirection(Vector2Int.zero);
        player.SetUsingUIInput(false);
    }

}

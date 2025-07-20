using UnityEngine;

public abstract class BonusBase : MonoBehaviour
{
    public abstract BonusType Type { get; }

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameManager != null)
            {
                gameManager.CollectBonus(Type);
            }
            Destroy(gameObject);
        }
    }
}

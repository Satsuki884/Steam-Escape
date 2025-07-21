using UnityEngine;

public abstract class BonusBase : MonoBehaviour
{
    public abstract BonusType Type { get; }

    private GameManager gameManager;

    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatFrequency = 1f;

    private Vector3 startPos;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        startPos = transform.position;
    }

    private void Update()
    {
        FloatMotion();
    }

    private void FloatMotion()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
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

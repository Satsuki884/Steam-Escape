using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    public float moveSpeed = 2f;
    private Vector2Int gridPosition;

    public GameObject bombPrefab;

    public int bombRange = 1;

    private LevelGenerator mapGen;
    private GameManager gameManager;

    private Rigidbody2D rb;

    private Vector2 movement;

    void Start()
    {
        currentLives = maxLives;
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody2D>();

        gridPosition = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        movement = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBomb();
        }

        if (Input.GetKey(KeyCode.UpArrow))
            movement = Vector2.up;
        else if (Input.GetKey(KeyCode.DownArrow))
            movement = Vector2.down;
        else if (Input.GetKey(KeyCode.LeftArrow))
            movement = Vector2.left;
        else if (Input.GetKey(KeyCode.RightArrow))
            movement = Vector2.right;
    }

    void FixedUpdate()
    {
        if (movement != Vector2.zero)
            Move(movement);
    }

    void Move(Vector2 dir)
    {
        Vector2Int targetPos = gridPosition + Vector2Int.RoundToInt(dir);

        // Проверяем коллизию с неразрушаемыми стенами
        if (mapGen.IsIndestructible(targetPos))
            return;

        gridPosition = targetPos;
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    void PlaceBomb()
    {
        Instantiate(bombPrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explosion"))
        {
            TakeDamage();
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage();
        }
        else if (other.CompareTag("Gear"))
        {
            gameManager.AddGears(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Bonus"))
        {
            gameManager.CollectBonus(other.gameObject);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Exit"))
        {
            gameManager.OnPlayerFoundExit();
        }
    }

    void TakeDamage()
    {
        currentLives--;
        gameManager.UpdateLives(currentLives);

        if (currentLives <= 0)
        {
            gameManager.GameOver();
            Destroy(gameObject);
        }
        else
        {
            // Можно добавить эффект урона, мигалки и т.п.
            // И телепортировать игрока на старт, например
        }
    }
}

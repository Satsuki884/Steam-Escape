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
        // HandleInput();
        HandleKeyboardInput();

        if (bombRequested)
        {
            PlaceBomb();
            bombRequested = false;
        }
    }
    // void FixedUpdate()
    // {
    //     if (movement != Vector2.zero)
    //         Move(movement);
    // }
    void FixedUpdate()
    {
        if (pendingInput != Vector2Int.zero)
        {
            Move(pendingInput);
            pendingInput = Vector2Int.zero;
        }
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            bombRequested = true;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            pendingInput = Vector2Int.up;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            pendingInput = Vector2Int.down;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            pendingInput = Vector2Int.left;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            pendingInput = Vector2Int.right;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explosion") || other.CompareTag("Enemy"))
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
    public int maxActiveBombs = 1;
    private int currentActiveBombs = 0;


    void PlaceBomb()
    {
        if (currentActiveBombs >= maxActiveBombs)
            return;

        Vector3 spawnPos = new Vector3(gridPosition.x, gridPosition.y, 0);
        GameObject bombObj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        Bomb bombScript = bombObj.GetComponent<Bomb>();
        if (bombScript != null)
            bombScript.SetOwner(this); // Привязываем владельца

        currentActiveBombs++;
    }


    public void OnBombExploded()
    {
        currentActiveBombs = Mathf.Max(0, currentActiveBombs - 1);
    }

    public void IncreaseMaxBombs(int amount)
    {
        maxActiveBombs += amount;
    }

    // Методы для UI-кнопок
    private Vector2Int pendingInput = Vector2Int.zero;
    private bool bombRequested = false;

    public void SetMoveDirection(Vector2Int dir)
    {
        pendingInput = dir;
    }

    public void TryPlaceBomb()
    {
        bombRequested = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    public int CurrentLives
    {
        get { return currentLives; }
        set
        {
            currentLives = value;
        }
    }

    [SerializeField] private Rigidbody rb;

    private Vector2 targetWorldPos;
    private bool hasTarget = false;

    public float moveSpeed = 2f;
    private Vector2Int gridPosition;

    [SerializeField] private GameObject bombPrefab;

    public int bombRange = 1;

    private LevelGenerator mapGen;
    private GameManager gameManager;

    private bool isInvincible = false;

    [SerializeField] private Renderer meshRenderer;
    private Color originalColor;

    // private Vector2 movement;

    void Start()
    {

        currentLives = maxLives;
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();
        ResetPlayerState();
        gridPosition = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        if (meshRenderer == null)
            meshRenderer = GetComponent<Renderer>();

        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
    }

    // private bool usingUIInput = false;

    // void Update()
    // {
    //     if (!isMoving && moveDirection != Vector2Int.zero)
    //     {
    //         TryMove(moveDirection);
    //     }
    // }

    void Update()
    {
        if (!hasTarget && moveDirection != Vector2Int.zero)
        {
            Vector2Int targetGridPos = gridPosition + moveDirection;

            if (!mapGen.IsIndestructible(targetGridPos) && !mapGen.IsDestructible(targetGridPos))
            {
                targetWorldPos = new Vector2(targetGridPos.x, targetGridPos.y);
                hasTarget = true;
                gridPosition = targetGridPos;
            }
        }

        if (hasTarget)
        {
            Vector2 currentPos = rb.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetWorldPos, moveSpeed * Time.deltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(newPos, targetWorldPos) < 0.01f)
            {
                rb.MovePosition(targetWorldPos);
                hasTarget = false;
            }
        }
    }

    // void TryMove(Vector2Int dir)
    // {
    //     Vector2Int targetPos = gridPosition + dir;

    //     if (mapGen.IsIndestructible(targetPos) || mapGen.IsDestructible(targetPos))
    //     {
    //         return;
    //     }

    //     StartCoroutine(MoveSmoothly(targetPos));
    // }

    // IEnumerator MoveSmoothly(Vector2Int targetGridPos)
    // {
    //     isMoving = true;

    //     Vector3 startPos = transform.position;
    //     Vector3 endPos = new Vector3(targetGridPos.x, targetGridPos.y, 0);
    //     float t = 0;

    //     while (t < 1f)
    //     {
    //         t += Time.deltaTime * moveSpeed;
    //         transform.position = Vector3.Lerp(startPos, endPos, t);
    //         yield return null;
    //     }

    //     gridPosition = targetGridPos;
    //     transform.position = endPos;

    //     isMoving = false;
    // }
    private Vector2Int moveDirection = Vector2Int.zero;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion") || other.CompareTag("Enemy") && !isInvincible)
        {
            TakeDamage();
        }
        else if (other.CompareTag("Gear"))
        {
            gameManager.AddGears(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Exit"))
        {
            Destroy(other.gameObject);
            gameManager.OnPlayerFoundExit();
        }
    }

    public void TakeDamage()
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
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        Debug.Log("Player is now invincible!");
        isInvincible = true;
        float originalSpeed = moveSpeed;
        moveSpeed = originalSpeed * 2f;

        float flashInterval = 0.2f;
        float timer = 0f;
        float duration = 5f;

        while (timer < duration)
        {
            // Меняем цвет на голубой
            if (meshRenderer != null)
                meshRenderer.material.color = Color.cyan;

            yield return new WaitForSeconds(flashInterval);

            // Возвращаем оригинальный цвет
            if (meshRenderer != null)
                meshRenderer.material.color = originalColor;

            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2;
        }

        // Восстанавливаем скорость и цвет
        moveSpeed = originalSpeed;
        if (meshRenderer != null)
            meshRenderer.material.color = originalColor;

        isInvincible = false;
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
        {
            bombScript.SetOwner(this);
            bombScript.SetBombRange(bombRange);  // передаём радиус сюда
        }
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

    public void SetMoveDirection(Vector2Int dir)
    {
        moveDirection = dir;
    }

    // public void SetUsingUIInput(bool value)
    // {
    //     usingUIInput = value;
    // }


    public void TryPlaceBomb()
    {
        if (currentActiveBombs < maxActiveBombs)
        {
            PlaceBomb();
        }
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    public void ResetPlayerState()
    {
        currentLives = maxLives = 3;
        moveSpeed = 15f;
        bombRange = 1;
        maxActiveBombs = 1;
        currentActiveBombs = 0;
        gameManager.ResetScore();      // Обнулим очки
        gameManager.ResetGears();      // Обнулим шестерёнки

        gridPosition = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        moveDirection = Vector2Int.zero;
    }


}

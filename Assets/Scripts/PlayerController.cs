using System.Collections;
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

    // private Vector2 movement;

    void Start()
    {

        currentLives = maxLives;
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();
        ResetPlayerState();
        gridPosition = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        usingUIInput = true;
    }

    private bool usingUIInput = false;

    void Update()
    {
        if (!isMoving && moveDirection != Vector2Int.zero)
        {
            TryMove(moveDirection);
        }
    }

    void TryMove(Vector2Int dir)
    {
        Vector2Int targetPos = gridPosition + dir;

        if (mapGen.IsIndestructible(targetPos) || mapGen.IsDestructible(targetPos))
        {
            return;
        }

        StartCoroutine(MoveSmoothly(targetPos));
    }

    IEnumerator MoveSmoothly(Vector2Int targetGridPos)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetGridPos.x, targetGridPos.y, 0);
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        gridPosition = targetGridPos;
        transform.position = endPos;

        isMoving = false;
    }
    private bool isMoving = false;
    private Vector2Int moveDirection = Vector2Int.zero;

    private void OnTriggerEnter(Collider other)
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
            Destroy(other.gameObject);
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

    public void SetUsingUIInput(bool value)
    {
        usingUIInput = value;
    }


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
        moveSpeed = 2f;
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

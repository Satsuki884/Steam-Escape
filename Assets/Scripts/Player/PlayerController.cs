using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    [SerializeField] private float baseSpeed = 5f;
    private float speedBonus = 0f;
    private float invincibilityMultiplier = 1f;
    public float moveSpeed
    {
        get { return (baseSpeed + speedBonus) * invincibilityMultiplier; }
    }
    private Vector2Int gridPosition;

    [SerializeField] private GameObject bombPrefab;

    public int bombRange = 1;

    private LevelGenerator mapGen;
    private GameManager gameManager;

    private bool isInvincible = false;

    [SerializeField] private Renderer meshRenderer;
    private SaveData saveData;
    [SerializeField] private ShopConfig shopConfig;
    private Color originalColor;

    private bool canMove = true;

    void Start()
    {

        saveData = SaveData.Instance;
        Color playerColor = shopConfig.GetPlayerColorById(saveData.GetPlayerColorId());
        if (meshRenderer != null)
            meshRenderer.material.color = playerColor;

        rb.isKinematic = true;
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

    // void Update()
    // {

    //     if (!canMove) return;
    //     if (!hasTarget && moveDirection != Vector2Int.zero)
    //     {
    //         Vector2Int targetGridPos = gridPosition + moveDirection;

    //         if (!mapGen.IsIndestructible(targetGridPos) && !mapGen.IsDestructible(targetGridPos))
    //         {
    //             targetWorldPos = new Vector2(targetGridPos.x, targetGridPos.y);
    //             hasTarget = true;
    //             gridPosition = targetGridPos;
    //         }
    //     }

    //     if (hasTarget)
    //     {
    //         Vector2 currentPos = rb.position;
    //         Vector2 newPos = Vector2.MoveTowards(currentPos, targetWorldPos, moveSpeed * Time.deltaTime);
    //         rb.MovePosition(newPos);

    //         if (Vector2.Distance(newPos, targetWorldPos) < 0.01f)
    //         {
    //             rb.MovePosition(targetWorldPos);
    //             hasTarget = false;
    //         }
    //     }
    // }

    void Update()
    {
        if (!canMove) return;

        if (!hasTarget && moveDirection != Vector2Int.zero)
        {
            Vector2Int targetGridPos = gridPosition + moveDirection;

            if (!mapGen.IsIndestructible(targetGridPos) && !mapGen.IsDestructible(targetGridPos))
            {
                targetWorldPos = new Vector2(targetGridPos.x, targetGridPos.y);
                hasTarget = true;
                gridPosition = targetGridPos;
                MoveToTarget(targetWorldPos);
            }
        }
    }

    private void MoveToTarget(Vector2 target)
    {
        float distance = Vector2.Distance(rb.position, target);
        float duration = distance / moveSpeed;

        rb.DOMove(target, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            hasTarget = false;
        });
    }

    private Vector2Int moveDirection = Vector2Int.zero;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !isInvincible)
        {
            AudioManager.Instance.PlaySFX("enemyHit");
            TakeDamage();
        }
        else if (other.CompareTag("Explosion") && !isInvincible)
        {
            AudioManager.Instance.PlaySFX("explosionHit");
            TakeDamage();
        }
        else if (other.CompareTag("Gear"))
        {
            AudioManager.Instance.PlaySFX("bonus");
            gameManager.AddGears(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Exit"))
        {
            gameManager.AddScore(250);
            AudioManager.Instance.PlaySFX("newLevel");
            Destroy(other.gameObject);
            gameManager.OnPlayerFoundExit();
        }
    }

    public void TakeDamage()
    {
        if (isInvincible) return;
        currentLives--;
        gameManager.UpdateUI();
        if (currentLives <= 0)
        {
            AudioManager.Instance.PlaySFX("endGame");
            gameManager.GameOver();
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    public void AddSpeedBonus(float amount)
    {
        speedBonus += amount;
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        invincibilityMultiplier = 1.5f;
        canMove = false;
        gameManager.UpdateUI();

        Color startColor = meshRenderer.material.color;
        Vector3 startScale = transform.localScale;

        float t = 0f;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            meshRenderer.material.color = Color.Lerp(startColor, Color.black, t / 0.75f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / 0.75f);
            yield return null;
        }
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, t / 1.5f);
            yield return null;
        }
        t = 0f;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            meshRenderer.material.color = Color.Lerp(Color.black, startColor, t / 0.75f);
            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, t / 0.75f);
            yield return null;
        }

        canMove = true;

        float flashInterval = 0.2f;
        float timer = 0f;
        float duration = 5f;

        while (timer < duration)
        {
            if (meshRenderer != null)
                meshRenderer.material.color = Color.cyan;

            yield return new WaitForSeconds(flashInterval);

            if (meshRenderer != null)
                meshRenderer.material.color = startColor;

            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2;
        }

        invincibilityMultiplier = 1f;
        gameManager.UpdateUI();

        if (meshRenderer != null)
            meshRenderer.material.color = startColor;

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
        AudioManager.Instance.PlaySFX("bomb");

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

    public void TryPlaceBomb()
    {
        if (currentActiveBombs < maxActiveBombs && canMove)
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
        currentLives = maxLives = 5;
        baseSpeed = 5f;
        speedBonus = 0f;
        invincibilityMultiplier = 1f;
        bombRange = 1;
        maxActiveBombs = 1;
        currentActiveBombs = 0;
        gameManager.ResetScore();
        gameManager.ResetGears();
        gameManager.UpdateUI();

        gridPosition = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        moveDirection = Vector2Int.zero;
    }


}

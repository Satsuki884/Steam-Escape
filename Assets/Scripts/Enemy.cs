using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveInterval = 1f;
    private float moveTimer;

    private Vector2Int gridPosition;

    private LevelGenerator mapGen;
    private GameManager gameManager;

    void Start()
    {
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();

        gridPosition = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    void Update()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0;
            MoveRandom();
        }
    }

    void MoveRandom()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int dir = directions[Random.Range(0, directions.Length)];
        Vector2Int targetPos = gridPosition + dir;

        if (mapGen.IsIndestructible(targetPos) || mapGen.IsDestructible(targetPos))
            return;

        gridPosition = targetPos;
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explosion"))
        {
            gameManager.AddScore(100);
            gameManager.OnEnemyKilled(this);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            // Урон игроку обработается в PlayerController
        }
    }
}
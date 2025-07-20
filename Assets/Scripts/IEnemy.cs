using UnityEngine;

public class IEnemy : MonoBehaviour
{
    protected Vector2Int gridPosition;
    protected LevelGenerator mapGen;
    protected GameManager gameManager;

    protected Vector2Int currentDirection = Vector2Int.right;

    protected Rigidbody rb;

    protected Vector3 targetWorldPos;
    [SerializeField] private float moveSpeed = 3f;
    protected bool hasTarget = false;

    void Start()
    {
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();

        gridPosition = Vector2Int.RoundToInt(transform.position);
        targetWorldPos = GridToWorld(gridPosition);
        rb.position = targetWorldPos;
        hasTarget = true;
    }
    void FixedUpdate()
    {
        if (!hasTarget)
        {
            DecideNextDirection();

            Vector2Int nextPos = gridPosition + currentDirection;

            if (CanMoveTo(nextPos))
            {
                gridPosition = nextPos;
                targetWorldPos = GridToWorld(gridPosition);
                hasTarget = true;
            }
            else
            {
                hasTarget = false;
                return;
            }
        }

        Vector3 newPos = Vector3.MoveTowards(rb.position, targetWorldPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector3.Distance(rb.position, targetWorldPos) < 0.01f)
        {
            rb.position = targetWorldPos;
            hasTarget = false;
        }
    }



    void DecideNextDirection()
    {
        Vector2Int[] lateralDirs = GetLateralDirections(currentDirection);
        Vector2Int forwardPos = gridPosition + currentDirection;

        bool forwardBlocked = !CanMoveTo(forwardPos);
        bool lateralLeftBlocked = !CanMoveTo(gridPosition + lateralDirs[0]);
        bool lateralRightBlocked = !CanMoveTo(gridPosition + lateralDirs[1]);

        if (forwardBlocked && lateralLeftBlocked && lateralRightBlocked)
        {
            Vector2Int backDir = -currentDirection;
            if (CanMoveTo(gridPosition + backDir))
                currentDirection = backDir;
            return;
        }

        if (forwardBlocked)
        {
            foreach (Vector2Int lateralDir in lateralDirs)
            {
                if (CanMoveTo(gridPosition + lateralDir))
                {
                    currentDirection = lateralDir;
                    return;
                }
            }
        }
        else
        {
            // направление не меняем, продолжаем двигаться вперёд
        }
    }

    protected bool CanMoveTo(Vector2Int pos)
    {
        if (mapGen.IsIndestructible(pos) || mapGen.IsDestructible(pos))
            return false;

        Collider[] hits = Physics.OverlapSphere(GridToWorld(pos), 0.1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("Bomb"))
                return false;
        }

        return true;
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x, gridPos.y, 0f);
    }

    Vector2Int[] GetLateralDirections(Vector2Int dir)
    {
        if (dir == Vector2Int.up || dir == Vector2Int.down)
            return new[] { Vector2Int.left, Vector2Int.right };
        else
            return new[] { Vector2Int.up, Vector2Int.down };
    }
}
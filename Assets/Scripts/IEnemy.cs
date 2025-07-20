using System.Collections.Generic;
using UnityEngine;

public class IEnemy : MonoBehaviour
{
    public float moveInterval = 1f;
    protected float moveTimer;

    protected Vector2Int gridPosition;

    protected LevelGenerator mapGen;
    protected GameManager gameManager;

    private Vector2Int currentDirection = Vector2Int.right;

    private Rigidbody rb;

    private Vector3 targetWorldPos;
    private bool isMoving = false;
    private float moveSpeed = 3f; // скорость движения между клетками

    void Start()
    {
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();

        gridPosition = Vector2Int.RoundToInt(transform.position);
        targetWorldPos = new Vector3(gridPosition.x, gridPosition.y, 0);
        transform.position = targetWorldPos;
    }

    protected virtual void Update()
    {
        // Если враг уже движется, не запускаем новое движение
        if (isMoving) return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Vector2Int oldGridPos = gridPosition;
            MoveSmart();

            // Если позиция изменилась — начинаем движение
            if (gridPosition != oldGridPos)
            {
                targetWorldPos = new Vector3(gridPosition.x, gridPosition.y, 0);
                isMoving = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            // Плавно двигаем Rigidbody к targetWorldPos
            Vector3 newPos = Vector3.MoveTowards(rb.position, targetWorldPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            // Если дошли до цели — останавливаем движение
            if (Vector3.Distance(rb.position, targetWorldPos) < 0.01f)
            {
                rb.position = targetWorldPos;
                isMoving = false;
            }
        }
    }

    void MoveSmart()
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
            {
                currentDirection = backDir;
                gridPosition += backDir;
            }
            return;
        }

        if (forwardBlocked)
        {
            foreach (Vector2Int lateralDir in lateralDirs)
            {
                Vector2Int targetPos = gridPosition + lateralDir;
                if (CanMoveTo(targetPos))
                {
                    currentDirection = lateralDir;
                    gridPosition = targetPos;
                    return;
                }
            }
        }

        if (!forwardBlocked)
        {
            gridPosition = forwardPos;
        }
    }

    bool CanMoveTo(Vector2Int pos)
    {
        if (mapGen.IsIndestructible(pos) || mapGen.IsDestructible(pos))
            return false;

        Collider[] hits = Physics.OverlapSphere(new Vector3(pos.x, pos.y, 0), 0.1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("Bomb"))
                return false;
        }

        return true;
    }

    Vector2Int[] GetLateralDirections(Vector2Int dir)
    {
        if (dir == Vector2Int.up || dir == Vector2Int.down)
            return new[] { Vector2Int.left, Vector2Int.right };
        else
            return new[] { Vector2Int.up, Vector2Int.down };
    }
}

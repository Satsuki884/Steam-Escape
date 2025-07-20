using System.Collections.Generic;
using UnityEngine;

public class IEnemy : MonoBehaviour
{
    public float moveInterval = 1f;
    protected float moveTimer;

    protected Vector2Int gridPosition;

    protected LevelGenerator mapGen;
    protected GameManager gameManager;

    private Vector2Int currentDirection = Vector2Int.right; // начальное направление (можно рандомизировать)
    // private float moveCooldown = 0.5f;

    void Start()
    {
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();

        gridPosition = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    protected virtual void Update()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0;
            MoveSmart();
        }
    }

    void MoveSmart()
    {
        Vector2Int[] lateralDirs = GetLateralDirections(currentDirection);
        List<Vector2Int> tryDirections = new List<Vector2Int> { currentDirection };
        tryDirections.AddRange(lateralDirs); // влево и вправо от текущего
        tryDirections.Add(-currentDirection); // в конец списка — назад, если всё заблокировано

        foreach (Vector2Int dir in tryDirections)
        {
            Vector2Int targetPos = gridPosition + dir;
            if (CanMoveTo(targetPos))
            {
                currentDirection = dir;
                gridPosition = targetPos;
                transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
                return;
            }
        }
    }

    bool CanMoveTo(Vector2Int pos)
    {
        if (mapGen.IsIndestructible(pos) || mapGen.IsDestructible(pos))
            return false;

        // Проверка на врагов, бомбы и другие препятствия
        Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(pos.x, pos.y));
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
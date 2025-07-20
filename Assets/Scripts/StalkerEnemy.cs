using System;
using UnityEngine;

public class StalkerEnemy : IEnemy
{
    private bool isFollowingPlayer = false;
    private Transform playerTransform;
    private bool isMoving = false;
    private Vector3 targetWorldPosition;

    // void Update()
    // {
    //     if (isMoving)
    //     {
    //         MoveTowardsTarget();
    //         return;
    //     }

    //     if (isFollowingPlayer && playerTransform != null)
    //     {
    //         TryFollowPlayer(playerTransform.position);
    //     }
    // }

    // private void TryFollowPlayer(Vector3 playerWorldPos)
    // {
    //     Vector2Int playerGrid = Vector2Int.RoundToInt(new Vector2(playerWorldPos.x, playerWorldPos.y));
    //     Vector2Int delta = playerGrid - gridPosition;

    //     Vector2Int[] directions;

    //     // Приоритетная ось: сначала X, потом Y — или наоборот
    //     if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
    //     {
    //         directions = new[]
    //         {
    //             new Vector2Int(Math.Sign(delta.x), 0),
    //             new Vector2Int(0, Math.Sign(delta.y))
    //         };
    //     }
    //     else
    //     {
    //         directions = new[]
    //         {
    //             new Vector2Int(0, Math.Sign(delta.y)),
    //             new Vector2Int(Math.Sign(delta.x), 0)
    //         };
    //     }

    //     foreach (Vector2Int dir in directions)
    //     {
    //         Vector2Int targetGrid = gridPosition + dir;
    //         if (CanMoveTo(targetGrid))
    //         {
    //             gridPosition = targetGrid;
    //             targetWorldPosition = GridToWorld(gridPosition);
    //             isMoving = true;
    //             break;
    //         }
    //     }
    // }

    // private void MoveTowardsTarget()
    // {
    //     float speed = 5f;
    //     transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, speed * Time.deltaTime);

    //     if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
    //     {
    //         transform.position = targetWorldPosition;
    //         isMoving = false;
    //     }
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            gameManager.AddScore(100);
            gameManager.OnEnemyKilled(this);
            Destroy(gameObject);
        }
        // else if (other.CompareTag("PlayerZone"))
        // {
        //     PlayerController player = other.GetComponentInParent<PlayerController>();
        //     if (player != null)
        //     {
        //         isFollowingPlayer = true;
        //         playerTransform = player.transform;
        //     }
        // }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("PlayerZone"))
    //     {
    //         isFollowingPlayer = false;
    //         playerTransform = null;
    //     }
    // }

    // // Этот метод уже есть в IEnemy, но на случай если нужен в отдельном скрипте
    // protected Vector3 GridToWorld(Vector2Int gridPos)
    // {
    //     return new Vector3(gridPos.x, gridPos.y, 0f);
    // }
}

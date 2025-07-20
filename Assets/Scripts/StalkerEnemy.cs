using System;
using UnityEngine;

public class StalkerEnemy : IEnemy
{
    private bool isFollowingPlayer = false;
    private Transform playerTransform;
    private float moveCooldown = 0.5f;
    // private float moveTimer = 0f;

    private void Update()
    {
        if (isFollowingPlayer && playerTransform != null)
        {
            moveTimer += Time.deltaTime;
            if (moveTimer >= moveCooldown)
            {
                FollowPlayer(playerTransform.position);
                moveTimer = 0f;
            }
        }
        else
        {
            base.Update();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            gameManager.AddScore(100);
            gameManager.OnEnemyKilled(this);
            Destroy(gameObject);
        }
        else if (other.CompareTag("PlayerZone"))
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                isFollowingPlayer = true;
                playerTransform = player.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerZone"))
        {
            isFollowingPlayer = false;
            playerTransform = null;
        }
    }

    private void FollowPlayer(Vector3 playerWorldPos)
    {
        Vector2Int playerPos = Vector2Int.RoundToInt(new Vector2(playerWorldPos.x, playerWorldPos.y));
        Vector2Int direction = Vector2Int.RoundToInt(((Vector2)(playerPos - gridPosition)).normalized);
        Vector2Int targetPos = gridPosition + direction;

        if (!mapGen.IsIndestructible(targetPos) && !mapGen.IsDestructible(targetPos))
        {
            gridPosition = targetPos;
            transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        }
    }
}
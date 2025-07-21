using UnityEngine;

public class SimpleEnemy: IEnemy
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            gameManager.AddScore(100);
            gameManager.OnEnemyKilled(this);
            StartCoroutine(DestroySequence());
        }
    }
}
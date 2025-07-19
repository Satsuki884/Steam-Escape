using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    public float fuseTime = 2f;
    public GameObject explosionPrefab;

    private LevelGenerator mapGen;
    private GameManager gameManager;

    void Start()
    {
        mapGen = FindObjectOfType<LevelGenerator>();
        gameManager = FindObjectOfType<GameManager>();

        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        Vector2Int bombPos = Vector2Int.RoundToInt(transform.position);
        CreateExplosionAt(bombPos);

        // Взрыв во все 4 стороны с дальностью 1
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = bombPos + dir;

            if (mapGen.IsIndestructible(checkPos))
            {
                // Взрыв остановлен стеной
                continue;
            }

            CreateExplosionAt(checkPos);

            if (mapGen.IsDestructible(checkPos))
            {
                mapGen.DestroyBlock(checkPos);
                gameManager.AddScore(50);
            }
        }

        Destroy(gameObject);
    }

    void CreateExplosionAt(Vector2Int pos)
    {
        Instantiate(explosionPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
    }
}

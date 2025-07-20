using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    public float fuseTime = 2f;
    public GameObject explosionPrefab;

    private LevelGenerator mapGen;
    private GameManager gameManager;

    private PlayerController owner;
    private int bombRange = 1;

    public void SetOwner(PlayerController player)
    {
        owner = player;
    }

    public void SetBombRange(int range)
    {
        bombRange = range;
    }

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

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= bombRange; i++)
            {
                Vector2Int checkPos = bombPos + dir * i;

                if (mapGen.IsIndestructible(checkPos))
                {
                    // Взрыв останавливается на несокрушимой стене
                    break;
                }

                CreateExplosionAt(checkPos);

                if (mapGen.IsDestructible(checkPos))
                {
                    mapGen.DestroyBlock(checkPos);
                    gameManager.AddScore(50);
                    // Взрыв не распространяется дальше за разрушимым блоком
                    break;
                }
            }
        }

        if (owner != null)
            owner.OnBombExploded();

        Destroy(gameObject);
    }

    void CreateExplosionAt(Vector2Int pos)
    {
        Instantiate(explosionPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
    }
}

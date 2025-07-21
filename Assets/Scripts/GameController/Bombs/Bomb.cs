using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float fuseTime = 4f;
    [SerializeField] private GameObject explosionPrefab;

    private LevelGenerator mapGen;
    private GameManager gameManager;

    private PlayerController owner;
    private int bombRange = 1;

    private Renderer bombRenderer;
    private Color originalColor;

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

        bombRenderer = GetComponentInChildren<Renderer>();
        if (bombRenderer != null)
        {
            originalColor = bombRenderer.material.color;
        }

        StartCoroutine(Countdown());
        StartCoroutine(Pulsate());
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(fuseTime/2f+fuseTime/4f);
        StartCoroutine(ChangeColorToRed());

        yield return new WaitForSeconds(fuseTime/4f);
        Explode();
    }

    IEnumerator Pulsate()
    {
        float time = 0f;
        Vector3 baseScale = transform.localScale;

        while (true)
        {
            time += Time.deltaTime * 5f;
            float scale = 1f + Mathf.Sin(time) * 0.1f;
            transform.localScale = baseScale * scale;
            yield return null;
        }
    }

    IEnumerator ChangeColorToRed()
    {
        if (bombRenderer == null) yield break;

        float duration = 1f;
        float elapsed = 0f;
        Color targetColor = Color.red;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bombRenderer.material.color = Color.Lerp(originalColor, targetColor, elapsed / duration);
            yield return null;
        }

        bombRenderer.material.color = targetColor;
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
        AudioManager.Instance.PlaySFX("explosionSound");
        Instantiate(explosionPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
    }
}

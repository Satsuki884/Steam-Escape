using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int width = 15;
    public int height = 15;

    public GameObject indestructibleWallPrefab;
    public GameObject destructibleBlockPrefab;
    public List<GameObject> bonusPrefabs;
    public GameObject playerPrefab;
    public GameObject gearPrefab;
    public GameObject exitPrefab;
    private Vector2Int exitPosition;

    private GameObject[,] mapTiles;

    private List<Vector2Int> destructiblePositions = new List<Vector2Int>();

    private Dictionary<Vector2Int, GameObject> hiddenObjects = new Dictionary<Vector2Int, GameObject>();


    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (mapTiles != null)
        {
            foreach (var tile in mapTiles)
                if (tile != null)
                    Destroy(tile);
        }

        mapTiles = new GameObject[width, height];
        destructiblePositions.Clear();
        hiddenObjects.Clear();

        Vector2Int spawnPos = new Vector2Int(1, height - 2); // Верхний левый угол (внутри границ)
        List<Vector2Int> blockedAroundSpawn = GetSurroundingPositions(spawnPos);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                Vector3 pos = new Vector3(x, y, 0);

                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    mapTiles[x, y] = Instantiate(indestructibleWallPrefab, pos, Quaternion.identity, transform);
                    continue;
                }

                if (x % 2 == 0 && y % 2 == 0)
                {
                    mapTiles[x, y] = Instantiate(indestructibleWallPrefab, pos, Quaternion.identity, transform);
                    continue;
                }

                // Пропускаем установку разрушаемых блоков в радиусе 1 от спавна
                if (blockedAroundSpawn.Contains(currentPos))
                    continue;

                if (Random.value < 0.5f)
                {
                    mapTiles[x, y] = Instantiate(destructibleBlockPrefab, pos, Quaternion.identity, transform);
                    destructiblePositions.Add(currentPos);
                }
            }
        }

        SpawnGearsAndBonuses();
        SpawnExit();

        Instantiate(playerPrefab, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity);
    }

    List<Vector2Int> GetSurroundingPositions(Vector2Int center)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = center.x + dx;
                int ny = center.y + dy;

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    positions.Add(new Vector2Int(nx, ny));
                }
            }
        }

        return positions;
    }



    void SpawnGearsAndBonuses()
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>(destructiblePositions);
        ShuffleList(availablePositions);

        int totalGears = Random.Range(2, 6);
        int totalBonuses = Random.Range(2, 6);
        int index = 0;

        for (int i = 0; i < totalGears && index < availablePositions.Count; i++, index++)
        {
            Vector2Int pos = availablePositions[index];
            GameObject gear = Instantiate(gearPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform);
            gear.SetActive(false);
            hiddenObjects[pos] = gear;
        }

        for (int i = 0; i < totalBonuses && index < availablePositions.Count; i++, index++)
        {
            Vector2Int pos = availablePositions[index];
            GameObject bonus = Instantiate(bonusPrefabs[Random.Range(0, bonusPrefabs.Count)], new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform);
            bonus.SetActive(false);
            hiddenObjects[pos] = bonus;
        }
    }


    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }


    void SpawnExit()
    {
        if (destructiblePositions.Count == 0) return;

        exitPosition = destructiblePositions[Random.Range(0, destructiblePositions.Count)];
        GameObject exitGO = Instantiate(exitPrefab, new Vector3(exitPosition.x, exitPosition.y, 0), Quaternion.identity, transform);
        exitGO.SetActive(false);
        hiddenObjects[exitPosition] = exitGO;
    }



    public void OnDestructibleDestroyed(Vector2Int pos)
    {
        if (hiddenObjects.ContainsKey(pos))
        {
            hiddenObjects[pos].SetActive(true);
            hiddenObjects.Remove(pos);
        }
    }

    public bool IsIndestructible(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return true; // За пределами - считаем стеной

        if (mapTiles[pos.x, pos.y] == null)
            return false;

        // Проверяем теги
        var tile = mapTiles[pos.x, pos.y];
        return tile.CompareTag("IndestructibleWall");
    }

    public bool IsDestructible(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return false;

        if (mapTiles[pos.x, pos.y] == null)
            return false;

        var tile = mapTiles[pos.x, pos.y];
        return tile.CompareTag("DestructibleBlock");
    }

    public void DestroyBlock(Vector2Int pos)
    {
        if (IsDestructible(pos))
        {
            Destroy(mapTiles[pos.x, pos.y]);
            mapTiles[pos.x, pos.y] = null;
            OnDestructibleDestroyed(pos);
        }
    }
}

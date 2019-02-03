using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    // https://www.redblobgames.com/grids/hexagons/
    public GameObject HexagonPrefab;
    public MapShape MapShape = MapShape.Rectangle;
    public Vector2Int MapSize = new Vector2Int(5, 5);
    public Material[] Materials;
    public GameObject[] Objects;
    public Material OutlineMaterial;

    private Dictionary<string, List<GameObject>> grid = new Dictionary<string, List<GameObject>>();

    public void GenerateGrid()
    {
        ClearGrid();

        switch (MapShape)
        {
            case MapShape.Hexagon:
                GenHexGrid();
                break;
            case MapShape.Rectangle:
                GenRectGrid();
                break;
            case MapShape.Parrallelogram:
                GenParrallGrid();
                break;
            case MapShape.Triangle:
                GenTriGrid();
                break;
        }
    }

    void GenHexGrid()
    {
        Tile tile;
        Vector3 pos = Vector3.zero;

        int mapSize = Mathf.Max(MapSize.x, MapSize.y);

        for (int q = -mapSize; q <= mapSize; q++)
        {
            int r1 = Mathf.Max(-mapSize, -q - mapSize);
            int r2 = Mathf.Min(mapSize, -q + mapSize);
            for (int r = r1; r <= r2; r++)
            {
                pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                pos.z = Const.hexRadius * 3.0f / 2.0f * r;

                GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                go.name = "Hex[" + q + "," + r + "," + (-q - r).ToString() + "]";
                go.transform.parent = transform;

                tile = go.AddComponent<Tile>();
                tile.index = new CubeIndex(q, r, -q - r);

                List<GameObject> tileObjects = new List<GameObject>();
                tileObjects.Add(go);

                grid.Add(tile.index.ToString(), tileObjects);
            }
        }
    }

    void GenRectGrid()
    {
        int mapWidth = MapSize.x;
        int mapHeight = MapSize.y;

        Tile tile;
        Vector3 pos = Vector3.zero;
        for (int r = 0; r < mapHeight; r++)
        {
            int rOff = r >> 1; // /2
            for (int q = -rOff; q < mapWidth - rOff; q++)
            {
                pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                pos.z = Const.hexRadius * 3.0f / 2.0f * r;

                GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                go.name = "Hex[" + q + "," + r + "," + (-q - r).ToString() + "]";
                go.transform.parent = transform;
                // go.transform.localScale = new Vector3(1, Random.Range(1f, 4f), 1);

                tile = go.AddComponent<Tile>();
                tile.index = new CubeIndex(q, r, -q - r);

                List<GameObject> tileObjects = new List<GameObject>();
                tileObjects.Add(go);

                grid.Add(tile.index.ToString(), tileObjects);
            }
        }
    }

    void ClearLines()
    {
        foreach (var entry in grid)
        {
            if (entry.Value != null)
            {
                DestroyImmediate(Const.GetTileFromGameObjects(entry.Value).GetComponent<LineRenderer>(), false);
            }
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Tile tile = Const.GetTileFromWorldPosition(hit.point, grid);
            if (tile != null)
            {
                ClearLines();
                Const.DrawOutline(tile.gameObject, OutlineMaterial);
            }
        }
    }

    void GenParrallGrid()
    {
        Tile tile;
        Vector3 pos = Vector3.zero;

        for (int q = 0; q <= MapSize.x; q++)
        {
            for (int r = 0; r <= MapSize.y; r++)
            {
                pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                pos.z = Const.hexRadius * 3.0f / 2.0f * r;

                GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                go.name = "Hex[" + q + "," + r + "," + (-q - r).ToString() + "]";
                go.transform.parent = transform;
                // go.transform.localScale = new Vector3(1, Random.Range(1f, 4f), 1);

                tile = go.AddComponent<Tile>();
                tile.index = new CubeIndex(q, r, -q - r);

                List<GameObject> tileObjects = new List<GameObject>();
                tileObjects.Add(go);

                grid.Add(tile.index.ToString(), tileObjects);
            }
        }
    }

    void GenTriGrid()
    {
        Tile tile;
        Vector3 pos = Vector3.zero;

        int mapSize = Mathf.Max(MapSize.x, MapSize.y);

        for (int q = 0; q <= mapSize; q++)
        {
            for (int r = 0; r <= mapSize - q; r++)
            {
                pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                pos.z = Const.hexRadius * 3.0f / 2.0f * r;

                GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                go.name = "Hex[" + q + "," + r + "," + (-q - r).ToString() + "]";
                go.transform.parent = transform;
                // go.transform.localScale = new Vector3(1, Random.Range(1f, 4f), 1);

                tile = go.AddComponent<Tile>();
                tile.index = new CubeIndex(q, r, -q - r);

                List<GameObject> tileObjects = new List<GameObject>();
                tileObjects.Add(go);

                grid.Add(tile.index.ToString(), tileObjects);
            }
        }
    }

    public void ClearGrid()
    {
        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            DestroyImmediate(child.gameObject);
        }

        grid.Clear();
    }

    public void LoadGrid(string path)
    {
        ClearGrid();

        TextReader tr = new StreamReader(path);

        string line;
        Tile tile;
        Vector3 pos = Vector3.zero;
        while ((line = tr.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            if (items.Length > 2)
            {
                // parse tile
                int tileX = int.Parse(items[0]);
                int tileZ = int.Parse(items[1]);
                float tileScaleY = float.Parse(items[2]);
                int tileMaterial = int.Parse(items[3]);

                // creat tile
                CubeIndex index = new CubeIndex(tileX, tileZ);
                pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (index.x + index.y / 2.0f);
                pos.z = Const.hexRadius * 3.0f / 2.0f * index.y;

                GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                go.name = "Hex[" + index.x + "," + index.y + "," + index.z + "]";
                go.transform.parent = transform;
                go.transform.localScale = new Vector3(1, tileScaleY, 1);

                tile = go.AddComponent<Tile>();
                tile.index = index;

                Const.GetMeshFromTile(tile).material = Materials[tileMaterial];

                // in case it's water change its layer to unwalkable.
                if (tileMaterial == 2)
                {
                    go.layer = 9; // unwalkable
                }

                List<GameObject> tileObjects = new List<GameObject>();
                tileObjects.Add(go);

                // parse objects
                int numObjects = (items.Length - 4) / 6;
                for (int i = 0; i < numObjects; i++)
                {
                    int start = 4 + i * 6;
                    int objectId = int.Parse(items[start]);
                    Vector3 position = new Vector3(float.Parse(items[start + 1]),
                                                    float.Parse(items[start + 2]),
                                                    float.Parse(items[start + 3]));
                    float rotationY = float.Parse(items[start + 4]);
                    float scale = float.Parse(items[start + 5]);

                    // create object
                    go = Instantiate(Objects[objectId], position, Quaternion.Euler(0, rotationY, 0));

                    // in case it's a tree or rock on top make tile unwalkable.
                    if (objectId == 0 || objectId == 1 || objectId == 2 || objectId == 3)
                    {
                        tile.gameObject.layer = 9;
                    }

                    go.transform.localScale *= scale;
                    go.transform.parent = transform;
                    tileObjects.Add(go);
                }

                // add to grid
                grid.Add(index.ToString(), tileObjects);
            }
        }

        FindObjectOfType<UnityEngine.AI.NavMeshSurface>();

        // HighlightWalkable();
    }

    void HighlightWalkable()
    {
        foreach(var entry in grid)
        {
            List<GameObject> gameObjects = entry.Value;

            var tile = gameObjects[0].GetComponent<Tile>();
            Renderer mesh = Const.GetMeshFromTile(tile);

            // check if it's water material.
            bool walkable = !mesh.sharedMaterial.name.Contains(Materials[2].name);
            // if it was not water, check if there is a tree.
            if (walkable)
            {
                for (int i = 1; i < gameObjects.Count; i++)
                {
                    string gameObjectName = gameObjects[i].name;
                    
                    if (gameObjectName.Contains("BigTree") || gameObjectName.Contains("PineTree"))
                    {
                        walkable = false;
                        break;
                    }
                }
            }

            tile.walkable = walkable;

            if (walkable)
            {
                Const.DrawOutline(tile.gameObject, OutlineMaterial);
            }
        }

        List<Tile> visitedTiles = new List<Tile>();
        Tile randTile = grid.ElementAt(Random.Range(0, grid.Count)).Value[0].GetComponent<Tile>();
        
        while(visitedTiles.Count < grid.Count)
        {
            visitedTiles.Add(randTile);

            List<Tile> neighbours = Const.Neighbours(randTile, grid);
            foreach (Tile neighbour in neighbours)
            {
                if (randTile.walkable && neighbour.walkable
                    && Mathf.Abs(randTile.transform.localScale.y - neighbour.transform.localScale.y) < 0.5f)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = transform;
                    go.transform.position = randTile.transform.position;
                    LineRenderer lr = go.AddComponent<LineRenderer>();
                    lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
                    lr.startColor = Color.white;
                    lr.endColor = Color.white;
                    lr.SetPosition(0, randTile.transform.position);
                    lr.SetPosition(1, neighbour.transform.position);
                }
            }
        }
        // how to recurse in the grid from a start position?
        // start a tile...set walkable to true or false.
        // look at neighbour tiles...set walkable to true or false.
        // if origin tile.walkable and neighbour tile.walkable and similar height, draw connection
        // check neighbours of neighbour until visited neighbours == grid.count
    }
}

[System.Serializable]
public enum MapShape
{
    Rectangle,
    Hexagon,
    Parrallelogram,
    Triangle
}

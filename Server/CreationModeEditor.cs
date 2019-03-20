using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationModeEditor : MonoBehaviour {

    public GameObject HexagonPrefab;
    public Material[] Materials;
    public GameObject[] Objects;
    public CreationModeGUI GUI;
    public Material OutlineMaterial;

    // Key: Tile index and Value: list of game objects like Tile at 0, and other game objects.
    private Dictionary<string, List<GameObject>> grid = new Dictionary<string, List<GameObject>>();

    private bool clear = false;
    private int range = 0;
    private float addToScaleUponElevation = 0.1f;
    private float waterScaleY = 0.7f;
    private Vector2Int GridSizeLimits = new Vector2Int(10, 40);
    private List<LoadTile> gridToBeLoaded = new List<LoadTile>();

    public void SetRange(float value)
    {
        range = (int)(value * 10);
    }

    public int GetRange()
    {
        return range;
    }
    
    public Dictionary<string, List<GameObject>> GetGrid()
    {
        return grid;
    }

    bool IsThereBigTreeAround(Tile tile)
    {
        List<Tile> neighbours = Const.TilesInRange(tile, 1, grid);
        foreach (var neighbour in neighbours)
        {
            List<GameObject> gameObjects = grid[neighbour.index.ToString()];
            foreach (var gameObject in gameObjects)
            {
                if (gameObject.name.Contains("BigTree"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool AreThereGameObjectsInTile(Tile tile)
    {
        List<GameObject> gameObjects = grid[tile.index.ToString()];

        // by default there is only 1 tile object.
        return gameObjects.Count > 1;
    }

    bool AreThereGameObjectsAroundTile(Tile tile)
    {
        List<Tile> neighbours = Const.TilesInRange(tile, 1, grid);
        foreach (var neighbour in neighbours)
        {
            if (grid[neighbour.index.ToString()].Count > 1) {
                return true;
            }
        }

        return false;
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

    void ApplyElevationToTile(Tile tile)
    {
        Vector3 scale = tile.transform.localScale;
        bool holdingAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        scale.y = Mathf.Clamp(scale.y + (holdingAlt ? -addToScaleUponElevation : addToScaleUponElevation), 1, 10);
        tile.transform.localScale = scale;

        // elevate the objects on top.
        List<GameObject> gameObjects = grid[tile.index.ToString()];
        foreach (var gameObject in gameObjects)
        {
            // not the title itself.
            if (!gameObject.GetComponent<Tile>())
            {
                Vector3 pos = gameObject.transform.position;
                pos.y = 2 * tile.GetComponentInChildren<MeshRenderer>().bounds.extents.y + tile.transform.position.y;
                gameObject.transform.position = pos;
            }
        }
    }

    void ApplyMaterialToTile(Tile tile, bool applyRandomGeneration)
    {
        Renderer renderer = Const.GetMeshFromTile(tile);

        // if changing to a different material, set applyRandomGeneration flag to true, and apply the change immediately.
        if (!renderer.material.name.Contains(Materials[GUI.GetSelectedMaterial()].name))
        {
            applyRandomGeneration = true;
        }

        if (!applyRandomGeneration)
        {
            return;
        }

        EraseTileObjects(tile);

        Vector3 scale = tile.transform.localScale;

        // water
        if (GUI.GetSelectedMaterial() == 2)
        {
            scale.y = waterScaleY;
        }
        // if changing from water set scale y to 1.
        else if (renderer.material.name.Contains(Materials[2].name))
        {
            scale.y = 1f;
        }

        tile.transform.localScale = scale;
        renderer.material = Materials[GUI.GetSelectedMaterial()];

        // add foliage to tiles.
        // make sure there are no gameobjects on the tile already.
        // in grass or mud.
        if (grid[tile.index.ToString()].Count == 1 && (GUI.GetSelectedMaterial() == 0 || GUI.GetSelectedMaterial() == 1))
        {
            GameObject go = null;

            if (!IsThereBigTreeAround(tile))
            {
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                // 20% it's a tree on grass
                if (Random.value < 0.2f && GUI.GetSelectedMaterial() == 0)
                {
                    if (Random.value < 0.25f)
                    {
                        if (!AreThereGameObjectsAroundTile(tile))
                        {
                            // big tree
                            go = Instantiate(Objects[0], tile.transform.position, randomRotation);
                        }
                    }
                    else
                    {
                        // pine tree
                        go = Instantiate(Objects[1], tile.transform.position, randomRotation);
                    }
                }
                // 15 % it's a stone on grass or mud.
                else if (Random.value < 0.15f)
                {
                    if (Random.value < 0.5f)
                    {
                        // rock 1
                        go = Instantiate(Objects[2], tile.transform.position, randomRotation);
                    }
                    else
                    {
                        // rock 2
                        go = Instantiate(Objects[3], tile.transform.position, randomRotation);
                    }
                }
            }
            
            if (go != null)
            {
                // put on top of underlying tile.
                Vector3 pos = go.transform.position;
                pos.y = 2 * tile.GetComponentInChildren<MeshRenderer>().bounds.extents.y + tile.transform.position.y;
                go.transform.position = new Vector3(Random.Range(pos.x - 0.2f, pos.x + 0.2f), pos.y, Random.Range(pos.z - 0.2f, pos.z + 0.2f));
                go.transform.localScale *= Random.Range(0.8f, 1.2f);
                go.transform.parent = transform;
                grid[tile.index.ToString()].Add(go);
            }
        }
    }

    void EraseTileObjects(Tile tile)
    {
        List<GameObject> gameObjects = grid[tile.index.ToString()];
        
        // don't delete the tile itself
        for (int i = gameObjects.Count - 1; i > 0; i--) {
            Destroy(gameObjects[i]);
            grid[tile.index.ToString()].RemoveAt(i);
        }
    }

    void AddGameplayObject(Tile tile)
    {
        Renderer renderer = Const.GetMeshFromTile(tile);

        // not on water.
        if (grid[tile.index.ToString()].Count == 1 && !renderer.material.name.Contains(Materials[2].name))
        {
            int gameplayObjectIndex = Const.GameplayObjectStartIndex + GUI.GetSelectedGameplayObject();
            GameObject go = Instantiate(Objects[gameplayObjectIndex], tile.transform.position, Quaternion.identity);
            Vector3 pos = go.transform.position;
            pos.y = 2 * tile.GetComponentInChildren<MeshRenderer>().bounds.extents.y + tile.transform.position.y;
            go.transform.position = pos;
            go.transform.parent = transform;
            grid[tile.index.ToString()].Add(go);
        }
    }
    
    float timeToResetRandomGeneration = 0.7f;
    float randomGenerationTimer = 0f;

    bool ShouldApplyRandomGeneration()
    {
        randomGenerationTimer += Time.deltaTime;
        if (randomGenerationTimer >= timeToResetRandomGeneration)
        {
            randomGenerationTimer = 0f;
            return true;
        }

        return false;
    }

    void Update() {
        if (!GUI.GetIsInteractingWithUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Tile hitTile = hit.transform.GetComponent<Tile>();

                if (hitTile != null)
                {
                    ClearLines();

                    List<Tile> tilesInRange = Const.TilesInRange(hitTile, range, grid);
                    
                    bool applyRandomGeneration = ShouldApplyRandomGeneration();

                    foreach (Tile tile in tilesInRange)
                    {
                        Const.DrawOutline(tile.gameObject, OutlineMaterial);

                        if (Input.GetMouseButton(0))
                        {
                            Renderer renderer = Const.GetMeshFromTile(tile);

                            // elevate the tiles as long as it's not water tile.
                            if (GUI.GetIsElevationSelected() && !renderer.material.name.Contains(Materials[2].name))
                            {
                                ApplyElevationToTile(tile);
                            }
                            else if (GUI.GetSelectedMaterial() > -1)
                            {
                                ApplyMaterialToTile(tile, applyRandomGeneration);
                            }
                            else if (GUI.GetIsEraseSelected())
                            {
                                EraseTileObjects(tile);
                            }
                            else if (GUI.GetSelectedGeneralObject() > -1)
                            {
                                Debug.Log("Place general object.");
                            }
                            else if (GUI.GetSelectedGameplayObject() > -1)
                            {
                                AddGameplayObject(tile);
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnSizeChanged(float value)
    {
        ClearGrid();

        int size = (int)(value * (GridSizeLimits.y - GridSizeLimits.x)) + GridSizeLimits.x;

        StartCoroutine(SlowSpawn(size));
    }
    
    public void QueueTileToLoad(LoadTile loadTile)
    {
        gridToBeLoaded.Add(loadTile);
    }

    public void LoadLevel()
    {
        StartCoroutine(SlowLoadLevel());
    }

    IEnumerator SlowLoadLevel()
    {
        Tile tile;
        Vector3 pos = Vector3.zero;
        int count = 0;

        foreach (var loadTile in gridToBeLoaded)
        {
            CubeIndex index = new CubeIndex(loadTile.tileX, loadTile.tileZ);
            if (!grid.ContainsKey(index.ToString()))
            {
                pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (index.x + index.y / 2.0f);
                pos.z = Const.hexRadius * 3.0f / 2.0f * index.y;

                GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                go.name = "Hex[" + index.x + "," + index.y + "," + index.z + "]";
                go.transform.parent = transform;
                go.transform.localScale = new Vector3(1, loadTile.tileScaleY, 1);
                
                tile = go.AddComponent<Tile>();
                tile.index = index;

                Const.GetMeshFromTile(tile).material = Materials[loadTile.tileMaterial];

                List<GameObject> tileObjects = new List<GameObject>();
                tileObjects.Add(go);

                foreach (var loadGameObject in loadTile.gameObjects)
                {
                    go = Instantiate(Objects[loadGameObject.objectId], loadGameObject.position, Quaternion.Euler(0, loadGameObject.rotationY, 0));
                    go.transform.localScale *= loadGameObject.scale;
                    go.transform.parent = transform;
                    tileObjects.Add(go);
                }

                grid.Add(index.ToString(), tileObjects);
            }

            count++;
            // spawn 50 tiles with objects per frame.
            if (count > 50)
            {
                count = 0;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator SlowSpawn(int size)
    {
        Tile tile;
        Vector3 pos = Vector3.zero;

        for (int q = -size; q <= size; q++)
        {
            int r1 = Mathf.Max(-size, -q - size);
            int r2 = Mathf.Min(size, -q + size);
            for (int r = r1; r <= r2; r++)
            {
                CubeIndex index = new CubeIndex(q, r, -q - r);
                if (!grid.ContainsKey(index.ToString()))
                {
                    pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                    pos.z = Const.hexRadius * 3.0f / 2.0f * r;

                    GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                    go.name = "Hex[" + q + "," + r + "," + (-q - r).ToString() + "]";
                    go.transform.parent = transform;
                    // go.transform.localScale = new Vector3(1, Random.Range(1f, 4f), 1);

                    tile = go.AddComponent<Tile>();
                    tile.index = index;

                    List<GameObject> tileObjects = new List<GameObject>();
                    tileObjects.Add(go);

                    grid.Add(index.ToString(), tileObjects);
                }
            }

            if (clear)
            {
                yield break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void ClearGrid()
    {
        clear = true;
        Tile[] tiles = FindObjectsOfType<Tile>();

        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
                if (grid.ContainsKey(tile.index.ToString()))
                {
                    foreach (GameObject go in grid[tile.index.ToString()])
                    {
                        Destroy(go);
                    }

                    grid[tile.index.ToString()].Clear();
                }
            }
        }

        grid.Clear();
        clear = false;
    }
}

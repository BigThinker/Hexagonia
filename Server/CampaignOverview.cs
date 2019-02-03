using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class CampaignOverview : MonoBehaviour {

    public GameObject HexagonPrefab;
    public GameObject PlusHexagonPrefab;
    public Material OutlineMaterial;
    public CampaignOverviewGUI GUI;

    private Dictionary<string, List<GameObject>> grid = new Dictionary<string, List<GameObject>>();
    private Tile firstSelection;
    private Tile secondSelection;
    private DirectoryInfo dirInfo;

    private List<Tile> reorderedTiles = new List<Tile>();
    private int currentOrder = -1;

    void Start () {
        // Global.CurrentCampaign = "Campaign2";

        if (Global.CurrentCampaign != "")
        {
            if (Application.isEditor)
            {
                dirInfo = new DirectoryInfo(Application.dataPath + "/Campaigns/Offline/" + Global.CurrentCampaign);
            }
            else
            {
                dirInfo = new DirectoryInfo(Application.persistentDataPath + "/Campaigns/Offline/" + Global.CurrentCampaign);
            }

            FileInfo[] files = dirInfo.GetFiles("*.*");
            Tile tile;
            Vector3 pos = Vector3.zero;
            Global.CurrentLevelOrder = -1;

            foreach (FileInfo file in files)
            {
                if (file.Name.EndsWith(Const.LevelFileExtension))
                {
                    string content = File.ReadAllText(file.FullName);
                    string[] nParts = content.Split('\n');
                    int order = int.Parse(nParts[1]);
                    string[] commaParts = nParts[2].Split(',');
                    int tileX = int.Parse(commaParts[0]);
                    int tileY = int.Parse(commaParts[1]);

                    if (order > Global.CurrentLevelOrder)
                    {
                        Global.CurrentLevelOrder = order;
                    }

                    CubeIndex index = new CubeIndex(tileX, tileY, -tileX - tileY);

                    if (!grid.ContainsKey(index.ToString()))
                    {
                        pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (tileX + tileY / 2.0f);
                        pos.z = Const.hexRadius * 3.0f / 2.0f * tileY;

                        GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                        string fileName = file.Name.Substring(0, file.Name.Length - Const.LevelFileExtension.Length);
                        go.name = fileName;
                        Const.FindChildRecursive(go.transform, "Order").GetComponent<TextMeshProUGUI>().text = order.ToString();
                        Const.FindChildRecursive(go.transform, "LevelName").GetComponent<TextMeshProUGUI>().text = fileName;
                        go.transform.parent = transform;

                        tile = go.AddComponent<Tile>();
                        tile.index = index;

                        List<GameObject> tileObjects = new List<GameObject>();
                        tileObjects.Add(go);

                        grid.Add(tile.index.ToString(), tileObjects);
                    }
                }
            }

            // next one.
            Global.CurrentLevelOrder++;

            if (grid.Count > 1)
            {
                GUI.ShowReorder();
            }

            RefreshPlusHexagons();
        }
        else
        {
            Global.CurrentLevelOrder = 1;

            // when coming from creation mode -> new campaign...simply show a plus hexagon tile.
            CreateOnePlusHexagon();
        }
    }

    List<Tile> GetCampaignTiles()
    {
        List<Tile> campaignTiles = new List<Tile>();
        foreach(var entry in grid)
        {
            List<GameObject> gameObjects = entry.Value;
            Tile tile = gameObjects[0].GetComponent<Tile>();
            int tileX, tileY;
            if (!Const.ParseLevelName(tile.name, out tileX, out tileY))
            {
                campaignTiles.Add(tile);
            }
        }
        return campaignTiles;
    }

    void RefreshPlusHexagons()
    {
        ClearPlusHexagons();

        // add transparent tiles around them.
        List<Tile> campaignTiles = GetCampaignTiles();

        if (campaignTiles.Count == 0)
        {
            CreateOnePlusHexagon();
        }
        else
        {
            Tile tile;
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < campaignTiles.Count; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    CubeIndex o = campaignTiles[i].index + Const.directions[j];
                    if (!grid.ContainsKey(o.ToString()))
                    {
                        pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (o.x + o.y / 2.0f);
                        pos.z = Const.hexRadius * 3.0f / 2.0f * o.y;
                        GameObject go = Instantiate(PlusHexagonPrefab, pos, Quaternion.identity);
                        go.name = o.x + "," + o.y;
                        go.transform.parent = transform;

                        tile = go.AddComponent<Tile>();
                        tile.index = o;

                        List<GameObject> tileObjects = new List<GameObject>();
                        tileObjects.Add(go);

                        grid.Add(tile.index.ToString(), tileObjects);
                    }
                }
            }
        }
    }

    void CreateOnePlusHexagon()
    {
        CubeIndex index = new CubeIndex(0, 0, 0);
        GameObject go = Instantiate(PlusHexagonPrefab, Vector3.zero, Quaternion.identity);
        go.name = index.x + "," + index.y;
        go.transform.parent = transform;

        Tile tile = go.AddComponent<Tile>();
        tile.index = index;

        List<GameObject> tileObjects = new List<GameObject>();
        tileObjects.Add(go);

        grid.Add(tile.index.ToString(), tileObjects);
    }

    void ClearPlusHexagons()
    {
        List<string> keysToBeDeleted = new List<string>();
        foreach (var entry in grid)
        {
            List<GameObject> gameObjects = entry.Value;
            Tile tile = gameObjects[0].GetComponent<Tile>();
            int tileX, tileY;
            if (Const.ParseLevelName(tile.name, out tileX, out tileY))
            {
                keysToBeDeleted.Add(tile.index.ToString());
                Destroy(tile.gameObject);
            }
        }

        for (int i = 0; i < keysToBeDeleted.Count; i++)
        {
            grid.Remove(keysToBeDeleted[i]);
        }
    }

    public string GetSelectedLevelName()
    {
        foreach(var entry in grid)
        {
            if (entry.Value[0].GetComponent<LineRenderer>())
            {
                return entry.Value[0].name;
            }
        }

        return "";
    }

    public void StartReordering()
    {
        currentOrder = 1;
        reorderedTiles.Clear();

        DeselectTileSelections();
    }

    public void EndReordering()
    {
        ClearCampaignOutlines();
    }

    void DeselectTileSelections()
    {
        if (firstSelection != null)
        {
            Destroy(firstSelection.GetComponent<LineRenderer>());
            firstSelection = null;
        }
        
        if (secondSelection != null)
        {
            Destroy(secondSelection.GetComponent<LineRenderer>());
            secondSelection = null;
        }
    }

    void ClearCampaignOutlines()
    {
        List<Tile> campaignTiles = GetCampaignTiles();
        foreach(var tile in campaignTiles)
        {
            Destroy(tile.GetComponent<LineRenderer>());
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !GUI.GetIsInteractingWithUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Tile tile = hit.transform.GetComponent<Tile>();
                int tileX, tileY;

                // if 2 tiles are selected disable delete and edit.
                // if 1 or no tile is selected disable switch.
                if (tile != null)
                {
                    if (GUI.GetIsReorderActive())
                    {
                        if (!reorderedTiles.Contains(tile) && !Const.ParseLevelName(tile.name, out tileX, out tileY))
                        {
                            Const.FindChildRecursive(tile.transform, "Order").GetComponent<TextMeshProUGUI>().text = currentOrder.ToString();

                            // save to file.
                            string[] contentLines = File.ReadAllLines(dirInfo.FullName + "/" + tile.name + Const.LevelFileExtension);
                            contentLines[1] = currentOrder.ToString();
                            File.WriteAllLines(dirInfo.FullName + "/" + tile.name + Const.LevelFileExtension, contentLines);

                            // draw outline.
                            Const.DrawOutline(tile.gameObject, OutlineMaterial);

                            currentOrder++;
                            reorderedTiles.Add(tile);

                            if (reorderedTiles.Count == GetCampaignTiles().Count)
                            {
                                GUI.DeselectReorderButton();
                                EndReordering();
                            }
                        }
                    }
                    else
                    {
                        // selecting same tile again deselects it.
                        if (tile == firstSelection)
                        {
                            Destroy(firstSelection.GetComponent<LineRenderer>());
                            firstSelection = null;
                            // DebugTileSelection(tile, "1st=null");
                            CheckUI();
                        }
                        else if (tile == secondSelection)
                        {
                            Destroy(secondSelection.GetComponent<LineRenderer>());
                            secondSelection = null;
                            // DebugTileSelection(tile, "2nd=null");
                            CheckUI();
                        }
                        else
                        {
                            // plus hexagon tiles have their own logic.
                            if (Const.ParseLevelName(tile.name, out tileX, out tileY))
                            {
                                if (firstSelection != null)
                                {
                                    Destroy(firstSelection.GetComponent<LineRenderer>());
                                    // DebugTileSelection(firstSelection, "1st=null");
                                    firstSelection = null;
                                }
                                if (secondSelection != null)
                                {
                                    Destroy(secondSelection.GetComponent<LineRenderer>());
                                    // DebugTileSelection(secondSelection, "2nd=null");
                                    secondSelection = null;
                                }

                                firstSelection = tile;
                                Const.DrawOutline(tile.gameObject, OutlineMaterial);
                                CheckUI();
                            }
                            else
                            {
                                // if previous choice was a plus hexagon, clear it first because we can't select 2 tiles where one is plus hexagon.
                                if (firstSelection != null && Const.ParseLevelName(firstSelection.name, out tileX, out tileY))
                                {
                                    Destroy(firstSelection.GetComponent<LineRenderer>());
                                    firstSelection = null;
                                }

                                if (firstSelection != null && secondSelection != null)
                                {
                                    Destroy(secondSelection.GetComponent<LineRenderer>());
                                    // DebugTileSelection(secondSelection, "2nd=null");
                                    secondSelection = null;
                                }

                                // there is already a first selection, so this must be the 2nd selection.
                                if (firstSelection != null && secondSelection == null)
                                {
                                    secondSelection = tile;
                                    Const.DrawOutline(tile.gameObject, OutlineMaterial);
                                    // DebugTileSelection(tile, "2nd");
                                    CheckUI();
                                }
                                // don't select a first tile if 2nd is also selected and it's a plus hexagon.
                                else if (firstSelection == null)
                                {
                                    firstSelection = tile;
                                    Const.DrawOutline(tile.gameObject, OutlineMaterial);
                                    // DebugTileSelection(tile, "1st");
                                    CheckUI();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void DebugTileSelection(Tile tile, string text)
    {
        TextMeshProUGUI tmpro = Const.FindChildRecursive(tile.transform, "LevelName").GetComponent<TextMeshProUGUI>();
        if (tmpro)
        {
            string[] nParts = tmpro.text.Split('\n');
            tmpro.text = nParts[0] + "\n" + text;
        }
    }

    void CheckUI()
    {
        if (firstSelection == null && secondSelection == null)
        {
            // hide ui.
            GUI.HideUI();
        }
        else if ((firstSelection != null && secondSelection == null)
                || (firstSelection == null && secondSelection != null))
        {
            // show edit and possibly delete if it's not a plus hexagon tile.
            GUI.HideUI();
            GUI.ShowEdit();

            int tileX, tileY;
            if ((firstSelection != null && !Const.ParseLevelName(firstSelection.name, out tileX, out tileY) && CanDelete(firstSelection))
                || (secondSelection != null && !Const.ParseLevelName(secondSelection.name, out tileX, out tileY) && CanDelete(secondSelection)))
            {
                GUI.ShowDelete();
            }
        }
        else
        {
            GUI.HideUI();
            GUI.ShowSwitch();
        }
    }

    public void DeleteSelectedTile()
    {
        int tileX, tileY;
        if (firstSelection != null && !Const.ParseLevelName(firstSelection.name, out tileX, out tileY) && CanDelete(firstSelection))
        {
            Delete(firstSelection);
        }
        else if (secondSelection != null && !Const.ParseLevelName(secondSelection.name, out tileX, out tileY) && CanDelete(secondSelection))
        {
            Delete(secondSelection);
        }
    }

    void Delete(Tile tile)
    {
        File.Delete(dirInfo.FullName + "/" + tile.name + Const.LevelFileExtension);
        grid.Remove(tile.index.ToString());
        Destroy(tile.gameObject);
        RefreshPlusHexagons();
        GUI.HideUI();
    }

    public void Switch()
    {
        if (firstSelection != null && secondSelection != null)
        {
            Vector3 pos = firstSelection.transform.position;
            firstSelection.transform.position = secondSelection.transform.position;
            secondSelection.transform.position = pos;

            // change the files themselves.
            string[] content1Lines = File.ReadAllLines(dirInfo.FullName + "/" + firstSelection.name + Const.LevelFileExtension);
            string[] content2Lines = File.ReadAllLines(dirInfo.FullName + "/" + secondSelection.name + Const.LevelFileExtension);
            string temp = content1Lines[2];
            content1Lines[2] = content2Lines[2];
            content2Lines[2] = temp;
            File.WriteAllLines(dirInfo.FullName + "/" + firstSelection.name + Const.LevelFileExtension, content1Lines);
            File.WriteAllLines(dirInfo.FullName + "/" + secondSelection.name + Const.LevelFileExtension, content2Lines);

            DeselectTileSelections();

            GUI.HideUI();
        }
    }

    bool CanDelete(Tile tile)
    {
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SimpleCampaignOverview : MonoBehaviour
{
    public GameObject HexagonPrefab;
    public Material OutlineMaterial;
    public Camera Camera;
    public Vector3 Offset;

    [HideInInspector]
    public List<Vector3> SpawnPoints;

    private Dictionary<string, List<GameObject>> grid = new Dictionary<string, List<GameObject>>();
    private DirectoryInfo dirInfo;

    public delegate void SpawnPointsParsed();
    public SpawnPointsParsed OnSpawnPointsParsed;

    void Start()
    {
        // Global.CurrentCampaign = "Campaign2";
        // Global.CurrentLevel = "Level1";

        Camera.transform.position += Offset;

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

            int smallestOrder = int.MaxValue;
            Tile smallestOrderTile = null;

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

                    CubeIndex index = new CubeIndex(tileX, tileY, -tileX - tileY);

                    if (!grid.ContainsKey(index.ToString()))
                    {
                        pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (tileX + tileY / 2.0f);
                        pos.z = Const.hexRadius * 3.0f / 2.0f * tileY;

                        GameObject go = Instantiate(HexagonPrefab, pos + Offset, Quaternion.identity);
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

                        if (order <= smallestOrder)
                        {
                            smallestOrderTile = tile;
                            
                            smallestOrder = order;
                        }
                    }
                }
            }

            if (smallestOrderTile != null)
            {
                Global.CurrentLevel = smallestOrderTile.name;
                Const.DrawOutline(smallestOrderTile.gameObject, OutlineMaterial);

                GetSpawnPoints();
            }
        }
    }

    public void GetSpawnPoints()
    {
        SpawnPoints = new List<Vector3>();
        TextReader tr;
        if (Application.isEditor)
        {
            tr = new StreamReader(Application.dataPath + "/Campaigns/Offline/"
                                + Global.CurrentCampaign + "/"
                                + Global.CurrentLevel + Const.LevelFileExtension);
        }
        else
        {
            tr = new StreamReader(Application.persistentDataPath + "/Campaigns/Offline/"
                                + Global.CurrentCampaign + "/"
                                + Global.CurrentLevel + Const.LevelFileExtension);
        }

        string line;
        while ((line = tr.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            if (items.Length > 4)
            {
                // parse objects
                int numObjects = (items.Length - 4) / 6;
                for (int i = 0; i < numObjects; i++)
                {
                    int start = 4 + i * 6;
                    int objectId = int.Parse(items[start]);
                    if (objectId == 4)
                    {
                        Vector3 position = new Vector3(float.Parse(items[start + 1]),
                                                    float.Parse(items[start + 2]),
                                                    float.Parse(items[start + 3]));
                        SpawnPoints.Add(position);
                    }
                }
            }
        }
        
        if (OnSpawnPointsParsed != null)
        {
            OnSpawnPointsParsed();
        }
    }
}

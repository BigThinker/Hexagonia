using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

public class PlayMode : MonoBehaviour
{
    public GameObject HexagonPrefab;
    public Material[] Materials;
    public GameObject[] Objects;
    public Material OutlineMaterial;
    public GameObject[] Characters;
    public GameObject GameMasterGiant;
    public Vector3 CameraOffset;
    public PlayModeGUI PlayModeGUI;

    // Key: Tile index and Value: list of game objects like Tile at 0, and other game objects.
    private Dictionary<string, List<GameObject>> grid = new Dictionary<string, List<GameObject>>();
    private Multiplayer multiPlayer;
    private CustomAnimatorDriverClient GameMasterGiantInst;

    void Start()
    {
        multiPlayer = FindObjectOfType<Multiplayer>();
    }
    
    public void InstantiatePlayers()
    {
        if (multiPlayer == null)
            return;

        foreach (var entry in multiPlayer.Players)
        {
            Player player = entry.Value;
            if (player.Character == null)
            {
                player.CharacterId = Mathf.Min(player.CharacterId, Characters.Length - 1);
                player.Character = Instantiate(Characters[player.CharacterId], player.SpawnPosition, Quaternion.identity);
            }
        }
    }

    void ClearTiles()
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));

        grid.Clear();
    }

    public void InstantiateTiles(TileDataMessage msg)
    {
        if (multiPlayer.LocalPlayer.Id == msg.Id)
        {
            ClearTiles();

            int i;
            Tile tile;
            Vector3 pos = Vector3.zero;

            // instantiate tiles.
            for (i = 0; i < msg.TilesXZ.Length / 2; i++)
            {
                int tileX = msg.TilesXZ[i * 2];
                int tileZ = msg.TilesXZ[i * 2 + 1];
                float tileScaleY = msg.TilesScaleY[i];
                int tileMaterial = msg.TilesMaterial[i];

                CubeIndex index = new CubeIndex(tileX, tileZ);
                if (!grid.ContainsKey(index.ToString()))
                {
                    pos.x = Const.hexRadius * Mathf.Sqrt(3.0f) * (index.x + index.y / 2.0f);
                    pos.z = Const.hexRadius * 3.0f / 2.0f * index.y;

                    GameObject go = Instantiate(HexagonPrefab, pos, Quaternion.identity);
                    go.name = "Hex[" + index.x + "," + index.y + "," + index.z + "]";
                    go.transform.parent = transform;
                    go.transform.localScale = new Vector3(1, tileScaleY, 1);

                    tile = go.AddComponent<Tile>();
                    tile.index = index;

                    Const.GetMeshFromTile(tile).material = Materials[tileMaterial];

                    List<GameObject> tileObjects = new List<GameObject>();
                    tileObjects.Add(go);

                    grid.Add(index.ToString(), tileObjects);
                }
            }

            // instantiate objects.
            for (i = 0; i < msg.ObjectsId.Length; i++)
            {
                int objectId = msg.ObjectsId[i];
                Vector3 position = msg.ObjectsPosition[i];
                float rotationY = msg.ObjectsRotationY[i];
                float scale = msg.ObjectsScale[i];

                GameObject go = Instantiate(Objects[objectId], position, Quaternion.Euler(0, rotationY, 0));
                go.transform.localScale *= scale;
                go.transform.parent = transform;

                // spawn points are not visible
                if (objectId == 4
                    || objectId == 5)
                {
                    go.SetActive(false);
                }

                tile = Const.GetTileFromWorldPosition(position, grid);

                // try to add to grid.
                if (tile != null && grid.ContainsKey(tile.index.ToString()))
                {
                    grid[tile.index.ToString()].Add(go);
                }
            }

            // make sure players are instantiated (after world has rendered).
            InstantiatePlayers();
        }
    }

    public void MovePlayer(MoveMessage msg)
    {
        Player player = multiPlayer.Players[msg.Id];
        if (player.Character != null)
        {
            player.LerpTimer = 0f;
            player.StartPosition = player.Character.transform.position;
            player.StartRotation = player.Character.transform.rotation;
            player.EndPosition = msg.Position;
            player.EndRotation = Quaternion.Euler(0, msg.RotationY, 0);
            LineRenderer lines = player.Character.GetComponent<LineRenderer>();
            lines.material.color = player.Color;
            lines.positionCount = msg.Path.Length;
            lines.SetPositions(msg.Path);

            // play animation.
            player.Character.GetComponent<Animator>().SetBool("Walking", (player.StartPosition - player.EndPosition).magnitude > 0);
        }
    }

    void Update()
    {
        if (multiPlayer != null)
        {
            if (multiPlayer.LocalPlayer != null && multiPlayer.LocalPlayer.Character != null)
            {
                Camera.main.transform.position = multiPlayer.LocalPlayer.Character.transform.position + CameraOffset;

                if (PlayModeGUI.ViewGameMasterToggle.isOn)
                {
                    Camera.main.transform.LookAt(Const.FindChildRecursive(GameMasterGiantInst.transform, "Head"));
                }
                else
                {
                    Camera.main.transform.LookAt(multiPlayer.LocalPlayer.Character.transform);
                }

                if (Input.GetMouseButtonDown(0) && !PlayModeGUI.GetIsInteractingWithUI())
                {
                    RaycastHit hit;

                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        multiPlayer.RequestMove(hit.point);
                    }
                }
            }

            // players lerp to destination (Extrapolation to fix 0.5s delay?).
            foreach(var entry in multiPlayer.Players)
            {
                Player player = entry.Value;
                if (player.Character != null)
                {
                    player.LerpTimer += Time.deltaTime;
                    player.Character.transform.position = Vector3.Lerp(player.StartPosition, player.EndPosition, player.LerpTimer / Const.NetMoveUpdateRate);
                    player.Character.transform.rotation = Quaternion.Slerp(player.StartRotation, player.EndRotation, player.LerpTimer / Const.NetMoveUpdateRate);
                }
            }
        }
    }

    public void SetGameMasterSpawnLocation(Vector3 SpawnLocation)
    {
        if (GameMasterGiantInst == null)
        {
            GameMasterGiantInst = Instantiate(GameMasterGiant, SpawnLocation, Quaternion.identity)
                                    .GetComponent<CustomAnimatorDriverClient>();
        }

        GameMasterGiantInst.transform.position = SpawnLocation;
    }

    public void ProcessMocap(byte[] data)
    {
        if (GameMasterGiantInst != null)
        {
            GameMasterGiantInst.ApplyMotion(Decompress(data));
        }
    }

    public static byte[] Decompress(byte[] data)
    {
        MemoryStream input = new MemoryStream(data);
        MemoryStream output = new MemoryStream();
        using (Unity.IO.Compression.DeflateStream dstream = 
                new Unity.IO.Compression.DeflateStream(input, Unity.IO.Compression.CompressionMode.Decompress))
        {
            CopyTo(dstream, output);
        }
        return output.ToArray();
    }

    public static void CopyTo(Stream input, Stream output)
    {
        byte[] buffer = new byte[16 * 1024];
        int bytesRead;
        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, bytesRead);
        }
    }
}

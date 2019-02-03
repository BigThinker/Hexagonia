using Neuron;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class PlayMode : MonoBehaviour
{
    public GameObject HexagonPrefab;
    public Material[] Materials;
    public GameObject[] Objects;
    public Material OutlineMaterial;
    public GameObject[] Characters;
    public GameObject GameMasterGiant;
    public NavMeshSurface NavMeshSurface;
    public PlayModeGUI PlayModeGUI;
    public delegate void LevelLoaded();
    public LevelLoaded OnLevelLoaded;
    [HideInInspector]
    public Vector3 GameMasterSpawnLocation = Vector3.zero;

    // Tile index as key and list of game objects like Tile at 0, and other game objects as value.
    private Dictionary<string, List<GameObject>> grid = new Dictionary<string, List<GameObject>>();
    private List<LoadTile> gridToBeLoaded = new List<LoadTile>();
    private Multiplayer multiPlayer;
    private float netMoveTimer = 0f;
    private CustomAnimatorDriver Mocap;
    // delta compression.
    private float[] prevMocapData = new float[NeuronActor.MaxFrameDataLength];

    void Start()
    {
        // Global.CurrentCampaign = "Campaign2";
        // Global.CurrentLevel = "Level1";

        if (Global.CurrentCampaign == "" || Global.CurrentLevel == "")
            return;

        multiPlayer = FindObjectOfType<Multiplayer>();

        LoadLevel();
        StartCoroutine(SlowSpawnLevel()); // and instantiate players at the end.
    }

    public bool LoadLevel()
    {
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
        int lineNumber = 0;
        while ((line = tr.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            if (lineNumber > 2)
            {
                // parse tile
                int tileX = int.Parse(items[0]);
                int tileZ = int.Parse(items[1]);
                float tileScaleY = float.Parse(items[2]);
                int tileMaterial = int.Parse(items[3]);

                LoadTile loadTile = new LoadTile(tileX, tileZ, tileScaleY, tileMaterial);

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

                    loadTile.AddGameObject(objectId, position, rotationY, scale);
                }

                gridToBeLoaded.Add(loadTile);
            }
            else if (lineNumber == 1)
            {
                // levelOrder = int.Parse(items[0]);
            }
            else if (lineNumber == 2)
            {
                // levelTileX = int.Parse(items[0]);
                // levelTileY = int.Parse(items[1]);
            }

            lineNumber++;
        }

        return false;
    }

    IEnumerator SlowSpawnLevel()
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

                if (loadTile.tileMaterial == 2)
                {
                    go.layer = 9; // unwalkable (water)
                }

                List<GameObject> tileObjects = new List<GameObject>();
                tileObjects.Add(go);

                foreach (var loadGameObject in loadTile.gameObjects)
                {
                    go = Instantiate(Objects[loadGameObject.objectId], loadGameObject.position, Quaternion.Euler(0, loadGameObject.rotationY, 0));
                    go.transform.localScale *= loadGameObject.scale;
                    go.transform.parent = transform;
                    tileObjects.Add(go);

                    if (loadGameObject.objectId == 0 
                        || loadGameObject.objectId == 1
                        || loadGameObject.objectId == 2
                        || loadGameObject.objectId == 3) {
                        tile.gameObject.layer = 9; // unwalkable (trees n' rocks)
                    }
                    else if (loadGameObject.objectId == 5)
                    {
                        GameMasterSpawnLocation = loadGameObject.position;
                    }

                    // spawn points are not visible
                    if (loadGameObject.objectId == 4
                        || loadGameObject.objectId == 5)
                    {
                        go.SetActive(false);
                    }
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

        GameMasterSpawnLocation.y = -19f; // only upper half appears.
        Mocap = Instantiate(GameMasterGiant, GameMasterSpawnLocation, Quaternion.identity)
                        .GetComponent<CustomAnimatorDriver>();

        var gameMasterSpawnLocationMessage = new GameMasterSpawnLocationMessage()
        {
            SpawnLocation = GameMasterSpawnLocation
        };
        NetworkServer.SendToAll(CustomMsg.GameMasterSpawnLocation, gameMasterSpawnLocationMessage);

        NavMeshSurface.BuildNavMesh();

        if (OnLevelLoaded != null)
        {
            OnLevelLoaded();
        }

        InstantiatePlayers();
    }
    
    public void InstantiatePlayers()
    {
        foreach (var entry in multiPlayer.Players)
        {
            Player player = entry.Value;
            if (player.Character == null)
            {
                player.CharacterId = Mathf.Min(player.CharacterId, Characters.Length - 1);
                player.Character = Instantiate(Characters[player.CharacterId], player.SpawnPosition, Quaternion.identity);
                player.CurrentTileIndex = Const.GetTileFromWorldPosition(player.SpawnPosition, grid).index;
            }
        }
    }

    public void SendTileDataToPlayers()
    {
        int totalTiles = 0;

        foreach (var entry in multiPlayer.Players)
        {
            Player player = entry.Value;

            if (player.Character != null)
            {
                var msg = new TileDataMessage();
                msg.Id = player.Id;

                Tile centerTile = Const.GetTileFromWorldPosition(player.Character.transform.position, grid);
                List<Tile> tilesInRange = Const.TilesInRange(centerTile, 6, grid);

                totalTiles += tilesInRange.Count;

                msg.TilesXZ = new int[2 * tilesInRange.Count];
                msg.TilesScaleY = new float[tilesInRange.Count];
                msg.TilesMaterial = new int[tilesInRange.Count];

                // loop to find out object count.
                int objectsCounter = 0;
                foreach (var tile in tilesInRange)
                {
                    List<GameObject> gameObjects = grid[tile.index.ToString()];
                    objectsCounter += gameObjects.Count - 1; // tile gameobject is at 0.
                }

                msg.ObjectsId = new int[objectsCounter];
                msg.ObjectsPosition = new Vector3[objectsCounter];
                msg.ObjectsRotationY = new float[objectsCounter];
                msg.ObjectsScale = new float[objectsCounter];

                tilesDataPerSecond += msg.TilesXZ.Length * sizeof(int)
                                    + msg.TilesScaleY.Length * sizeof(float)
                                    + msg.TilesMaterial.Length * sizeof(int)
                                    + msg.ObjectsId.Length * sizeof(int)
                                    + msg.ObjectsPosition.Length * 3 * sizeof(float)
                                    + msg.ObjectsRotationY.Length * sizeof(float)
                                    + msg.ObjectsScale.Length * sizeof(float);

                objectsCounter = 0;
                // loop to assign message data.
                for (int i = 0; i < tilesInRange.Count; i++)
                {
                    Tile tile = tilesInRange[i];

                    int material = -1;
                    string materialName = Const.GetMeshFromTile(tile).material.name;
                    if (materialName.Contains(Materials[0].name))
                    {
                        material = 0;
                    }
                    else if (materialName.Contains(Materials[1].name))
                    {
                        material = 1;
                    }
                    else if (materialName.Contains(Materials[2].name))
                    {
                        material = 2;
                    }

                    // Tile:
                    // tileX;
                    // tileZ;
                    // tileScaleY;
                    // tileMaterial;
                    msg.TilesXZ[i * 2] = tile.index.x;
                    msg.TilesXZ[i * 2 + 1] = tile.index.z;
                    msg.TilesScaleY[i] = tile.transform.localScale.y;
                    msg.TilesMaterial[i] = material;

                    List<GameObject> gameObjects = grid[tile.index.ToString()];
                    for (int j = 1; j < gameObjects.Count; j++)
                    {
                        GameObject go = gameObjects[j];
                        if (go != null)
                        {
                            int objectId = -1;
                            if (go.name.Contains("BigTree"))
                            {
                                objectId = 0;
                            }
                            else if (go.name.Contains("PineTree"))
                            {
                                objectId = 1;
                            }
                            else if (go.name.Contains("Rock01"))
                            {
                                objectId = 2;
                            }
                            else if (go.name.Contains("Rock02"))
                            {
                                objectId = 3;
                            }
                            else if (go.name.Contains("SpawnPoint"))
                            {
                                objectId = 4;
                            }
                            else if (go.name.Contains("GameMaster"))
                            {
                                objectId = 5;
                            }

                            // Game objects:
                            // objectId;
                            // position;
                            // rotationY;
                            // scale;
                            msg.ObjectsId[objectsCounter] = objectId;
                            msg.ObjectsPosition[objectsCounter] = go.transform.position;
                            msg.ObjectsRotationY[objectsCounter] = go.transform.rotation.eulerAngles.y;
                            msg.ObjectsScale[objectsCounter] = go.transform.localScale.x;

                            objectsCounter++;
                        }
                    }
                }

                if (sendTilesData)
                {
                    NetworkServer.SendToAll(CustomMsg.TileData, msg);

                    tilesDataLowerRatePerSecond += msg.TilesXZ.Length * sizeof(int)
                                                + msg.TilesScaleY.Length * sizeof(float)
                                                + msg.TilesMaterial.Length * sizeof(int)
                                                + msg.ObjectsId.Length * sizeof(int)
                                                + msg.ObjectsPosition.Length * 3 * sizeof(float)
                                                + msg.ObjectsRotationY.Length * sizeof(float)
                                                + msg.ObjectsScale.Length * sizeof(float);
                }
            }
        }

        if (updateUIPerSecond)
        {
            total += tilesDataPerSecond;
            totalOptimized += tilesDataLowerRatePerSecond;

            PlayModeGUI.SetTilesText(tilesDataPerSecond, totalTiles, multiPlayer.Players.Count);
            tilesDataPerSecond = 0;
            PlayModeGUI.SetTilesLowerRateText(tilesDataLowerRatePerSecond);
            tilesDataLowerRatePerSecond = 0;
        }

        sendTilesData = false;
    }

    // Debug data
    bool firstTimeSendingMocapData = true;
    int ticks = 0;
    float elapsed = 0f;
    bool updateUIPerSecond = false;
    bool sendTilesData = false;
    int mocapDataPerSecond = 0;
    int prevMocapDataPerSecond = 0;
    int mocapDeltaCompressedDataPerSecond = 0;
    int prevMocapDeltaCompressedDataPerSecond = 0;
    int mocapSystemDeflateCompressedDataPerSecond = 0;
    int prevMocapSystemDeflateCompressedDataPerSecond = 0;
    int mocapUnityDeflateCompressedDataPerSecond = 0;
    int prevMocapUnityDeflateCompressedDataPerSecond = 0;
    int tilesDataPerSecond = 0;
    int tilesDataLowerRatePerSecond = 0;
    int total = 0;
    int totalOptimized = 0;

    void Update()
    {
        ticks++;
        elapsed += Time.deltaTime;
        if (elapsed >= 1)
        {
            PlayModeGUI.SetFrameRateText(ticks);

            elapsed = 0;
            ticks = 0;
            updateUIPerSecond = true;
        }

        netMoveTimer += Time.deltaTime;
        if (netMoveTimer >= Const.NetMoveUpdateRate)
        {
            netMoveTimer = 0;

            foreach(var entry in multiPlayer.Players)
            {
                Player player = entry.Value;

                if (player.Character != null)
                {
                    var msg = new MoveMessage();
                    msg.Id = player.Id;
                    msg.Position = player.Character.transform.position;
                    msg.RotationY = player.Character.transform.eulerAngles.y;
                    msg.Path = player.Character.GetComponent<NavMeshAgent>().path.corners;
                    NetworkServer.SendToAll(CustomMsg.Move, msg);

                    player.Character.GetComponent<Animator>().SetBool("Walking",
                        player.Character.GetComponent<NavMeshAgent>().velocity.magnitude > 0);
                }
            }

            sendTilesData = true;
            // SendTileDataToPlayers();
        }

        /*
        // test moving first player that joins.
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                multiPlayer.Players[1].Character.GetComponent<NavMeshAgent>().destination = hit.point;
            }
        }
        */

        // moved here to debug how much bytes are being sent for tiles data.
        SendTileDataToPlayers();

        // visualize path
        foreach (var entry in multiPlayer.Players)
        {
            Player player = entry.Value;
            if (player.Character != null)
            {
                NavMeshAgent agent = player.Character.GetComponent<NavMeshAgent>();
                LineRenderer lines = player.Character.GetComponent<LineRenderer>();
                if (agent.hasPath)
                {
                    lines.material.color = player.Color;
                    lines.positionCount = agent.path.corners.Length;
                    lines.SetPositions(agent.path.corners);
                }
            }
        }

        if (Mocap != null && Mocap.actor != null)
        {
            if (firstTimeSendingMocapData)
            {
                firstTimeSendingMocapData = false;

                NetworkServer.SendToAll(CustomMsg.MocapData, GetFullMocapDataMessage());
            }
            else
            {
                var mocapDataMessage = new MocapDataMessage();

                NeuronActor mocapData = Mocap.actor;

                List<byte> bytes = new List<byte>();

                // Debug data (original mocap data)
                int mocapDataPerFrame = mocapData.GetData().Length * sizeof(float);
                mocapDataPerSecond += mocapDataPerFrame;
                if (updateUIPerSecond)
                {
                    total += mocapDataPerSecond;
                    prevMocapDataPerSecond = mocapDataPerSecond;
                    mocapDataPerSecond = 0;
                }
                PlayModeGUI.SetMocapText(mocapDataPerFrame, prevMocapDataPerSecond);

                int numBones = NeuronActor.MaxFrameDataLength / 6;

                for (int i = 0; i < numBones; i++)
                {
                    int offset = 0;
                    offset += i * 6;

                    Vector3 position = new Vector3(-mocapData.GetData()[offset], mocapData.GetData()[offset + 1], mocapData.GetData()[offset + 2]);
                    Vector3 prevPosition = new Vector3(-prevMocapData[offset], prevMocapData[offset + 1], prevMocapData[offset + 2]);

                    offset = 0;
                    offset += 3 + i * 6;

                    Vector3 rotation = new Vector3(mocapData.GetData()[offset + 1], -mocapData.GetData()[offset], -mocapData.GetData()[offset + 2]);
                    Vector3 prevRotation = new Vector3(prevMocapData[offset + 1], -prevMocapData[offset], -prevMocapData[offset + 2]);

                    bool positionChanged = false;
                    bool rotationChanged = false;

                    if (Vector3.Distance(position, prevPosition) > 0)
                    {
                        positionChanged = true;
                        position *= NeuronActor.NeuronUnityLinearScale;
                    }

                    if (Vector3.Distance(rotation, prevRotation) > 0)
                    {
                        rotationChanged = true;
                    }

                    // rotation changed
                    if (rotationChanged && !positionChanged)
                    {
                        // 14 bytes
                        bytes.Add((byte)i); // bone index
                        bytes.Add(0); // what changed
                        bytes.AddRange(BitConverter.GetBytes(rotation.x));
                        bytes.AddRange(BitConverter.GetBytes(rotation.y));
                        bytes.AddRange(BitConverter.GetBytes(rotation.z));
                    }
                    // position changed
                    else if (!rotationChanged && positionChanged)
                    {
                        // 14 bytes
                        bytes.Add((byte)i); // bone index
                        bytes.Add(1); // what changed
                        bytes.AddRange(BitConverter.GetBytes(position.x));
                        bytes.AddRange(BitConverter.GetBytes(position.y));
                        bytes.AddRange(BitConverter.GetBytes(position.z));
                    }
                    // both changed
                    else if (rotationChanged && positionChanged)
                    {
                        // 26 bytes
                        bytes.Add((byte)i); // bone index
                        bytes.Add(2); // what changed
                        bytes.AddRange(BitConverter.GetBytes(rotation.x));
                        bytes.AddRange(BitConverter.GetBytes(rotation.y));
                        bytes.AddRange(BitConverter.GetBytes(rotation.z));
                        bytes.AddRange(BitConverter.GetBytes(position.x));
                        bytes.AddRange(BitConverter.GetBytes(position.y));
                        bytes.AddRange(BitConverter.GetBytes(position.z));
                    }
                }

                Array.Copy(mocapData.GetData(), prevMocapData, mocapData.GetData().Length);

                // Debug data (delta compression)
                mocapDeltaCompressedDataPerSecond += bytes.Count;
                if (updateUIPerSecond)
                {
                    prevMocapDeltaCompressedDataPerSecond = mocapDeltaCompressedDataPerSecond;
                    mocapDeltaCompressedDataPerSecond = 0;
                }
                PlayModeGUI.SetMocapDeltaCompressionText(bytes.Count, prevMocapDeltaCompressedDataPerSecond);

                mocapDataMessage.Data = Compress(bytes.ToArray());

                byte[] SystemIODeflate = CompressSystemIO(bytes.ToArray());

                // Debug data (System.IO.Compression.Deflate)
                mocapSystemDeflateCompressedDataPerSecond += SystemIODeflate.Length;
                if (updateUIPerSecond)
                {
                    prevMocapSystemDeflateCompressedDataPerSecond = mocapSystemDeflateCompressedDataPerSecond;
                    mocapSystemDeflateCompressedDataPerSecond = 0;
                }
                PlayModeGUI.SetDeflateSystemCompressionText(SystemIODeflate.Length, prevMocapSystemDeflateCompressedDataPerSecond);

                // Debug data (Unity.IO.Compression.Deflate)
                mocapUnityDeflateCompressedDataPerSecond += mocapDataMessage.Data.Length;
                if (updateUIPerSecond)
                {
                    totalOptimized += mocapUnityDeflateCompressedDataPerSecond;

                    prevMocapUnityDeflateCompressedDataPerSecond = mocapUnityDeflateCompressedDataPerSecond;
                    mocapUnityDeflateCompressedDataPerSecond = 0;
                }
                PlayModeGUI.SetDeflateUnityCompressionText(mocapDataMessage.Data.Length, prevMocapUnityDeflateCompressedDataPerSecond);

                NetworkServer.SendToAll(CustomMsg.MocapData, mocapDataMessage);
            }
        }

        // Debug data (VoIP)
        if (multiPlayer != null && multiPlayer.UNetDissonance != null)
        {
            PlayModeGUI.SetVoIPText(multiPlayer.UNetDissonance.GetTotalTraffic());

            if (updateUIPerSecond)
            {
                total += (int)multiPlayer.UNetDissonance.GetTotalBytesPerSecond();
                totalOptimized += (int)multiPlayer.UNetDissonance.GetTotalBytesPerSecond();
            }
        }

        if (updateUIPerSecond)
        {
            PlayModeGUI.SetTotalText(total);
            PlayModeGUI.SetTotalOptimizedText(totalOptimized);
            total = 0;
            totalOptimized = 0;
        }

        updateUIPerSecond = false;
    }

    public MocapDataMessage GetFullMocapDataMessage()
    {
        if (Mocap != null && Mocap.actor != null)
        {
            var mocapDataMessage = new MocapDataMessage();

            NeuronActor mocapData = Mocap.actor;

            List<byte> bytes = new List<byte>();

            int numBones = NeuronActor.MaxFrameDataLength / 6;

            for (int i = 0; i < numBones; i++)
            {
                int offset = 0;
                offset += i * 6;

                Vector3 position = new Vector3(-mocapData.GetData()[offset], mocapData.GetData()[offset + 1], mocapData.GetData()[offset + 2]);

                offset = 0;
                offset += 3 + i * 6;

                Vector3 rotation = new Vector3(mocapData.GetData()[offset + 1], -mocapData.GetData()[offset], -mocapData.GetData()[offset + 2]);

                bool positionChanged = false;
                bool rotationChanged = false;

                if (Vector3.Distance(position, Vector3.zero) > 0)
                {
                    positionChanged = true;
                    position *= NeuronActor.NeuronUnityLinearScale;
                }

                if (Vector3.Distance(rotation, Vector3.zero) > 0)
                {
                    rotationChanged = true;
                }

                // rotation changed
                if (rotationChanged && !positionChanged)
                {
                    // 14 bytes
                    bytes.Add((byte)i); // bone index
                    bytes.Add(0); // what changed
                    bytes.AddRange(BitConverter.GetBytes(rotation.x));
                    bytes.AddRange(BitConverter.GetBytes(rotation.y));
                    bytes.AddRange(BitConverter.GetBytes(rotation.z));
                }
                // position changed
                else if (!rotationChanged && positionChanged)
                {
                    // 14 bytes
                    bytes.Add((byte)i); // bone index
                    bytes.Add(1); // what changed
                    bytes.AddRange(BitConverter.GetBytes(position.x));
                    bytes.AddRange(BitConverter.GetBytes(position.y));
                    bytes.AddRange(BitConverter.GetBytes(position.z));
                }
                // both changed
                else if (rotationChanged && positionChanged)
                {
                    // 26 bytes
                    bytes.Add((byte)i); // bone index
                    bytes.Add(2); // what changed
                    bytes.AddRange(BitConverter.GetBytes(rotation.x));
                    bytes.AddRange(BitConverter.GetBytes(rotation.y));
                    bytes.AddRange(BitConverter.GetBytes(rotation.z));
                    bytes.AddRange(BitConverter.GetBytes(position.x));
                    bytes.AddRange(BitConverter.GetBytes(position.y));
                    bytes.AddRange(BitConverter.GetBytes(position.z));
                }
            }

            mocapDataMessage.Data = Compress(bytes.ToArray());

            return mocapDataMessage;
        }

        return null;
    }
    
    public static byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (Unity.IO.Compression.DeflateStream dstream = 
                new Unity.IO.Compression.DeflateStream(output, Unity.IO.Compression.CompressionMode.Compress))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public static byte[] CompressSystemIO(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, CompressionMode.Compress))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public void MovePlayer(int id, Vector3 destination)
    {
        multiPlayer.Players[id].Character.GetComponent<NavMeshAgent>().destination = destination;
    }
}

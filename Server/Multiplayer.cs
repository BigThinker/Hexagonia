﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Multiplayer : MonoBehaviour {

    private static Multiplayer instance;

    public LobbyGUI GUI;
    public Dictionary<int, Player> Players = new Dictionary<int, Player>();
    public SimpleCampaignOverview CampaignOverview;

    [HideInInspector]
    public Dissonance.Integrations.UNet_LLAPI.UNetCommsNetwork UNetDissonance;

    private List<Color> colors = new List<Color>(new Color[] { Color.white,
                                            Color.black,
                                            Color.blue,
                                            Color.cyan,
                                            Color.gray,
                                            Color.green,
                                            Color.red,
                                            Color.yellow
                                            });
    private List<Vector3> spawnPoints;
    private bool levelHasLoaded = false;
    private int currentCharacterId = 0;

    void Start() {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            Host();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool Host()
    {
        if (NetworkServer.Listen(4444))
        {
            Debug.Log("Server is listening...");
            NetworkServer.RegisterHandler(MsgType.Connect, GotPlayer);
            NetworkServer.RegisterHandler(MsgType.Disconnect, LostPlayer);

            // custom msgs
            NetworkServer.RegisterHandler(CustomMsg.StartGame, GotStartGame);
            NetworkServer.RegisterHandler(CustomMsg.RequestMove, GotRequestMove);

            //...
            CampaignOverview.OnSpawnPointsParsed += OnSpawnPointsParsed;

            switch (SceneManager.GetActiveScene().name)
            {
                case "Lobby":
                    GUI.ShowUI();
                    break;
                case "PlayMode":
                    FindObjectOfType<PlayMode>().OnLevelLoaded += OnLevelLoaded;
                    break;
            }

            UNetDissonance = GetComponent<Dissonance.Integrations.UNet_LLAPI.UNetCommsNetwork>();
            if (UNetDissonance != null) {
                UNetDissonance.InitializeAsServer();
            }

            return true;
        }

        return false;
    }
    
    void GotRequestMove(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<RequestMoveMessage>();
        FindObjectOfType<PlayMode>().MovePlayer(netMsg.conn.connectionId, msg.Destination);
    }

    void OnSpawnPointsParsed()
    {
        spawnPoints = new List<Vector3>(CampaignOverview.SpawnPoints);
        
        // assign spawn points to players who joined before spawn points were parsed.
        foreach(var entry in Players)
        {
            Player player = entry.Value;
            player.SpawnPosition = GetNextSpawnPoint();
            NetworkServer.SendToAll(CustomMsg.SpawnPoint, new SpawnPointMessage()
            {
                Id = player.Id,
                SpawnPosition = player.SpawnPosition
            });
        }
    }

    void OnLevelLoaded()
    {
        levelHasLoaded = true;
    }

    public void StartGame()
    {
        NetworkServer.SendToAll(CustomMsg.StartGame, new StartGameMessage());
        
        // check in case no players are connected to start the game immediately.
        CheckFinalStartGame();
    }

    // when server gets start game (ok) from all clients, he starts the game.
    void GotStartGame(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;

        Players[id].Ready = true;

        CheckFinalStartGame();
    }

    void CheckFinalStartGame()
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            int count = 0;

            foreach (var entry in Players)
            {
                Player player = entry.Value;
                if (player.Ready)
                {
                    count++;
                }
            }

            if (count == Players.Count)
            {
                NetworkServer.SendToAll(CustomMsg.FinalStartGame, new FinalStartGameMessage());

                SceneManager.LoadScene("PlayMode");
            }
        }
    }

    Vector3 GetNextSpawnPoint()
    {
        Vector3 spawnPosition = Vector3.zero;

        if (spawnPoints.Count > 0)
        {
            int randSpawnIndex = Random.Range(0, spawnPoints.Count);
            spawnPosition = spawnPoints[randSpawnIndex];
            spawnPoints.RemoveAt(randSpawnIndex);
        }
        else if (CampaignOverview.SpawnPoints.Count > 0)
        {
            spawnPosition = CampaignOverview.SpawnPoints[Random.Range(0, CampaignOverview.SpawnPoints.Count)];
        }

        return spawnPosition;
    }

    void GotPlayer(NetworkMessage netMsg)
    {
        Debug.Log("Got player with id: " + netMsg.conn.connectionId);

        int id = netMsg.conn.connectionId;

        Players[id] = new Player();
        Players[id].Id = id;
        Players[id].Name = "Player" + Random.Range(1, 1000);

        // Color
        Color color;
        if (colors.Count > 0)
        {
            int randColorIndex = Random.Range(0, colors.Count);
            color = colors[randColorIndex];
            colors.RemoveAt(randColorIndex);
        }
        else
        {
            color = Random.ColorHSV();
        }
        Players[id].Color = color;

        // Spawn position
        Players[id].SpawnPosition = GetNextSpawnPoint();
        Players[id].CharacterId = currentCharacterId;

        currentCharacterId++;
        if (currentCharacterId >= Const.NumCharacters)
        {
            currentCharacterId = 0;
        }

        // send the new player to everyone.
        NetworkServer.SendToAll(CustomMsg.PlayerInfo, new PlayerInfoMessage()
        {
            Id = id,
            Name = Players[id].Name,
            Color = Players[id].Color,
            SpawnPosition = Players[id].SpawnPosition,
            CharacterId = Players[id].CharacterId
        });

        // send everyone to the new player.
        foreach(KeyValuePair<int, Player> entry in Players)
        {
            var player = entry.Value;
            if (id != player.Id)
            {
                netMsg.conn.Send(CustomMsg.PlayerInfo, new PlayerInfoMessage()
                {
                    Id = player.Id,
                    Name = player.Name,
                    Color = player.Color,
                    SpawnPosition = player.SpawnPosition,
                    CharacterId = player.CharacterId
                });
            }
        }

        // send dissonance id to new player.
        if (UNetDissonance != null)
        {
            var dissonanceIdMessage = new DissonanceIdMessage()
            {
                Id = 0,
                DissonanceId = UNetDissonance.PlayerName
            };

            netMsg.conn.Send(CustomMsg.DissonanceId, dissonanceIdMessage);
        }

        PlayMode playMode = FindObjectOfType<PlayMode>();
        if (playMode != null)
        {
            var gameMasterSpawnLocationMessage = new GameMasterSpawnLocationMessage()
            {
                SpawnLocation = playMode.GameMasterSpawnLocation
            };

            NetworkServer.SendToAll(CustomMsg.GameMasterSpawnLocation, gameMasterSpawnLocationMessage);

            // send all mocap data to new player (no delta compression).
            var mocapDataMessage = playMode.GetFullMocapDataMessage();
            if (mocapDataMessage != null)
            {
                netMsg.conn.Send(CustomMsg.MocapData, mocapDataMessage);
            }
        }

        switch (SceneManager.GetActiveScene().name)
        {
            case "Lobby":
                GUI.UpdateUI();
                break;
            case "PlayMode":
                // game has continued already, so send a final start game message to the new player.
                netMsg.conn.Send(CustomMsg.FinalStartGame, new FinalStartGameMessage());

                if (levelHasLoaded)
                {
                    FindObjectOfType<PlayMode>().InstantiatePlayers();
                }
                break;
        }
    }

    void LostPlayer(NetworkMessage netMsg)
    {
        Debug.Log("Lost player with id: " + netMsg.conn.connectionId);

        int id = netMsg.conn.connectionId;

        if (Players[id].Character != null)
        {
            Destroy(Players[id].Character);
        }

        Players.Remove(id);

        // broadcast.
        NetworkServer.SendToAll(CustomMsg.PlayerDisconnect, new PlayerDisconnectMessage()
        {
            Id = netMsg.conn.connectionId
        });

        if (GUI != null)
        {
            GUI.UpdateUI();
        }

        // in case a player is lost while server is getting ok from all clients.
        CheckFinalStartGame();
    }
}

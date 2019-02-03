using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Multiplayer : MonoBehaviour {

    private static Multiplayer instance;

    public LobbyGUI GUI;
    public Dictionary<int, Player> Players = new Dictionary<int, Player>();
    public Player LocalPlayer;

    private NetworkClient client;
    private Dissonance.Integrations.UNet_LLAPI.UNetCommsNetwork UNetDissonance;

    void Start () {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            Join();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Join()
    {
        if (client == null)
        {
            client = new NetworkClient();

            client.RegisterHandler(MsgType.Connect, OnConnected);
            client.RegisterHandler(MsgType.Disconnect, OnDisconnected);

            // custom msgs
            client.RegisterHandler(CustomMsg.PlayerInfo, GotPlayerInfo);
            client.RegisterHandler(CustomMsg.PlayerDisconnect, GotPlayerDisconnect);
            client.RegisterHandler(CustomMsg.StartGame, GotStartGame);
            client.RegisterHandler(CustomMsg.FinalStartGame, GotFinalStartGame);
            client.RegisterHandler(CustomMsg.TileData, GotTileData);
            client.RegisterHandler(CustomMsg.Move, GotMove);
            client.RegisterHandler(CustomMsg.SpawnPoint, GotSpawnPoint);
            client.RegisterHandler(CustomMsg.DissonanceId, GotDissonanceId);
            client.RegisterHandler(CustomMsg.MocapData, GotMocapData);
            client.RegisterHandler(CustomMsg.GameMasterSpawnLocation, GotGameMasterSpawnLocation);

            UNetDissonance = GetComponent<Dissonance.Integrations.UNet_LLAPI.UNetCommsNetwork>();
        }
        
        if (GUI != null)
        {
            client.Connect(GUI.GetIPAddress(), 4444);
        }
        else
        {
            client.Connect("127.0.0.1", 4444);
        }
    }

    // got server dissonance id.
    void GotDissonanceId(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<DissonanceIdMessage>();

        Dissonance.VoiceBroadcastTrigger voiceBroadcastTrigger = GetComponent<Dissonance.VoiceBroadcastTrigger>();
        if (voiceBroadcastTrigger != null) {
            voiceBroadcastTrigger.PlayerId = msg.DissonanceId;
        }
    }

    void GotSpawnPoint(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<SpawnPointMessage>();
        Players[msg.Id].SpawnPosition = msg.SpawnPosition;
    }

    void GotMove(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<MoveMessage>();
        PlayMode playMode = FindObjectOfType<PlayMode>();
        if (playMode != null)
        {
            playMode.MovePlayer(msg);
        }
    }

    void GotTileData(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<TileDataMessage>();
        FindObjectOfType<PlayMode>().InstantiateTiles(msg);
    }

    void GotMocapData(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<MocapDataMessage>();
        FindObjectOfType<PlayMode>().ProcessMocap(msg.Data);
    }

    void GotGameMasterSpawnLocation(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<GameMasterSpawnLocationMessage>();
        PlayMode playMode = FindObjectOfType<PlayMode>();
        if (playMode != null)
        {
            playMode.SetGameMasterSpawnLocation(msg.SpawnLocation);
        }
    }

    public void RequestMove(Vector3 destination)
    {
        client.Send(CustomMsg.RequestMove, new RequestMoveMessage()
        {
            Destination = destination
        });
    }

    void GotStartGame(NetworkMessage netMsg)
    {
        client.Send(CustomMsg.StartGame, new StartGameMessage());
    }

    void GotFinalStartGame(NetworkMessage netMsg)
    {
        // don't reload the same scene.
        if (SceneManager.GetActiveScene().name != "PlayMode")
        {
            SceneManager.LoadScene("PlayMode");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Lobby":
                GUI = FindObjectOfType<LobbyGUI>();
                break;
            case "PlayMode":
                // PlayMode = FindObjectOfType<PlayMode>();
                break;
        }
    }

    void GotPlayerInfo(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<PlayerInfoMessage>();

        Players[msg.Id] = new Player();
        Players[msg.Id].Id = msg.Id;
        Players[msg.Id].Name = msg.Name;
        Players[msg.Id].Color = msg.Color;
        Players[msg.Id].SpawnPosition = msg.SpawnPosition;
        Players[msg.Id].CharacterId = msg.CharacterId;

        if (LocalPlayer == null)
        {
            LocalPlayer = Players[msg.Id];
            
            if (UNetDissonance != null) {
                UNetDissonance.InitializeAsClient(GUI.GetIPAddress());
            }
        }

        switch (SceneManager.GetActiveScene().name)
        {
            case "Lobby":
                GUI.UpdateUI();
                break;
            case "PlayMode":
                break;
        }
    }

    void GotPlayerDisconnect(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<PlayerDisconnectMessage>();

        if (Players[msg.Id].Character != null)
        {
            Destroy(Players[msg.Id].Character);
        }

        Players.Remove(msg.Id);

        if (GUI != null)
        {
            GUI.UpdateUI();
        }
    }

    void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server.");

        if (GUI != null)
        {
            GUI.ShowUI();
        }
    }

    void OnDisconnected(NetworkMessage netMsg)
    {
        Debug.Log("Disconnected from server.");

        Players.Clear();
        LocalPlayer = null;

        switch (SceneManager.GetActiveScene().name)
        {
            case "Lobby":
                GUI.UpdateUI();
                GUI.HideUI();
                break;
            case "PlayMode":
                SceneManager.LoadScene("Lobby");
                break;
        }
    }
}

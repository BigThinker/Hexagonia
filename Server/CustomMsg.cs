using UnityEngine;
using UnityEngine.Networking;

public class CustomMsg
{
    public static short PlayerInfo = MsgType.Highest + 1;
    public static short PlayerDisconnect = MsgType.Highest + 2;
    public static short StartGame = MsgType.Highest + 3;
    public static short FinalStartGame = MsgType.Highest + 4;
    public static short TileData = MsgType.Highest + 5;
    public static short Move = MsgType.Highest + 6;
    public static short SpawnPoint = MsgType.Highest + 7;
    public static short RequestMove = MsgType.Highest + 8;
    public static short DissonanceId = MsgType.Highest + 9;
    public static short MocapData = MsgType.Highest + 10;
    public static short GameMasterSpawnLocation = MsgType.Highest + 11;
}

public class PlayerInfoMessage : MessageBase
{
    public int Id;
    public string Name;
    public Color Color;
    public Vector3 SpawnPosition;
    public int CharacterId;
}

public class SpawnPointMessage : MessageBase
{
    public int Id;
    public Vector3 SpawnPosition;
}

public class PlayerDisconnectMessage : MessageBase
{
    public int Id;
}

public class StartGameMessage : MessageBase
{

}

public class FinalStartGameMessage : MessageBase
{

}

public class TileDataMessage : MessageBase
{
    // recepient
    public int Id;

    // tile data
    public int[] TilesXZ;
    public float[] TilesScaleY;
    public int[] TilesMaterial;

    // objects data
    public int[] ObjectsId;
    public Vector3[] ObjectsPosition;
    public float[] ObjectsRotationY;
    public float[] ObjectsScale;
}

public class MoveMessage : MessageBase
{
    public int Id;
    public Vector3 Position;
    public float RotationY;
    public Vector3[] Path;
}

public class RequestMoveMessage : MessageBase
{
    public Vector3 Destination;
}

public class DissonanceIdMessage : MessageBase
{
    public int Id;
    public string DissonanceId;
}

public class MocapDataMessage : MessageBase
{
    public byte[] Data;
}

public class GameMasterSpawnLocationMessage : MessageBase
{
    public Vector3 SpawnLocation;
}
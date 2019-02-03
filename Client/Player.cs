using UnityEngine;

public class Player
{
    public int Id;
    public string Name;
    public Color Color;
    public Vector3 SpawnPosition;
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public Quaternion StartRotation;
    public Quaternion EndRotation;
    public float LerpTimer;
    public GameObject Character;
    public int CharacterId;
}

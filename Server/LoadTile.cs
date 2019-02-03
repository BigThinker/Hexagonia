using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTile {
    public List<LoadGameObject> gameObjects = new List<LoadGameObject>();

    public int tileX;
    public int tileZ;
    public float tileScaleY;
    public int tileMaterial;

    public LoadTile(int tileX, int tileZ, float tileScaleY, int tileMaterial)
    {
        this.tileX = tileX;
        this.tileZ = tileZ;
        this.tileScaleY = tileScaleY;
        this.tileMaterial = tileMaterial;
    }

    public void AddGameObject(int objectId, Vector3 position, float rotationY, float scale)
    {
        gameObjects.Add(new LoadGameObject() {
            objectId = objectId,
            position = position,
            rotationY = rotationY,
            scale = scale
        });
    }
}

public class LoadGameObject
{
    public int objectId;
    public Vector3 position;
    public float rotationY;
    public float scale;
}

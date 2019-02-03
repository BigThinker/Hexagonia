using System.Collections.Generic;
using UnityEngine;

public class Const
{
    // TODO: implement general objects in the game.
    public const int GeneralObjectStartIndex = -1;
    // the 5th element and onwards are gameplay objects.
    public const int GameplayObjectStartIndex = 4;

    public const float NetMoveUpdateRate = 1 / 3.0f;

    public static string LevelFileExtension = ".level";
    public static float hexRadius = 1 / Mathf.Sqrt(3);
    public static Color SelectedColor = Color.cyan;

    public static CubeIndex[] directions =
        new CubeIndex[] {
            new CubeIndex(1, -1, 0),
            new CubeIndex(1, 0, -1),
            new CubeIndex(0, 1, -1),
            new CubeIndex(-1, 1, 0),
            new CubeIndex(-1, 0, 1),
            new CubeIndex(0, -1, 1)
        };

    // does not return tile itself.
    public static List<Tile> Neighbours(Tile tile, Dictionary<string, List<GameObject>> grid)
    {
        List<Tile> ret = new List<Tile>();
        CubeIndex o;

        for (int i = 0; i < 6; i++)
        {
            o = tile.index + directions[i];
            if (grid.ContainsKey(o.ToString()))
                ret.Add(GetTileFromIndex(o.ToString(), grid));
        }
        return ret;
    }

    // returns tile itself as well.
    public static List<Tile> TilesInRange(Tile center, int range, Dictionary<string, List<GameObject>> grid)
    {
        //Return tiles rnage steps from center, http://www.redblobgames.com/grids/hexagons/#range
        List<Tile> ret = new List<Tile>();

        if (center == null)
        {
            return ret;
        }

        CubeIndex o;

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
            {
                o = new CubeIndex(dx, dy, -dx - dy) + center.index;
                if (grid.ContainsKey(o.ToString()))
                    ret.Add(GetTileFromIndex(o.ToString(), grid));
            }
        }
        return ret;
    }

    public static Tile GetTileFromGameObjects(List<GameObject> objects)
    {
        // alternative: foreach object return the one that is tile.

        if (objects != null)
        {
            return objects[0].GetComponent<Tile>();
        }

        return null;
    }

    public static Tile GetTileFromIndex(string index, Dictionary<string, List<GameObject>> grid)
    {
        if (grid.ContainsKey(index))
        {
            return grid[index][0].GetComponent<Tile>();
        }

        return null;
    }

    public static Tile GetTileFromWorldPosition(Vector3 position, Dictionary<string, List<GameObject>> grid)
    {
        float q = (Mathf.Sqrt(3) / 3 * position.x - 1.0f / 3 * position.z) / hexRadius;
        float r = (2.0f / 3 * position.z) / hexRadius;
        int tileX = (int)Mathf.Round(q);
        int tileY = (int)Mathf.Round(r);
        CubeIndex index = new CubeIndex(tileX, tileY, -tileX - tileY);
        if (grid.ContainsKey(index.ToString()))
        {
            return GetTileFromGameObjects(grid[index.ToString()]);
        }

        return null;
    }

    public static Renderer GetMeshFromTile(Tile tile)
    {
        return tile.transform.GetChild(0).GetComponent<Renderer>();
    }

    public static void DrawOutline(GameObject gameObject, Material material)
    {
        Vector3 pos = gameObject.transform.position;
        LineRenderer lines = gameObject.GetComponent<LineRenderer>();

        if (!lines)
        {
            lines = gameObject.AddComponent<LineRenderer>(); ;
        }

        lines.material = material;
        lines.startWidth = 0.1f;
        lines.endWidth = 0.1f;
        lines.startColor = Color.black;
        lines.endColor = Color.black;
        lines.positionCount = 7;
        for (int vert = 0; vert <= 6; vert++)
            lines.SetPosition(vert, Tile.Corner(
                new Vector3(pos.x,
                2 * gameObject.GetComponentInChildren<MeshRenderer>().bounds.extents.y + pos.y + 0.1f,
                pos.z),
                hexRadius, vert));
    }

    public static bool ParseLevelName(string name, out int tileX, out int tileY)
    {
        tileX = 0;
        tileY = 0;
        string[] commaParts = name.Split(',');
        if (commaParts.Length == 2)
        {
            int parseCount = 0;
            int tX;
            if (int.TryParse(commaParts[0], out tX))
            {
                parseCount++;
            }
            int tY;
            if (int.TryParse(commaParts[1], out tY))
            {
                parseCount++;
            }

            if (parseCount == 2)
            {
                tileX = tX;
                tileY = tY;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static Transform FindChildRecursive(Transform transform, string name)
    {
        if (transform.name == name) return transform;
        foreach (Transform child in transform)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}

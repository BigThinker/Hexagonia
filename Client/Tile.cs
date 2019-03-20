using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    public CubeIndex index;

    public static Vector3 Corner(Vector3 origin, float radius, int corner)
    {
        float angle = 60 * corner + 30;
        angle *= Mathf.PI / 180;
        return new Vector3(origin.x + radius * Mathf.Cos(angle), origin.y, origin.z + radius * Mathf.Sin(angle));
    }
}

[System.Serializable]
public struct CubeIndex
{
    public int x;
    public int y;
    public int z;

    public CubeIndex(int x, int y, int z)
    {
        this.x = x; this.y = y; this.z = z;
    }

    public CubeIndex(int x, int z)
    {
        this.x = x; this.z = z; this.y = -x - z;
    }

    public static CubeIndex operator +(CubeIndex one, CubeIndex two)
    {
        return new CubeIndex(one.x + two.x, one.y + two.y, one.z + two.z);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        CubeIndex o = (CubeIndex)obj;
        if ((System.Object)o == null)
            return false;
        return ((x == o.x) && (y == o.y) && (z == o.z));
    }

    public override int GetHashCode()
    {
        return (x.GetHashCode() ^ (y.GetHashCode() + (int)(Mathf.Pow(2, 32) / (1 + Mathf.Sqrt(5)) / 2) + (x.GetHashCode() << 6) + (x.GetHashCode() >> 2)));
    }

    public override string ToString()
    {
        return string.Format("[" + x + "," + y + "," + z + "]");
    }
}

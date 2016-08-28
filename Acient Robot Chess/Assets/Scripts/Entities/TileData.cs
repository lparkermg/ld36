using UnityEngine;
using System.Collections;

public class TileData : MonoBehaviour {

    public int X = 0;
    public int Y = 0;

    public void UpdateData(int x, int y)
    {
        X = x;
        Y = y;
    }
}

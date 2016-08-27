using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class LevelDataHolder : MonoBehaviour {

    public List<Texture2D> LevelLayouts;
    public List<GameObject> LevelTiles;

	public Texture2D GetRandomLayout()
    {
        var layout = Random.Range(0, LevelLayouts.Count);
        return LevelLayouts[layout];
    }

    public Texture2D GetLayoutByNumber(int num)
    {
        return LevelLayouts[num];
    }

    public GameObject GetTileTypeByNumber(int num)
    {
        return LevelTiles[num];
    }
}

using UnityEngine;
using System.Collections;
using Helpers.Levels;
public class GameplayManager : MonoBehaviour {
    const int LEVEL_SIZE = 16;
    private int[,] _levelData;

    private LevelDataHolder _levelDataHolder;
	// Use this for initialization
	void Start () {
        _levelDataHolder = GetComponent<LevelDataHolder>();
        _levelData = LevelHelper.ParseFromTexture2D(_levelDataHolder.GetRandomLayout());
        StartCoroutine(LoadLevelTiles());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator LoadLevelTiles()
    {
        for (var x = 0; x < LEVEL_SIZE; x++)
        {
            for (var y = 0; y < LEVEL_SIZE; y++)
            {
                var placementLocation = new Vector3((float)x, 0.0f, (float)y);
                if (_levelData[x, y] < 3) {
                    var tileTemplate = _levelDataHolder.GetTileTypeByNumber(_levelData[x, y]);
                    GameObject tile = GameObject.Instantiate(tileTemplate,placementLocation,tileTemplate.transform.rotation) as GameObject;
                }
                
            }
        }
        yield return null;
    }
}

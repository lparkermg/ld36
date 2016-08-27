using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Helpers.Levels;
public class GameplayManager : MonoBehaviour {

    //Level Stuff.
    const int LEVEL_SIZE = 16;
    private int[,] _levelData;

    private LevelDataHolder _levelDataHolder;
    private GameObject[,] _tileLocations;
    #region Spawner Data
    private Vector3?[,] _redSpawnerLocations;
    private Vector3?[,] _blueSpawnerLocations;
    private int _redSpawnerCount;
    private int _blueSpawnerCount;
    #endregion

    //Minion Location Stuff
    private int[,] _minionLocations;
    public List<GameObject> FriendlyMinionObjects = new List<GameObject>();
    public List<GameObject> EnemyMinionObjects = new List<GameObject>();

    //Score Keeping
    public int CurrentRound = 1;
    public int TotalRounds = 5;

    public int BlueTotalPoints = 0;
    public int RedTotalPoints = 0;

    public List<int> LevelsDone = new List<int>();


	// Use this for initialization
	void Start () {
        _levelDataHolder = GetComponent<LevelDataHolder>();

        _tileLocations = new GameObject[LEVEL_SIZE, LEVEL_SIZE];
        _blueSpawnerLocations = new Vector3?[LEVEL_SIZE, LEVEL_SIZE];
        _redSpawnerLocations = new Vector3?[LEVEL_SIZE, LEVEL_SIZE];
        _minionLocations = new int[LEVEL_SIZE, LEVEL_SIZE];


        LoadNewLevel();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Level Stuff
    private void LoadNewLevel()
    {
        var layoutNotDone = false;

        while (!layoutNotDone)
        {
            var matched = 0;
            var levelLayout = _levelDataHolder.GetRandomLayout();

            if(LevelsDone.Count > 0)
            {
                foreach(var levelDone in LevelsDone)
                {
                    if (levelDone == _levelDataHolder.LastRandomLayout)
                        matched++;
                }

                if (matched == 0)
                {
                    _levelData = LevelHelper.ParseFromTexture2D(levelLayout);
                    layoutNotDone = true;
                }
            }

            _levelData = LevelHelper.ParseFromTexture2D(levelLayout);
            layoutNotDone = true;
        }

        LevelsDone.Add(_levelDataHolder.LastRandomLayout);
        StartCoroutine(LoadLevelTiles());
        StartCoroutine(SpawnMinions());

    }
    IEnumerator LoadLevelTiles()
    {
        for (var x = 0; x < LEVEL_SIZE; x++)
        {
            for (var y = 0; y < LEVEL_SIZE; y++)
            {
                var placementLocation = new Vector3((float)x, 0.0f, (float)y);
                if (_levelData[x, y] < 3) {
                    CheckSpawnType(_levelData[x, y]);
                    AddSpawnLocation(x, y, _levelData[x, y]);
                    var tileTemplate = _levelDataHolder.GetTileTypeByNumber(_levelData[x, y]);
                    GameObject tile = GameObject.Instantiate(tileTemplate,placementLocation,tileTemplate.transform.rotation) as GameObject;
                    _tileLocations[x, y] = tile;
                }
                
            }
        }
        yield return null;
    }

    /*IEnumerator DestroyOldLevel()
    {
        //TODO: Clear Tiles.
        //TODO: Clear Spawner Location Data.
        //TODO: Clear Remaining Minions.
        //TODO: Clear LevelData.
        //TODO: Clear Minion Locations Data.
    }*/

    IEnumerator SpawnMinions()
    {
        var spawnAmountBlue = _blueSpawnerCount / 2;
        var spawnAmountRed = _redSpawnerCount / 2;

        var placedAmountBlue = 0;
        var placedAmountRed = 0;

        var placedBlue = false;
        var placedRed = true;
        for(var x = 0; x < LEVEL_SIZE; x++)
        {
            for(var y = 0; y < LEVEL_SIZE; y++)
            {
                if (_blueSpawnerLocations[x, y] != null && !placedBlue && placedAmountBlue < spawnAmountBlue)
                {
                    var blueBot = _levelDataHolder.TeamBlueMinion;
                    GameObject blueMinion = GameObject.Instantiate(blueBot, (Vector3)_blueSpawnerLocations[x, y], blueBot.transform.rotation) as GameObject;
                    //TODO: Set location in minion class when it's made.
                    FriendlyMinionObjects.Add(blueMinion);
                    placedBlue = true;
                    placedAmountBlue++;
                    _minionLocations[x, y] = 1;

                }
                else if(_blueSpawnerLocations[x, y] != null && placedBlue)
                {
                    placedBlue = false;
                }

                if(_redSpawnerLocations[x,y] != null && !placedRed && placedAmountRed < spawnAmountRed)
                {
                    var redBot = _levelDataHolder.TeamRedMinion;
                    GameObject redMinion = GameObject.Instantiate(redBot, (Vector3)_redSpawnerLocations[x, y], redBot.transform.rotation) as GameObject;
                    //TODO: Set location in minion class when its made.
                    EnemyMinionObjects.Add(redMinion);
                    placedRed = true;
                    placedAmountRed++;
                    _minionLocations[x, y] = 2;
                }
                else if(_redSpawnerLocations[x, y] != null && placedRed)
                {
                    placedRed = false;
                }
            }
        }

        yield return null;
    }

    private void CheckSpawnType(int type)
    {
        if (type == 1)
            _blueSpawnerCount++;

        if (type == 2)
            _redSpawnerCount++;
    }

    private void AddSpawnLocation(int x, int y,int type)
    {
        var spawner = new Vector3((float)x, 1.0f, (float)y);
        if(type == 1)
        {
            _blueSpawnerLocations[x, y] = spawner;
        }
        else if(type == 2)
        {
            _redSpawnerLocations[x, y] = spawner;
        }
        else
        {
            _blueSpawnerLocations[x, y] = null;
            _redSpawnerLocations[x, y] = null;

        }
    }

    //Input Stuff
    public void SelectMinion(int x, int y)
    {
        //We Only need to select the blue team.
        if(_minionLocations[x,y] == 1)
        {
            //Get the minion that is at this location and set to selected.
        }
    }

    public bool CanMoveHere(int x, int y)
    {
        //We only need to check for if there's no data or if a friendly is there.
        if (_levelData[x,y] > 2)
            return false;

        //Team Blue Minion is here.
        if (_minionLocations[x, y] == 1)
            return false;

        return true;
    }

    //Minion Check
    private void CheckMinions()
    {
        if(FriendlyMinionObjects.Count == 0)
        {
            //TODO: Player Lost round code here.
            IncrementPoints(false, 10);
        }
        else if(EnemyMinionObjects.Count == 0)
        {
            //TODO: Player won round code here.
            IncrementPoints(true, 10);
        }

    }

    private void NextRound()
    {
        if(CurrentRound >= TotalRounds)
        {
            //TODO:Display Winner Message Here!
        }
        else
        {
            CurrentRound++;
            //TODO Destroy Old Level and Make new one.


        }
    }

    //Points Stuff
    public void IncrementPoints(bool forPlayer, int amount)
    {
        if (forPlayer)
        {
            BlueTotalPoints += amount;
        }
        else
        {
            RedTotalPoints += amount;
        }
    }
    
    private bool PlayerWins()
    {
        if(BlueTotalPoints > RedTotalPoints)
            return true;

        return false;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Helpers.Levels;
using Entities;
public class GameplayManager : MonoBehaviour {

    //Level Stuff.
    public const int LEVEL_SIZE = 16;
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
    private string[,] _minionLocations;
    private bool[,] _fightHere;
    public Dictionary<string, GameObject> FriendlyMinionObjects = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> EnemyMinionObjects = new Dictionary<string, GameObject>();

    //Score Keeping
    public int CurrentRound = 1;
    public int TotalRounds = 5;

    public int BlueTotalPoints = 0;
    public int RedTotalPoints = 0;

    public List<int> LevelsDone = new List<int>();

    //Current Stuff
    private string _selectedMinionName = "None";
    private GameObject _selectedMinion;
    public GameObject Cursor;
    private GameObject _cursor;

    //Input Stuff
    public Camera Camera;
    public Rigidbody CameraHolder;
    public InputManager InputManager;

	// Use this for initialization
	void Start () {
        _levelDataHolder = GetComponent<LevelDataHolder>();

        _tileLocations = new GameObject[LEVEL_SIZE, LEVEL_SIZE];
        _blueSpawnerLocations = new Vector3?[LEVEL_SIZE, LEVEL_SIZE];
        _redSpawnerLocations = new Vector3?[LEVEL_SIZE, LEVEL_SIZE];
        _minionLocations = new string[LEVEL_SIZE, LEVEL_SIZE];
        _fightHere = new bool[LEVEL_SIZE, LEVEL_SIZE];
        _cursor = GameObject.Instantiate(Cursor, new Vector3(0.0f, 0.0f, 0.0f), Cursor.transform.rotation) as GameObject;

        LoadNewLevel();
	}
	
	// Update is called once per frame
	void Update () {
        InputManager.CheckInput();
        //Auto change round for testing.
        if (Input.GetKeyUp(KeyCode.N))
        {
            NextRound();
        }
	}

    #region Level Stuff
    private void LoadNewLevel()
    {
        var layoutNotDone = false;
        _cursor.SetActive(false);
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
                    var data = tile.GetComponent<TileData>();
                    data.X = x;
                    data.Y = y;
                    _tileLocations[x, y] = tile;
                }
                
            }
        }
        yield return null;
    }

    IEnumerator DestroyOldLevel()
    {
        
        for(var x = 0; x <LEVEL_SIZE;x++)
        {
            for (var y = 0; y < LEVEL_SIZE; y++)
            {
                Destroy(_tileLocations[x, y]);
                _tileLocations[x, y] = null;

                _blueSpawnerLocations[x, y] = null;
                _redSpawnerLocations[x, y] = null;

                _levelData[x, y] = 0;

                _minionLocations[x, y] = "None";
            }
        }
        
        if(FriendlyMinionObjects.Count > 0)
        {
            foreach(var minion in FriendlyMinionObjects)
            {
                Destroy(minion.Value);
            }
        }

        FriendlyMinionObjects = new Dictionary<string, GameObject>();

        if(EnemyMinionObjects.Count > 0)
        {
            foreach(var minion in EnemyMinionObjects)
            {
                Destroy(minion.Value);
            }
        }

        EnemyMinionObjects = new Dictionary<string, GameObject>();


        yield return null;
    }

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
                    var botId = "Blue-" + placedAmountBlue;
                    blueMinion.name = botId;
                    blueMinion.GetComponent<MinionBot>().InitBot();
                    blueMinion.GetComponent<MinionBot>().UpdatePosition(x, y);
                    //TODO: Set location in minion class when it's made.
                    FriendlyMinionObjects.Add(botId, blueMinion);
                    placedBlue = true;
                    placedAmountBlue++;
                    _minionLocations[x, y] = botId;

                }
                else if(_blueSpawnerLocations[x, y] != null && placedBlue)
                {
                    placedBlue = false;
                }

                if(_redSpawnerLocations[x,y] != null && !placedRed && placedAmountRed < spawnAmountRed)
                {
                    var redBot = _levelDataHolder.TeamRedMinion;
                    GameObject redMinion = GameObject.Instantiate(redBot, (Vector3)_redSpawnerLocations[x, y], redBot.transform.rotation) as GameObject;
                    var botId = "Red-" + placedAmountRed;
                    redMinion.name = botId;
                    redMinion.GetComponent<MinionBot>().InitBot();
                    redMinion.GetComponent<MinionBot>().UpdatePosition(x, y);
                    //TODO: Set location in minion class when its made.
                    EnemyMinionObjects.Add(botId,redMinion);
                    placedRed = true;
                    placedAmountRed++;
                    _minionLocations[x, y] = botId;
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
    #endregion
    #region Input Stuff
    public void TrySelect(Vector3 mousePos,bool action)
    {
        RaycastHit hit;
        Ray ray = Camera.ScreenPointToRay(mousePos);

        if(Physics.Raycast(ray,out hit, 100.0f))
        {
            var tileData = hit.collider.gameObject.GetComponent<TileData>();
            var x = tileData.X;
            var y = tileData.Y;
            if (!action)
            {
                SelectMinion(x, y);
            }
            else
            {
                if(_selectedMinionName != "None")
                    TryMoveMinion(x, y);
            }
        }


    }

    public void RotateCamera(bool goLeft)
    {
        if (goLeft)
        {
            CameraHolder.AddTorque(new Vector3(0.0f, 1.0f, 0.0f), ForceMode.Acceleration);
        }
        else
        {
            CameraHolder.AddTorque(new Vector3(0.0f, -1.0f, 0.0f), ForceMode.Acceleration);
        }
    }
    private void SelectMinion(int x, int y)
    {
        if (!_fightHere[x, y])
        {
            if (_minionLocations[x, y] != null)
            {
                if (_minionLocations[x, y].Contains("Blue"))
                {
                    _selectedMinionName = _minionLocations[x, y];
                    _selectedMinion = GameObject.Find(_selectedMinionName);

                    _cursor.SetActive(true);
                    _cursor.transform.position = new Vector3((float)x, 0.0f, (float)y);

                }
            }
        }
    }

    public void TryMoveMinion(int x, int y)
    {
        var canMove = CanMoveHere(x, y);

        if (canMove)
        {
            var minionBot = _selectedMinion.GetComponent<MinionBot>();
            var oldX = minionBot.X;
            var oldY = minionBot.Y;

            //Move the selected minion object.
            _selectedMinion.transform.position = new Vector3((float)x,1.0f,(float)y);
            _selectedMinion.GetComponent<MinionBot>().UpdatePosition(x, y);
            //If the minion is moving on to an enemy minion then start a fight Coroutine and go until it's finished..
            //Also make that tile unselectable and remove the selection object.
            //Update the Level Data and minion locations with new ones based on who won the fight.
            _minionLocations[oldX, oldY] = null;
            _minionLocations[x, y] = _selectedMinionName;
            _cursor.transform.position = new Vector3((float)x, 0.0f, (float)y);
        }
    }

    private void UpdateMinionLocationsArray(string name, int newX, int newY)
    {
        for(var x = 0; x < LEVEL_SIZE; x++)
        {
            for(var y = 0; y < LEVEL_SIZE; y++)
            {
                if(_minionLocations[x,y] != null && _minionLocations[x,y] == name)
                {
                    _minionLocations[x, y] = null;
                }
            }
        }

        _minionLocations[newX, newY] = name;
    }

    private bool CanMoveHere(int x, int y)
    {
        if (_levelData[x,y] > 2)
            return false;

        if(_minionLocations[x,y] != null)
            if (_minionLocations[x, y].Contains("Blue"))
                return false;

        if (_fightHere[x, y])
            return false;

        return true;
    }



    #endregion
    #region Minion Stuff
    public void RemoveDeadMinion(string name, int x, int y)
    {
        _fightHere[x, y] = false;
        _minionLocations[x, y] = name;
        if (name.Contains("Blue"))
        {
            FriendlyMinionObjects.Remove(name);
            IncrementPoints(true, 2);
        }
        else if (name.Contains("Red"))
        {
            EnemyMinionObjects.Remove(name);
            IncrementPoints(true, 2);
        }

    }

    private void CheckRemainingMinions()
    {
        if(FriendlyMinionObjects.Count == 0)
        {
            //TODO: Player Lost round code here.
            
            IncrementPoints(false, 10);
            NextRound();

        }
        else if(EnemyMinionObjects.Count == 0)
        {
            //TODO: Player won round code here.
            IncrementPoints(true, 10);
            NextRound();
        }
    }
    #endregion

    #region Gameplay Stuff
    private void NextRound()
    {
        if(CurrentRound >= TotalRounds)
        {
            Debug.Log("WINNER");
            //TODO:Display Winner Message Here!
        }
        else
        {
            Debug.Log("Next Round");
            CurrentRound++;
            
            StartCoroutine(DestroyOldLevel());
            LoadNewLevel();
        }
    }


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
    
    private bool DidPlayerWin()
    {
        if(BlueTotalPoints > RedTotalPoints)
            return true;

        return false;
    }
    #endregion
}

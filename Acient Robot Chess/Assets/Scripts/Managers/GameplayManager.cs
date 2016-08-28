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
    public List<string> FriendlyFighting = new List<string>();
    public List<string> EnemyFighting = new List<string>();

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

    private bool _roundDone = false;

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
        CheckRemainingMinions();
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
        var maxCharge = 1.0f / (float)CurrentRound;
        maxCharge = 0.1f;
        _roundDone = false;
        StartCoroutine(AIProcess(maxCharge));

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
                _fightHere[x, y] = false;
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
        FriendlyFighting = new List<string>();
        if(EnemyMinionObjects.Count > 0)
        {
            foreach(var minion in EnemyMinionObjects)
            {
                Destroy(minion.Value);
            }
        }

        EnemyMinionObjects = new Dictionary<string, GameObject>();
        EnemyFighting = new List<string>();

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
                SelectMinion(x, y,false);
            }
            else
            {
                if(_selectedMinionName != "None")
                    TryMoveMinion(x, y,false,_selectedMinion.GetComponent<MinionBot>());
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
    private MinionBot SelectMinion(int x, int y, bool ai)
    {
        if (!_fightHere[x, y])
        {
            if (_minionLocations[x, y] != null)
            {
                if (_minionLocations[x, y].Contains("Blue") && !ai)
                {
                    _selectedMinionName = _minionLocations[x, y];
                    _selectedMinion = GameObject.Find(_selectedMinionName);
                    if (_selectedMinion != null)
                    {
                        _cursor.SetActive(true);
                        _cursor.transform.position = new Vector3((float)x, 0.0f, (float)y);
                        return _selectedMinion.GetComponent<MinionBot>();
                    }
                    else
                    {

                    }

                }
                else if(_minionLocations[x,y].Contains("Red") && ai)
                {
                    var minionName = _minionLocations[x, y];
                    return GameObject.Find(minionName).GetComponent<MinionBot>();
                }
            }  
        }
        return null;
    }

    public void TryMoveMinion(int x, int y,bool ai,MinionBot minionBot)
    {
        var canMove = CanMoveHere(x, y,ai);
        
        if (canMove)
        {
            var oldX = minionBot.X;
            var oldY = minionBot.Y;

            //Move the selected minion object.
            if (!ai)
            {
                _cursor.transform.position = new Vector3((float)x, 0.0f, (float)y);
            }
            minionBot.gameObject.transform.position = new Vector3((float)x,1.0f,(float)y);
            minionBot.UpdatePosition(x, y);


            if(_minionLocations[x,y] != null)
            {
                if (_minionLocations[x, y].Contains("Red") && !ai)
                {
                    _cursor.SetActive(false);
                    _selectedMinion = null;
                    _selectedMinionName = "None";
                    _fightHere[x, y] = true;
                    FriendlyFighting.Add(minionBot.gameObject.name);
                    EnemyFighting.Add(_minionLocations[x, y]);
                    StartCoroutine(Fight(minionBot.gameObject.name, _minionLocations[x, y], x, y, true));
                    return;
                }
                else if(_minionLocations[x,y].Contains("Blue") && ai)
                {
                    _fightHere[x, y] = true;
                    if(_minionLocations[x,y] == _selectedMinionName)
                    {
                        _selectedMinion = null;
                        _selectedMinionName = "None";
                        _cursor.SetActive(false);
                    }
                    EnemyFighting.Add(minionBot.gameObject.name);
                    FriendlyFighting.Add(_minionLocations[x, y]);
                    StartCoroutine(Fight( _minionLocations[x, y], minionBot.gameObject.name, x, y, false));
                    return;
                }
                else
                {
                    _minionLocations[oldX, oldY] = null;
                    _minionLocations[x, y] = minionBot.gameObject.name;
                }
            }
            //If the minion is moving on to an enemy minion then start a fight Coroutine and go until it's finished..
            //Also make that tile unselectable and remove the selection object.
            //Update the Level Data and minion locations with new ones based on who won the fight.

            
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

    private bool CanMoveHere(int x, int y,bool ai)
    {
        if (_levelData[x,y] > 2)
            return false;

        if (_minionLocations[x, y] != null)
            if (_minionLocations[x, y].Contains("Blue") && !ai || _minionLocations[x,y].Contains("Red") && ai)
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
            Destroy(FriendlyMinionObjects[name]);
            FriendlyMinionObjects.Remove(name);
            
            IncrementPoints(false, 2);
        }
        else if (name.Contains("Red"))
        {
            Destroy(EnemyMinionObjects[name]);
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

    #region Fight Stuff
    IEnumerator Fight(string blueName, string redName, int x, int y, bool playerAttackFirst)
    {
        var fightDone = false;
        var currentTime = 0.0f;
        var timePerRound = 0.5f;
        if (FriendlyMinionObjects.ContainsKey(blueName) && EnemyMinionObjects.ContainsKey(redName))
        {

            var blueMinion = FriendlyMinionObjects[blueName].GetComponent<MinionBot>();
            var redMinion = EnemyMinionObjects[redName].GetComponent<MinionBot>();

            Debug.Log("FIGHT FIGHT FIGHT");
            _fightHere[x, y] = true;
            var isBlueRound = playerAttackFirst;
            while (!fightDone)
            {
                if (currentTime >= timePerRound)
                {
                    if (isBlueRound)
                    {
                        var dmg = blueMinion.Attack();
                        Debug.Log("Blue dmg: " + dmg);
                        var dead = redMinion.TakeDamage(dmg);
                        if (dead)
                        {
                            _minionLocations[x, y] = blueMinion.gameObject.name;
                            fightDone = true;
                            FriendlyFighting.Remove(blueMinion.gameObject.name);
                            EnemyFighting.Remove(redMinion.gameObject.name);
                            RemoveDeadMinion(redMinion.gameObject.name, x, y);
                            _fightHere[x, y] = false;
                            Debug.Log("Blue Won");
                        }
                        currentTime = 0.0f;
                        isBlueRound = false;
                    }
                    else
                    {
                        var dmg = redMinion.Attack();
                        Debug.Log("Red dmg: " + dmg);
                        var dead = blueMinion.TakeDamage(dmg);
                        if (dead)
                        {
                            _minionLocations[x, y] = redMinion.gameObject.name;
                            fightDone = true;
                            FriendlyFighting.Remove(blueMinion.gameObject.name);
                            EnemyFighting.Remove(redMinion.gameObject.name);
                            RemoveDeadMinion(blueMinion.gameObject.name, x, y);
                            _fightHere[x, y] = false;
                            Debug.Log("Red Won");
                        }
                        currentTime = 0.0f;
                        isBlueRound = true;
                    }
                }
                else
                {
                    currentTime += Time.deltaTime;
                }

                yield return null;
            }
            _fightHere[x, y] = false;
        }
        _fightHere[x, y] = false;
        
        yield return null;
    }
    #endregion

    #region AI Stuff
    IEnumerator AIProcess(float maxChargeTime)
    {
        var currentTime = 0.0f;
        var names = new List<string>();

       
        while (!_roundDone)
        {
            
            if (currentTime > maxChargeTime)
            {
                names.Clear();
                foreach (var minion in EnemyMinionObjects)
                {
                    if (!EnemyFighting.Contains(minion.Key))
                        names.Add(minion.Key);
                }
                if (names.Count > 0)
                {
                    var selectedMinionNum = Random.Range(0, names.Count);
                    var selectedMinionObj = EnemyMinionObjects[names[selectedMinionNum]];
                    if (selectedMinionObj != null)
                    {
                        var selectedMinion = selectedMinionObj.GetComponent<MinionBot>();
                        var moveX = Random.Range(0, LEVEL_SIZE);
                        var moveY = Random.Range(0, LEVEL_SIZE);
                        TryMoveMinion(moveX, moveY, true, selectedMinion);
                    }
                }
                currentTime = 0.0f;
            }
            else
            {
                currentTime += Time.deltaTime;
            }
            yield return null;
        }

        yield return null;
    }
    #endregion
    #region Gameplay Stuff
    private void NextRound()
    {
        _roundDone = true;
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

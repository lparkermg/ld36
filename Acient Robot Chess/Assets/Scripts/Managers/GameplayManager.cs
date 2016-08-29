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

    //UI Stuff
    public GameplayUIManager UIManager;

    public GameObject FightMask;
    
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
        GameplayDataManager.MinionLocations = new string[LEVEL_SIZE, LEVEL_SIZE];
        GameplayDataManager.FightHere = new bool[LEVEL_SIZE, LEVEL_SIZE];
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
        UIManager.UpdateAll(BlueTotalPoints, RedTotalPoints, CurrentRound);
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
        var maxCharge = 2.0f / (float)CurrentRound;
        //maxCharge = 0.1f;
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

                GameplayDataManager.MinionLocations[x, y] = "None";
                GameplayDataManager.FightHere[x, y] = false;
            }
        }
        
        if(GameplayDataManager.FriendlyMinionObjects.Count > 0)
        {
            foreach(var minion in GameplayDataManager.FriendlyMinionObjects)
            {
                Destroy(minion.Value);
            }
        }

        GameplayDataManager.FriendlyMinionObjects = new Dictionary<string, GameObject>();
        GameplayDataManager.FriendlyFighting = new List<string>();
        if(GameplayDataManager.EnemyMinionObjects.Count > 0)
        {
            foreach(var minion in GameplayDataManager.EnemyMinionObjects)
            {
                Destroy(minion.Value);
            }
        }

        GameplayDataManager.EnemyMinionObjects = new Dictionary<string, GameObject>();
        GameplayDataManager.EnemyFighting = new List<string>();

        var excessMasks = GameObject.FindGameObjectsWithTag("FightMask");

        if(excessMasks.Length > 0)
        {
            for(var i = excessMasks.Length; i >= 0; i--)
            {
                Destroy(excessMasks[0]);
            }
        }
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
                    GameplayDataManager.FriendlyMinionObjects.Add(botId, blueMinion);
                    placedBlue = true;
                    placedAmountBlue++;
                    GameplayDataManager.MinionLocations[x, y] = botId;

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
                    GameplayDataManager.EnemyMinionObjects.Add(botId,redMinion);
                    placedRed = true;
                    placedAmountRed++;
                    GameplayDataManager.MinionLocations[x, y] = botId;
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
        var spawner = new Vector3((float)x, 0.5f, (float)y);
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
        if (!GameplayDataManager.FightHere[x, y])
        {
            if (GameplayDataManager.MinionLocations[x, y] != null)
            {
                if (GameplayDataManager.MinionLocations[x, y].Contains("Blue") && !ai)
                {
                    _selectedMinionName = GameplayDataManager.MinionLocations[x, y];
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
                else if(GameplayDataManager.MinionLocations[x,y].Contains("Red") && ai)
                {
                    var minionName = GameplayDataManager.MinionLocations[x, y];
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
            minionBot.gameObject.transform.position = new Vector3((float)x,0.5f,(float)y);
            minionBot.UpdatePosition(x, y);

            Debug.Log(GameplayDataManager.MinionLocations[x, y]);
            if(GameplayDataManager.MinionLocations[x,y] != null)
            {
                if (GameplayDataManager.MinionLocations[x, y].Contains("Red") && !ai)
                {
                    _cursor.SetActive(false);
                    _selectedMinion = null;
                    _selectedMinionName = "None";
                    GameplayDataManager.FightHere[x, y] = true;
                    GameplayDataManager.FriendlyFighting.Add(minionBot.gameObject.name);
                    GameplayDataManager.EnemyFighting.Add(GameplayDataManager.MinionLocations[x, y]);
                    GameObject fightMask = GameObject.Instantiate(FightMask, new Vector3(x, 0.5f, y), FightMask.transform.rotation) as GameObject;
                    fightMask.transform.SetParent(GameObject.Find(minionBot.gameObject.name).transform);
                    StartCoroutine(Fight(minionBot.gameObject.name, GameplayDataManager.MinionLocations[x, y], x, y, true,fightMask));
                    return;
                }
                else if(GameplayDataManager.MinionLocations[x,y].Contains("Blue") && ai)
                {
                    GameplayDataManager.FightHere[x, y] = true;
                    if(GameplayDataManager.MinionLocations[x,y] == _selectedMinionName)
                    {
                        _selectedMinion = null;
                        _selectedMinionName = "None";
                        _cursor.SetActive(false);
                    }
                    GameplayDataManager.EnemyFighting.Add(minionBot.gameObject.name);
                    GameplayDataManager.FriendlyFighting.Add(GameplayDataManager.MinionLocations[x, y]);
                    GameObject fightMask = GameObject.Instantiate(FightMask, new Vector3(x, 0.5f, y), FightMask.transform.rotation) as GameObject;
                    fightMask.transform.SetParent(GameObject.Find(minionBot.gameObject.name).transform);
                    StartCoroutine(Fight( GameplayDataManager.MinionLocations[x, y], minionBot.gameObject.name, x, y, false,fightMask));
                    return;
                }
                else
                {
                    GameplayDataManager.MinionLocations[oldX, oldY] = null;
                    GameplayDataManager.MinionLocations[x, y] = minionBot.gameObject.name;
                }
            }
            else
            {
                GameplayDataManager.MinionLocations[oldX, oldY] = null;
                GameplayDataManager.MinionLocations[x, y] = minionBot.gameObject.name;
            }

        }
    }

    private void UpdateMinionLocationsArray(string name, int newX, int newY)
    {
        for(var x = 0; x < LEVEL_SIZE; x++)
        {
            for(var y = 0; y < LEVEL_SIZE; y++)
            {
                if(GameplayDataManager.MinionLocations[x,y] != null && GameplayDataManager.MinionLocations[x,y] == name)
                {
                    GameplayDataManager.MinionLocations[x, y] = null;
                }
            }
        }

        GameplayDataManager.MinionLocations[newX, newY] = name;
    }

    private bool CanMoveHere(int x, int y,bool ai)
    {
        if (_levelData[x,y] > 2)
            return false;

        if (GameplayDataManager.MinionLocations[x, y] != null)
            if (GameplayDataManager.MinionLocations[x, y].Contains("Blue") && !ai || GameplayDataManager.MinionLocations[x,y].Contains("Red") && ai)
                return false;

        if (GameplayDataManager.FightHere[x, y])
            return false;

        return true;
    }



    #endregion
    #region Minion Stuff
    public void RemoveDeadMinion(string name, int x, int y)
    {
        GameplayDataManager.FightHere[x, y] = false;
        GameplayDataManager.MinionLocations[x, y] = name;
        if (name.Contains("Blue"))
        {
            Destroy(GameplayDataManager.FriendlyMinionObjects[name]);
            GameplayDataManager.FriendlyMinionObjects.Remove(name);
            
            
        }
        else if (name.Contains("Red"))
        {
            Destroy(GameplayDataManager.EnemyMinionObjects[name]);
            GameplayDataManager.EnemyMinionObjects.Remove(name);

        }

    }

    private void CheckRemainingMinions()
    {
        if(GameplayDataManager.FriendlyMinionObjects.Count == 0)
        {
            //TODO: Player Lost round code here.
            
            IncrementPoints(false, 10);
            NextRound();

        }
        else if(GameplayDataManager.EnemyMinionObjects.Count == 0)
        {
            //TODO: Player won round code here.
            IncrementPoints(true, 10);
            NextRound();
        }
    }
    #endregion

    #region Fight Stuff
    IEnumerator Fight(string blueName, string redName, int x, int y, bool playerAttackFirst,GameObject fightMask)
    {
        var fightDone = false;
        var currentTime = 0.0f;
        var timePerRound = 1f;
        if (GameplayDataManager.FriendlyMinionObjects.ContainsKey(blueName) && GameplayDataManager.EnemyMinionObjects.ContainsKey(redName))
        {

            var blueMinion = GameplayDataManager.FriendlyMinionObjects[blueName].GetComponent<MinionBot>();
            var redMinion = GameplayDataManager.EnemyMinionObjects[redName].GetComponent<MinionBot>();

            Debug.Log("FIGHT FIGHT FIGHT");
            GameplayDataManager.FightHere[x, y] = true;
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
                            GameplayDataManager.MinionLocations[x, y] = blueMinion.gameObject.name;
                            fightDone = true;
                            GameplayDataManager.FriendlyFighting.Remove(blueMinion.gameObject.name);
                            GameplayDataManager.EnemyFighting.Remove(redMinion.gameObject.name);
                            RemoveDeadMinion(redMinion.gameObject.name, x, y);
                            RemoveDeadMinion(blueMinion.gameObject.name, x, y);
                            IncrementPoints(true, 2);
                            GameplayDataManager.FightHere[x, y] = false;
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
                            GameplayDataManager.MinionLocations[x, y] = redMinion.gameObject.name;
                            fightDone = true;
                            GameplayDataManager.FriendlyFighting.Remove(blueMinion.gameObject.name);
                            GameplayDataManager.EnemyFighting.Remove(redMinion.gameObject.name);
                            RemoveDeadMinion(redMinion.gameObject.name, x, y);
                            RemoveDeadMinion(blueMinion.gameObject.name, x, y);
                            IncrementPoints(false, 2);
                            GameplayDataManager.FightHere[x, y] = false;
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

            Destroy(fightMask);

        }
        GameplayDataManager.FightHere[x, y] = false;
        
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
                foreach (var minion in GameplayDataManager.EnemyMinionObjects)
                {
                    if (!GameplayDataManager.EnemyFighting.Contains(minion.Key))
                        names.Add(minion.Key);
                }
                if (names.Count > 0)
                {
                    var selectedMinionNum = Random.Range(0, names.Count);
                    var selectedMinionObj = GameplayDataManager.EnemyMinionObjects[names[selectedMinionNum]];
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
            if(BlueTotalPoints == RedTotalPoints)
            {
                Debug.Log("Draw!!");
            }
            else if(BlueTotalPoints > RedTotalPoints)
            {
                Debug.Log("Winrar!");
            }
            else if(BlueTotalPoints < RedTotalPoints)
            {
                Debug.Log("Loser!!");
            }
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
            UIManager.UpdatePlayerPoints(BlueTotalPoints);
        }
        else
        {
            RedTotalPoints += amount;
            UIManager.UpdateComputerPoints(RedTotalPoints);
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

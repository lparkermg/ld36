using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GameplayDataManager
{
    public static string[,] MinionLocations;
    public static bool[,] FightHere;

    public static Dictionary<string, GameObject> FriendlyMinionObjects = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> EnemyMinionObjects = new Dictionary<string, GameObject>();
    public static List<string> FriendlyFighting = new List<string>();
    public static List<string> EnemyFighting = new List<string>();
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameplayUIManager : MonoBehaviour {

    public Text RoundNumber;
    public Text PlayerPoints;
    public Text ComputerPoints;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateAll(int playerPts, int compPts, int round)
    {
        PlayerPoints.text = playerPts.ToString("000");
        ComputerPoints.text = compPts.ToString("000");
        RoundNumber.text = round.ToString("0");
    }

    public void UpdatePlayerPoints(int points)
    {
        PlayerPoints.text = points.ToString("000");
    }

    public void UpdateComputerPoints(int points)
    {
        ComputerPoints.text = points.ToString("000");
    }

    public void UpdateRundNumber(int number)
    {
        RoundNumber.text = number.ToString("0");
    }
}

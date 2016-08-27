using UnityEngine;
using System.Collections;

public class SceneChangers : MonoBehaviour {

    public int SceneNumber;

    public bool IsTimed = false;

    public float TimeToChange = 2.0f;
    private float _currentTime;

    public KeyCode ButtonToChange;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (IsTimed)
            CheckTime();
        else
            CheckInput();
	}

    private void CheckInput()
    {
        if (Input.GetKeyDown(ButtonToChange))
            GameManager.Instance.LoadScene(SceneNumber);
    }

    private void CheckTime()
    {
        if(TimeToChange <= _currentTime)
        {
            GameManager.Instance.LoadScene(SceneNumber);
        }
        else
        {
            _currentTime += Time.deltaTime;
        }
    }
}

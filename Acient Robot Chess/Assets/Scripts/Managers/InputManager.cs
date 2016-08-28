using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

    public GameplayManager GameplayManager;

    public void CheckInput()
    {
        MouseCheck();
        KeyboardCheck();
    }

    private void MouseCheck()
    {
        if (Input.GetMouseButtonUp(0))
        {
            GameplayManager.TrySelect(Input.mousePosition, false);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            GameplayManager.TrySelect(Input.mousePosition, true);
        }
    }

    private void KeyboardCheck()
    {
        if(Input.GetAxis("Horizontal") > 0.05f)
        {
            GameplayManager.RotateCamera(false);
        }
        else if(Input.GetAxis("Horizontal") < -0.05f)
        {
            GameplayManager.RotateCamera(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.LoadScene(1);
        }
    }
}

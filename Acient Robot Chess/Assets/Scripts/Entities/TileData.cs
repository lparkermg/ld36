using UnityEngine;
using System.Collections;

public class TileData : MonoBehaviour {

    public int X = 0;
    public int Y = 0;

    private Material _matInstance;

    void Start()
    {
        _matInstance = GetComponent<Renderer>().material;
        _matInstance.EnableKeyword("_DisplayEmitAmount");
    }

    public void UpdateData(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Activate()
    {
        _matInstance.SetFloat("_DisplayEmitAmount", 1.0f);
        GetComponent<Renderer>().material = _matInstance;
    }

    public void Deactivate()
    {
        _matInstance.SetFloat("_DisplayEmitAmount", 0.0f);
        GetComponent<Renderer>().material = _matInstance;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSetup : MonoBehaviour {

    public List<AudioClip> BGMClips;
    public List<AudioClip> SfxClips;
    public AudioClip BattleWinSfxClip;
    public AudioClip BattleLostSfxClip;
    public AudioClip RoundWinClip;
    public AudioClip RoundLostClip;

    // Use this for initialization
    void Start () {
        GameManager.Instance.Initialise();
        GameManager.Instance.PopulateBgmClips(BGMClips);
        GameManager.Instance.PopulateSfxClips(SfxClips);
        var audioSource = GameObject.FindGameObjectWithTag("BgmSource").GetComponent<AudioSource>();
        audioSource.volume = GameManager.Instance.MusicVolume / 1.5f;
        audioSource.clip = GameManager.Instance.GetRandomAudioClip();
        audioSource.Play();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

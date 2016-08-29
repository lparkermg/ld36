using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    public AudioSource BgmAudioSource;

	// Use this for initialization
	void Start () {
        BgmAudioSource.volume = GameManager.Instance.MusicVolume;
	}
	
	// Update is called once per frame
	void Update () {
        IsStillPlaying();
        UpdateMusicVolume();
	}

    public void UpdateMusicVolume()
    {
        if(BgmAudioSource.volume != GameManager.Instance.MusicVolume)
        {
            BgmAudioSource.volume = GameManager.Instance.MusicVolume;
        }
    }

    private void IsStillPlaying()
    {
        if(!BgmAudioSource.isPlaying)
        {
            BgmAudioSource.clip = GameManager.Instance.GetRandomAudioClip();
            BgmAudioSource.Play();
        }
    }
}

using UnityEngine;
using System.Collections;
namespace Entities
{
    public class Settings
    {
        public float MusicVolume = 0.5f;
        public float SfxVolume = 0.5f;

        public Settings()
        {

        }

        public Settings(float musicVol,float sfxVol)
        {
            MusicVolume = musicVol;
            SfxVolume = sfxVol;
        }
        
    }
}

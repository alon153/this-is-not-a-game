using FMODUnity;
using UnityEngine;

namespace Audio
{
    public partial class AudioManager
    {
        [Header("Sounds")] 
        [SerializeField] private EventReference _defaultDash;
        [SerializeField] private EventReference _defaultCollision;
        [SerializeField] private MusicSounds _defaultMusic;
        [SerializeField] private EventReference _selectButton;
        [SerializeField] private EventReference _clickButton;
        [SerializeField] private EventReference _countDownButton;
        
        
        public static void SelectSound()
        {
            PlayOneShot(_instance._selectButton);
        }

        public static void ClickSound()
        {
            PlayOneShot(_instance._clickButton);
        }
    
        public static void CountDownSound()
        {
            PlayOneShot(_instance._countDownButton);
        }
        
        public static void PlayDash()
        {
            RuntimeManager.PlayOneShot(_instance._dashEvent);
        }

        public static void PlayAction()
        {
            if(!_instance._actionEvent.IsNull)
                RuntimeManager.PlayOneShot(_instance._dashEvent);
        }
    
        public static void PlayFall()
        {
            if(!_instance._fallEvent.IsNull)
                RuntimeManager.PlayOneShot(_instance._fallEvent);
        }
    }
}
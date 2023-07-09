﻿using FMODUnity;
using UnityEngine;

namespace Audio
{
    public partial class AudioManager
    {
        [Header("Sounds")] 
        [SerializeField] private EventReference _defaultDash;
        [SerializeField] private EventReference _defaultCollision;
        [SerializeField] private EventReference _defaultDeath;
        [SerializeField] private EventReference _defaultMove;
        [SerializeField] private EventReference _playerEnter;
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
            if(!DashEvent.IsNull)
                RuntimeManager.PlayOneShot(DashEvent);
        }

        public static void PlayAction()
        {
            if(!ActionEvent.IsNull)
                RuntimeManager.PlayOneShot(ActionEvent);
        }
    
        public static void PlayFall()
        {
            if(!FallEvent.IsNull)
                RuntimeManager.PlayOneShot(FallEvent);
        }

        public static void PlayCollision()
        {
            if(!CollisionEvent.IsNull)
                RuntimeManager.PlayOneShot(CollisionEvent);
        }

        public static void PlayPlayerEnter()
        {
            if(!_instance._playerEnter.IsNull)
                RuntimeManager.PlayOneShot(_instance._playerEnter);
        }
    }
}
using System;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using Object = System.Object;

namespace Audio
{
    [CreateAssetMenu(fileName = "Audio Bank", menuName = "ScriptableObjects/Audio/Audio Bank", order = 0)]
    public class AudioBank : ScriptableObject
    {
        #region Serialized Fields
        
        [field: SerializeField] public EventReference MusicEventReference { get; private set; }
        [SerializeField] private List<PlayerRefPair> _playerRefs = new();
        [SerializeField] private List<SfxRefPair> _sfxRefs = new();

        #endregion

        #region None-Serialized Fields

        private Dictionary<SoundType, Object> _refsDict;

        #endregion

        public EventReference this[SoundType soundType, int sound]
        {
            get
            {
                return soundType switch
                {
                    SoundType.Player => _playerRefs[sound]._reference,
                    SoundType.Sfx => _sfxRefs[sound]._reference
                };
            }
        }

        private void OnEnable()
        {
            InitSoundDictionary();
        }

        private void OnValidate()
        {
            InitSoundDictionary();

            foreach ((var key, var value) in _refsDict)
            {
                switch (key)
                {
                    case SoundType.Player:
                        ((List<PlayerRefPair>) value).Sort(((pair1, pair2) => pair1._type - pair2._type));
                        break;
                    case SoundType.Sfx:
                        ((List<SfxRefPair>) value).Sort(((pair1, pair2) => pair1._type - pair2._type));
                        break;
                }    
            }
        }

        private void InitSoundDictionary()
        {
            if (_refsDict == null)
            {
                _refsDict = new Dictionary<SoundType, Object>()
                {
                    {SoundType.Player, _playerRefs},
                    {SoundType.Sfx, _sfxRefs}
                };
            }
        }
    }

    #region Classes

    [Serializable]
    public abstract class SoundRefPair
    {
        public abstract int Type { get; }
        public EventReference _reference;
    }

    [Serializable]
    public class PlayerRefPair : SoundRefPair
    {
        public PlayerSounds _type;
        public override int Type => (int) _type;
    }

    [Serializable]
    public class SfxRefPair : SoundRefPair
    {
        public SfxSounds _type;
        public override int Type => (int) _type;
    }
    

    public enum SoundType
    {
        Player = 0,
        Sfx,
        Music,
    }

    public enum PlayerSounds
    {
        Dash = 0,
        DashCooldown,
    }
    
    
    public enum MusicSounds
    {
        Lobby = 0,
        Game,
        Pool,
        Lasers,
        Paint,
        Ikea,
        Rhythm,
        Boats,
        Island
    }

    public enum SfxSounds
    {
        Noise
    }

    #endregion
}
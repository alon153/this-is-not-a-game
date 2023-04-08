using System;
using System.Collections.Generic;
using System.Linq;
using GameMode.Ikea;
using GameMode.Modes;
using GameMode.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameMode
{
    [CreateAssetMenu(fileName = "GameModeFactory", menuName = "ScriptableObjects/GameModeFactory")]
    public class GameModeFactory: ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private PaintMode _paintMode;

        [SerializeField] private PoolMode _poolMode;

        [SerializeField] private IkeaMode _ikeaMode;

        #endregion
        
        #region None-Serialized Fields

        private List<GameModes> _availableModes;
        private List<GameModes> _playedModes;
        private Queue<GameModes> _startWith;
        
        #endregion

        #region Public Methods

        public void Init()
        {
            _availableModes = Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList();
            _playedModes = new List<GameModes>();
        }

        public void Init(List<GameModes> availableModes)
        {
            _availableModes = new(availableModes);
            _playedModes = new List<GameModes>();
        }
        
        public void Init(List<GameModes> startWith, List<GameModes> availableModes = null)
        {
            _startWith = new Queue<GameModes>(startWith);
            _availableModes = availableModes == null || availableModes.Count == 0 
                ? Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList() 
                : new(availableModes);
            _availableModes.RemoveAll((mode => _startWith.Contains(mode))); //remove duplicates in startWith and available
            _playedModes = new List<GameModes>();
        }

        public void Init(GameModes mode)
        {
            _availableModes = new List<GameModes>();
            _availableModes.Add(mode);
            _playedModes = new List<GameModes>();
        }

        public GameModeBase GetGameMode()
        {
            GameModes mode = GetModeEnum();
            return mode switch
            {
                GameModes.Paint => _paintMode,
                GameModes.Pool => _poolMode,
                _ => throw new Exception("No mode to create")
            };
        }

        #endregion

        #region Private Methods
        
        private GameModes GetModeEnum()
        {
            GameModes mode = GameModes.Null;

            if (_startWith != null && _startWith.Count > 0)
            {
                mode = _startWith.Dequeue();
            }
            else if(_availableModes.Count > 0)
            {
                int index = Random.Range(0, _availableModes.Count);
                mode = _availableModes[index];
                _availableModes.RemoveAt(index);
            }
            
            if(mode != GameModes.Null)
                _playedModes.Add(mode);
            
            return mode;
        }

        #endregion
    }
}
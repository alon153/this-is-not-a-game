using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace GameMode
{
    public class GameModeFactory
    {
        #region Fields

        private List<GameModes> _availableModes;
        private List<GameModes> _playedModes;
        private Queue<GameModes> _startWith;

        public GameModeFactory()
        {
            _availableModes = Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList();
            _playedModes = new List<GameModes>();
        }

        public GameModeFactory(List<GameModes> availableModes)
        {
            _availableModes = new(availableModes);
            _playedModes = new List<GameModes>();
        }
        
        public GameModeFactory(List<GameModes> startWith, List<GameModes> availableModes = null)
        {
            _startWith = new Queue<GameModes>(startWith);
            _availableModes = availableModes == null || availableModes.Count == 0 
                ? Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList() 
                : new(availableModes);
            _availableModes.RemoveAll((mode => _startWith.Contains(mode))); //remove duplicates in startWith and available
            _playedModes = new List<GameModes>();
        }

        public GameModeBase GetGameMode()
        {
            GameModes mode = GetModeEnum();
            return mode switch
            {
                GameModes.Ikea => throw new NotImplementedException(),
                GameModes.Pool => throw new NotImplementedException(),
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
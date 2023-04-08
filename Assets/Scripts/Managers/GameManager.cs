using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Basics;
using GameMode;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Utilities;
using PlayerController = Basics.Player.PlayerController;

namespace Managers
{
    public class GameManager : SingletonPersistent<GameManager>
    {
        #region Serialized Fields

        [SerializeField] private Arena _defaultArenaPrefab;
        
        [field: SerializeField] public List<Color> PlayerColors { get; private set; }

        [Header("Round Settings")] 
        [SerializeField] private int _roundLength;
        [SerializeField] private int _numRounds;
        
        [Header("Mode Factory Settings")]
        [SerializeField] private GameModeFactory _gameModeFactory;
        [SerializeField] private bool _isSingleMode;
        [SerializeField] private GameModes _singleMode;
        [SerializeField] private List<GameModes> _startWith;

        #endregion

        #region None-Serialized Fields

        private List<PlayerController> _players = new();

        private HashSet<int> _playerIds = new();

        private GameModeBase _gameMode;
        private int _roundsPlayed = 0;

        #endregion

        #region Properties

        public List<PlayerController> Players => Instance._players;
        public Arena DefaultArena { get; private set; }

        #endregion

        #region Event Functions

        public override void Awake()
        {
            base.Awake();
            Init();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called at each player's Start() event function.
        /// Handles all logistics needed for new players
        /// </summary>
        /// <param name="controller">
        /// The newly connected player
        /// </param>
        /// <returns>The index of the player</returns>
        public int RegisterPlayer(PlayerController controller)
        {
            if (_playerIds.Contains(controller.GetInstanceID()))
                return Players.FindIndex((playerController => playerController.GetInstanceID() == controller.GetInstanceID()));

            int index = _players.Count;

            _players.Add(controller);
            _playerIds.Add(controller.GetInstanceID());
            ScoreManager.Instance.SetNewPlayerScore(index);

            return index;
        }

        /// <summary>
        /// If the number of rounds played exceeded the number of rounds set for the whole game -> End game
        /// Else -> Generate a new game mode and start it
        /// </summary>
        public void NextRound()
        {
            if(_gameMode != null)
                return;
            
            if (_roundsPlayed >= _numRounds)
            {
                EndGame();
                return;
            }
            
            _gameMode = _gameModeFactory.GetGameMode();
            if(_gameMode == null)
            {
                EndGame();
                return;
            }
            
            UIManager.Instance.ActivateScoreDisplays();
            _gameMode.InitRound();
            TimeManager.Instance.StartCountDown(_roundLength);
        }
        
        /// <summary>
        /// Called by TimeManager when the current round's time is up.
        /// Calls the GameMode's OnTimeOver().
        /// </summary>
        public void OnTimeOver()
        {
            if(_gameMode == null)
                return;
            
            _gameMode.OnTimeOVer();
        }

        /// <summary>
        /// Updates number of rounds played and clears the round that has ended.
        /// </summary>
        public void EndRound()
        {
            if(_gameMode == null)
                return;

            _roundsPlayed++;
            _gameMode.ClearRound();
            _gameMode = null;
        }

        #endregion

        #region Private Methods
        
        private void EndGame()
        {
            print("End");
        }

        /// <summary>
        /// Inits the GameModeFactory.
        /// If singleMode was checked -> Init the factory with a single mode
        /// Else -> Init the factory with the StartWith list
        /// </summary>
        private void Init()
        {
            if (_isSingleMode)
                _gameModeFactory.Init(_singleMode);
            else
                _gameModeFactory.Init(_startWith);
            
        }        

        #endregion

        public void FreezePlayers(bool timed=true, float time=2)
        {
            foreach (var player in Players)
            {
                player.Freeze(timed, time);
            }
        }
        
        public void UnFreezePlayers()
        {
            foreach (var player in Players)
            {
                player.UnFreeze();
            }
        }

        public Arena GetCurArena()
        {
            return _gameMode.ModeArena;
        }

    }
}
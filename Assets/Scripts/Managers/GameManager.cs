﻿using System;
using System.Collections.Generic;
using System.Linq;
using Basics;
using GameMode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Utilities;
using PlayerController = Basics.Player.PlayerController;

namespace Managers
{
    public class GameManager : SingletonPersistent<GameManager>
    {
        #region Serialized Fields

        [field: SerializeField] private Arena DefaultArenaPrefab;
        
        [field: SerializeField] public List<Color> PlayerColors { get; private set; }

        [Header("Round Settings")] 
        [SerializeField] private int _roundLength;
        [SerializeField] private int _numRounds = 10;
        
        [Header("Mode Factory Settings")]
        [SerializeField] private GameModeFactory _gameModeFactory;
        [SerializeField] private bool _isSingleMode;
        [SerializeField] private GameModes _singleMode;
        [SerializeField] private List<GameModes> _modes;

        #endregion

        #region None-Serialized Fields

        private readonly List<PlayerController> _players = new();
        private readonly List<bool> _readys = new List<bool>();

        private HashSet<int> _playerIds = new();

       
        private GameModeBase _gameMode;
        private int _roundsPlayed = 0;
        private PlayerInputManager _inputManager;

        private Arena _currArena;
        
        #endregion

        #region Properties

        public List<PlayerController> Players => Instance._players;
        
        public UnityAction GameModeUpdateAction { get; set;}

        public Arena CurrArena
        {
            get => _currArena;
            set
            {
                if(_currArena != null)
                    Destroy(_currArena.gameObject);
                _currArena = value;
            }
        }

        #endregion

        #region Event Functions

        public override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Update()
        {
            if (!GameModeUpdateAction.Equals(null))
                GameModeUpdateAction.Invoke();
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
            
            _readys.Add(false);
            TimeManager.Instance.StopCountDown();

            ScoreManager.Instance.SetNewPlayerScore(index);

            PlacePlayer(controller, index);

            return index;
        }

        private void PlacePlayer(PlayerController controller, int index)
        {
            var controllerTrans = controller.transform;
            controllerTrans.position = index switch
            {
                0 => (CurrArena.TopLeft + CurrArena.Center) / 2,
                1 => (CurrArena.BottomRight + CurrArena.Center) / 2,
                2 => (CurrArena.TopRight + CurrArena.Center) / 2,
                3 => (CurrArena.BottomLeft + CurrArena.Center) / 2,
                _ => controllerTrans.position
            };
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
            
            UIManager.Instance.SetGameDesc(_gameMode.Name, _gameMode.Description);
            TimeManager.Instance.StartCountDown(5, (() =>
            {
                UIManager.Instance.ToggleGameDesc(false);
                _gameMode.InitRound();
                TimeManager.Instance.StartCountDown(_roundLength, OnTimeOver);
                UnFreezePlayers();
            }), UIManager.CountDownTimer.Main);
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
            TimeManager.Instance.StopCountDown();
            _gameMode?.EndRound();
        }

        /// <summary>
        /// Updates number of rounds played and clears the round that has ended.
        /// </summary>
        public void ClearRound()
        {
            if(_gameMode == null)
                return;

            _roundsPlayed++;
            _gameMode.ClearRound();
            _gameMode = null;
            NextRound();
        }

        #endregion

        #region Private Methods
        
        private void EndGame()
        {
            print($"Player {ScoreManager.Instance.GetWinner()} wins!");
        }

        private void StartGame()
        {
            _inputManager.enabled = false;
            foreach (var player in Players)
            {
                player.Ready = false;
            }
            UIManager.Instance.ActivateScoreDisplays();
            NextRound();
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
                _gameModeFactory.Init(_modes);
            CurrArena = Instantiate(DefaultArenaPrefab);
            _inputManager = GetComponent<PlayerInputManager>();
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

        public void SetReady(int index, bool value)
        {
            if(index < 0 || index >= _readys.Count)
                return;
            _readys[index] = value;
            
            if(_readys.Contains(false))
                TimeManager.Instance.StopCountDown();
            else
                TimeManager.Instance.StartCountDown(5, StartGame, UIManager.CountDownTimer.Main);
        }
    }
}
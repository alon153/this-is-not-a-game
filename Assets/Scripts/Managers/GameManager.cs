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

        [SerializeField] private Arena _arenaPrefab;
        
        [field: SerializeField] public List<Color> PlayerColors { get; private set; }

        [Header("GameMode")]
        [SerializeField] private GameModeFactory _gameModeFactory;
        [SerializeField] private bool _isSingleMode;
        [SerializeField] private GameModes _singleMode;
        [SerializeField] private List<GameModes> _startWith;

        #endregion

        #region None-Serialized Fields

        private List<PlayerController> _players = new();

        private HashSet<int> _playerIds = new();

        private GameModeBase _gameMode;

        #endregion

        #region Properties

        public static List<PlayerController> Players => Instance._players;
        public Arena Arena { get; private set; }

        #endregion

        #region Event Functions

        private void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            if (_isSingleMode)
                _gameModeFactory.Init(_singleMode);
            else
                _gameModeFactory.Init(_startWith);
            Arena = Instantiate(_arenaPrefab);
        }

        #endregion

        #region Public Methods

        public void RegisterPlayer(PlayerController controller)
        {
            if (_playerIds.Contains(controller.GetInstanceID()))
                return;

            controller.Index = _players.Count;
            controller.Renderer.color = PlayerColors[controller.Index];
            
            _players.Add(controller);
            _playerIds.Add(controller.GetInstanceID());
            ScoreManager.Instance.SetNewPlayerScore(controller.GetInstanceID());
        }

        public void ToggleRound()
        {
            if (_gameMode == null)
            {
                _gameMode = _gameModeFactory.GetGameMode();
                _gameMode.InitRound();
            }
            else
            {
                _gameMode.ClearRound();
                _gameMode = null;
            }
        }

        #endregion
    }
}
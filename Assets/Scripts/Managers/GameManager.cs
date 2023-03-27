using System;
using System.Collections.Generic;
using Basics;
using GameMode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utilities;
using PlayerController = Basics.Player.PlayerController;

namespace Managers
{
    public class GameManager : SingletonPersistent<GameManager>
    {
        #region Serialized Fields

        [SerializeField] private List<GameModes> _startWithModes;
        [field: SerializeField] public Arena Arena { get; private set; }
        
        #endregion

        #region None-Serialized Fields

        private List<PlayerController> _players = new();

        private HashSet<int> _playerIds = new();

        private GameModeFactory _gameModeFactory;

        #endregion

        #region Properties

        public static List<PlayerController> Players => Instance._players; 

        #endregion

        #region Event Functions

        private void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            _gameModeFactory = new GameModeFactory(_startWithModes);
        }

        #endregion

        #region Public Methods

        public void RegisterPlayer(PlayerController controller)
        {
            if(_playerIds.Contains(controller.GetInstanceID()))
                return;

            controller.Index = _players.Count;
            _players.Add(controller);
            _playerIds.Add(controller.GetInstanceID());
            ScoreManager.Instance.SetNewPlayerScore(controller.GetInstanceID());
        }

        #endregion
    }
}
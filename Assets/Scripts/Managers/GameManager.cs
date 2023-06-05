using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Audio;
using Basics;
using Basics.Player;
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

        [field: SerializeField] private List<PlayerData> PlayerDatas;
        [SerializeField] private Sprite _defaultPlayerSprite;

        [Header("Round Settings")] 
        [SerializeField] private int _roundLength;
        [SerializeField] private int _numRounds = 10;
        [field: SerializeField] public bool Zap { get; private set; }= false;
        [SerializeField] private float _zapLength = 0.5f;
        [field: SerializeField] public float InstructionsTime { get; private set; } = 3f;
        
        [Header("Mode Factory Settings")]
        [SerializeField] private GameModeFactory _gameModeFactory;
        [SerializeField] private bool _isSingleMode;
        [SerializeField] private GameModes _singleMode;
        [SerializeField] private List<GameModes> _modes;

        [Header("Debugging")] 
        
        [Tooltip("enable this check box if you want the game modes to appear in the order they are in list and " +
                 "not randomly")]
        [SerializeField] private bool _isGameModeByOrder = false;

        #endregion

        #region None-Serialized Fields

        private readonly List<PlayerController> _players = new();
        private readonly List<bool> _readys = new List<bool>();

        private HashSet<int> _playerIds = new();

      
        private int _roundsPlayed = 0;
        private PlayerInputManager _inputManager;

        private Arena _currArena;
        private GameState _state = GameState.Lobby;

        #endregion

        #region Properties

        public List<PlayerController> Players => Instance._players;

        public GameState CurrentState => _state;
        
        public UnityAction GameModeUpdateAction { get; set;}  
        public GameModeBase GameMode { get; private set; }

        public Color PlayerColor(int i) => PlayerDatas[i]._bloomColor;
        public AnimatorOverrideController PlayerAnimatorOverride(int i) => PlayerDatas[i]._animatorOverride;

        public Arena CurrArena
        {
            get => _currArena;
            set
            {
                var arenaObj = _currArena ? _currArena.gameObject : null; 
                _currArena = value;
                    if (arenaObj)
                        Destroy(arenaObj);

                foreach (var player in Players)
                {
                    if (_currArena.OutOfArena(player.transform.position)) player.Fall();
                }
            }
        }

        #endregion

        #region Event Functions

        public override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Start()
        {
            AudioManager.SetMusic(MusicSounds.Lobby);
        }

        private void Update()
        {
            GameModeUpdateAction?.Invoke();
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

        public void StartRound()
        {
            UIManager.Instance.HideAllMessages();
            TimeManager.Instance.StartCountDown(_roundLength, OnTimeOver);
            UnFreezePlayers();
        }

        /// <summary>
        /// If the number of rounds played exceeded the number of rounds set for the whole game -> End game
        /// Else -> Generate a new game mode and start it
        /// </summary>
        public void NextRound()
        {
            UIManager.Instance.ToggleFlash(true);

            GameMode?.ClearRound();

            _roundsPlayed++;
            if (_roundsPlayed > _numRounds)
            {
                EndGame();
            }
            else
            {
                GameMode = _gameModeFactory.GetGameMode(_isGameModeByOrder);
                if(GameMode == null)
                    EndGame();
                else
                {
                    FreezePlayers(false);
                    GameMode.InitRound();
                    if(InstructionsTime > 0)
                        UIManager.Instance.ShowInstructions(GameMode.Name, GameMode.InstructionsSprite, StartRound);
                    else
                        StartRound();
                }
            }
            
            TimeManager.Instance.DelayInvoke((() =>
            {
                UIManager.Instance.ToggleFlash(false);
            }), _zapLength);
        }
        
        /// <summary>
        /// Called by TimeManager when the current round's time is up.
        /// Calls the GameMode's OnTimeOver().
        /// </summary>
        public void OnTimeOver()
        {
            if(GameMode == null)
                return;
            
            GameMode?.OnTimerOver();
        }

        /// <summary>
        /// Updates number of rounds played and clears the round that has ended.
        /// </summary>
        public void EndRound()
        {
            TimeManager.Instance.StopCountDown();
            
            GameMode?.EndRound();
        }

        #endregion

        #region Private Methods
        
        private void EndGame()
        {
            CurrArena = Instantiate(DefaultArenaPrefab);
            print($"Player {ScoreManager.Instance.GetWinner()} wins!");
            UIManager.Instance.ShowWinner(ScoreManager.Instance.GetWinner());
        }

        private void StartGame()
        {
            _inputManager.enabled = false;
            foreach (var player in Players)
            {
                player.Ready = false;
            }
            
            AudioManager.SetMusic(MusicSounds.Game);
            _state = GameState.Playing;
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
            if(_state != GameState.Lobby)
                return;
            
            if(index < 0 || index >= _readys.Count)
                return;
            _readys[index] = value;
            
            if(_readys.Contains(false))
                TimeManager.Instance.StopCountDown();
            else
                TimeManager.Instance.StartCountDown(5, StartGame, UIManager.CountDownTimer.Main);
        }

        public int GetRoundLength()
        {
            return _roundLength;
        }

        public void SetDefaultSprite(PlayerController player)
        {
            player.Renderer.RegularSprite = _defaultPlayerSprite;
            player.Renderer.RegularColor = PlayerDatas[player.Index]._bloomColor;
        }
        
    }
    
   

    public enum GameState
    {
        Lobby, Playing
    }

    [Serializable]
    public class PlayerData
    {
        public AnimatorOverrideController _animatorOverride;
        public Color _bloomColor;
    }
}
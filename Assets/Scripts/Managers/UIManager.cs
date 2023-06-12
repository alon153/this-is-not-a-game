using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace Managers
{
    public class UIManager : SingletonPersistent<UIManager>
    {

        #region Serialized Fields
        
        [Header("General Texts")]
        [SerializeField] private TextMeshProUGUI _centerText;
        
        [Header("\nTimers")]
        [SerializeField] private TextMeshProUGUI _gameTimeText;
        [SerializeField] private TextMeshProUGUI _mainTimeText;

        [Header ("\nMisc")]
        [SerializeField] private TextMeshProUGUI[] playerScoreTexts;
        [SerializeField] private Image _flash;
        [SerializeField] private Animator _flashAnimatior;
        
        [Header("\nMenus")]
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private GameObject _firstPauseMenuBtn;
        [SerializeField] private GameObject _firstSettingsMenuBtn;
        [SerializeField] private GameObject _instructionsOffBtn;
        [SerializeField] private GameObject _instructionsOnBtn;

        [SerializeField] private EventSystem _eventSystem;

        #endregion
        
        #region Non-Serialized Fields
        
        private Dictionary<int, TextMeshProUGUI> _playerScoreDisplay = new Dictionary<int, TextMeshProUGUI>();
        
        private int _activeScoreDisplays = 0;

        private int _maxScoreDisplays;

        private TransitionWindow _transitionWindow;
        
        private static readonly int PlayFizz = Animator.StringToHash("PlayFizz");

        #endregion

        #region MonoBehaviour Methods

        public override void Awake()
        {
            base.Awake();
            _transitionWindow = GetComponentInChildren<TransitionWindow>();
        }

        private void Start()
        {
            _maxScoreDisplays = playerScoreTexts.Length;
            _transitionWindow.HideWindow(true);
        }
        #endregion

        #region Private Methods

        private string FormatTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        #endregion

        #region Public Methods
        
        public void ShowInstructions(string title, Sprite instructions)
        {
            _transitionWindow.ShowWindow(title, instructions);
        }
        
        public void HideInstructions()
        {
            _transitionWindow.HideWindow();
        }
        
        /// <summary>
        /// will lead to settings menu if in lobby, will lead to pause menu if in game. 
        /// </summary>
        public void OnPressStart()
        {
            switch (GameManager.Instance.State)
            {
               case GameState.Lobby:
                    _eventSystem.SetSelectedGameObject(_firstSettingsMenuBtn);
                    _settingsMenu.SetActive(true);
                    GameManager.Instance.State = GameState.SettingsMenu;
                    break;
                    
                case GameState.Playing: 
                    _eventSystem.SetSelectedGameObject(_firstPauseMenuBtn);
                    _pauseMenu.SetActive(true);
                    _transitionWindow.ShowWindow();
                    GameManager.Instance.State = GameState.PauseMenu;
                    break;
                
                case GameState.Instructions:
                case GameState.MainMenu:
                case GameState.PauseMenu:
                case GameState.SettingsMenu:
                    return;
            }

            Time.timeScale = 0;
        }

        public void OnReturnToGame()
        {   
            switch (GameManager.Instance.State){

                case GameState.PauseMenu:
                    _transitionWindow.HideWindow();
                    _pauseMenu.SetActive(false);
                    GameManager.Instance.State = GameState.Playing;
                    break;
                case GameState.SettingsMenu:
                    _settingsMenu.SetActive(false);
                    GameManager.Instance.State = GameState.Lobby;
                    break;
                
                case GameState.Lobby:
                case GameState.Playing:
                case GameState.Instructions:
                case GameState.MainMenu:
                    return;
            }
            Time.timeScale = 1;
        }

        public void ToggleInstructions()
        {
            GameManager.Instance.instructionsMode = !GameManager.Instance.instructionsMode;
            if (GameManager.Instance.instructionsMode)
            {
                _instructionsOffBtn.SetActive(false);
                _instructionsOnBtn.SetActive(true);
                _eventSystem.SetSelectedGameObject(_instructionsOnBtn);
            }

            else
            {
                _instructionsOffBtn.SetActive(true);
                _instructionsOnBtn.SetActive(false);
                _eventSystem.SetSelectedGameObject(_instructionsOffBtn);
            }
        }

        public void OnPressReset()
        {   
            Time.timeScale = 1;
            _pauseMenu.SetActive(false);
            _transitionWindow.HideWindow();
            GameManager.Instance.OnReset();
            GameManager.Instance.State = GameState.Lobby;
        }

        public void ToggleFlash(bool show)
        {
            _flash.gameObject.SetActive(show);
            if (show)
                _flashAnimatior.SetTrigger(PlayFizz);
        }

        /// <summary>
        /// Called when a new player is registered to game. Function attributes a text display to player score.
        /// </summary>
        /// <param name="playerId">
        /// unique player Id determined by game manager. 
        /// </param>
        public void SetNewPlayerDisplay(int playerId)
        {
            if (_maxScoreDisplays == _activeScoreDisplays)
            {
                Debug.LogError("Error: Trying to set a new player but reached maximum amount." +
                               "Available displays:" + _playerScoreDisplay.Count);
                return;
            }
            
            var color = GameManager.Instance.PlayerColor(playerId);
            playerScoreTexts[_activeScoreDisplays].color = color;
            _transitionWindow.SetPlayerColor(playerId, color);
            _playerScoreDisplay.Add(playerId, playerScoreTexts[_activeScoreDisplays]);
            _activeScoreDisplays += Constants.NewPlayerRegistered;
        }
        
        /// <summary>
        /// called when starting the round, function sets all displays that are linked with a player
        /// and sets score to zero. 
        /// </summary>
        public void ActivateScoreDisplays()
        {
            foreach (var key in _playerScoreDisplay.Keys)
            {
                _playerScoreDisplay[key].text = Constants.InitialScore;
                _playerScoreDisplay[key].gameObject.SetActive(true);
            }
        }

        public void SetScoreToPlayerDisplay(int playerId, float newScore)
        {
            // todo: make this juicier! add a coroutine or something!
            _playerScoreDisplay[playerId].text = newScore.ToString("0");
        }

        public void ToggleCenterText(bool show) => _centerText.gameObject.SetActive(show);

        public void HideAllMessages()
        {
            ToggleCenterText(false);
            HideInstructions();
        }

        /// <summary>
        /// can be called at game end to restart all displays. 
        /// </summary>
        public void ResetScoreDisplays()
        {
            foreach (var display in playerScoreTexts)
            {
                display.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called by TimeManager to update the display of the time left
        /// </summary>
        public void UpdateTime(float time, CountDownTimer timer = CountDownTimer.Game)
        {
            TextMeshProUGUI timerText = timer switch
            {
                CountDownTimer.Main => _mainTimeText,
                CountDownTimer.Game => _gameTimeText,
            };
            
            timerText.text = FormatTime((int) time);
            timerText.enabled = time > 0;
        }


        #endregion

        public enum CountDownTimer
        {
            Game,
            Main,
            Transition,
        }

        public void ShowWinner(int winner)
        {
            _centerText.color = GameManager.Instance.PlayerColor(winner);
            _centerText.text = $"Player {winner + 1} wins!";
            ToggleCenterText(true);
        }
        
        public void HideWinner()
        {
            ToggleCenterText(false);
        }

        public void StartCountdown(Action onEnd=null)
        {
            _transitionWindow.StartCountdown(onEnd);
        }

        public void InstructionsReady(int index, bool ready)
        {
            _transitionWindow.SetReady(index, ready);
        }
    }
}
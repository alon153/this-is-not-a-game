using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

namespace Managers
{
    public class UIManager : Singleton<UIManager>
    {

        #region Serialized Fields

        [Header("Lobby Readies")] 
        [SerializeField] private GameObject _lobbyReadiesContainer;
        [SerializeField] private GameObject[] _lobbyReadies;
        
        [Header("General Texts")]
        [SerializeField] private TextMeshProUGUI _centerText;
        
        [Header("\nTimers")]
        [SerializeField] private TextMeshProUGUI _gameTimeText;
        [SerializeField] private TextMeshProUGUI _mainTimeText;

        [Header ("\nMisc")]
        [SerializeField] private ScoreBlock[] _playerScores;
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

        private int _activeScoreDisplays = 0;

        private int _maxScoreDisplays;

        private TransitionWindow _transitionWindow;
        
        private static readonly int PlayFizz = Animator.StringToHash("PlayFizz");

        private int _currWinner = -1;

        private List<Animator> _lobbyReadiesAnimators = new();
        private static readonly int Press = Animator.StringToHash("Press");

        #endregion
        
        #region Properties

        public int CurrWinner
        {
            get => _currWinner;
            set
            {
                if(_currWinner != -1)
                    _playerScores[_currWinner].Lower();
                _playerScores[value].Raise();
                _currWinner = value;
            }
        }
        
        #endregion

        #region MonoBehaviour Methods

        public void Awake()
        {
            _transitionWindow = GetComponentInChildren<TransitionWindow>();
        }

        private void Start()
        {
            _maxScoreDisplays = _playerScores.Length;
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
                    GameManager.Instance.State = GameState.PauseMenu;
                    _transitionWindow.ShowWindow();
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

        public void OnPressMainMenu()
        {
            Time.timeScale = 1;
            AudioManager.SetMusic(MusicSounds.Lobby);
            SceneManager.LoadScene("Scenes/Opening");
        }

        public void ToggleFlash(bool show)
        {
            _flash.gameObject.SetActive(show);
            if (show)
                _flashAnimatior.SetTrigger(PlayFizz);
        }

        public void ToggleLobbyReadies(bool show)
        {
            _lobbyReadiesContainer.SetActive(show);
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
                               "Available displays:" + _playerScores.Length);
                return;
            }
            
            var color = GameManager.Instance.PlayerColor(playerId);

            _transitionWindow.EnableReadyButton(playerId);
            _lobbyReadies[playerId].SetActive(true);
            _lobbyReadiesAnimators.Add(_lobbyReadies[playerId].GetComponent<Animator>());

            _activeScoreDisplays += Constants.NewPlayerRegistered;

            for (int i = 0; i < _activeScoreDisplays; i++)
            {
                _playerScores[i].AnchorMinX = ((float) i) / _activeScoreDisplays;
                _playerScores[i].AnchorMaxX = ((float) i+1) / _activeScoreDisplays;
            }
        }
        
        public void ActivateScoreDisplays()
        {
            for(int i=0;i<_activeScoreDisplays;i++)
            {
                _playerScores[i].Lower(); // calling lower will raise from height 0 to default height
            }
        }

        public void SetScoreToPlayerDisplay(int playerId, float newScore)
        {
            // todo: make this juicier! add a coroutine or something!
            _playerScores[playerId].Score = newScore;
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
            for(int i=0;i<_activeScoreDisplays;i++)
            {
                _playerScores[i].Score = 0;
                _playerScores[i].MoveTo(0,true);
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
        
        public void LobbyReady(int index, bool ready)
        {
            _lobbyReadiesAnimators[index].SetBool(Press, ready);
        }
        
        #endregion
        
        public enum CountDownTimer
        {
            Game,
            Main,
            Transition,
        }
    }
}
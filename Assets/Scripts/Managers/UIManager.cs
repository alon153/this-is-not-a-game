using System;
using System.Collections;
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
        
        [Header("\nPause Menus")]
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private GameObject _firstPauseMenuBtn;
        [SerializeField] private GameObject _firstSettingsMenuBtn;
       
        [Header("\nInstructionMenu")]
        [SerializeField] private GameObject _instructionsOffBtn;
        [SerializeField] private GameObject _instructionsOnBtn;
        [SerializeField] private Image _instructionsBackground;

        [Header("\nWiningMenu")] 
        [SerializeField] private GameObject _endScreen;
        [SerializeField] private List<PlayerFinalScore> _finalScoreDisplays;
        [SerializeField] private GameObject _firstEndScreenBtn;
        
        
        [SerializeField] private EventSystem _eventSystem;

        [SerializeField] private RectTransform _scoreBlockParent;

        #endregion
        
        #region Non-Serialized Fields

        private int _activeScoreDisplays = 0;

        private int _maxScoreDisplays;

        private TransitionWindow _transitionWindow;
        private ChannelNameSlide _channelWindow;
        
        private static readonly int PlayFizz = Animator.StringToHash("PlayFizz");

        private int _currWinner = -1;

        private List<Animator> _lobbyReadiesAnimators = new();
        private static readonly int Press = Animator.StringToHash("Press");

        private Coroutine _fadeCoroutine = null;

        #endregion
        
        #region Properties

        public int CurrWinner
        {
            get => _currWinner;
            set
            {
                if(GameManager.Instance.State != GameState.Lobby && _currWinner != -1)
                    _playerScores[_currWinner].Lower();
                _playerScores[value].Raise();
                _currWinner = value;
            }
        }
        
        #endregion
        
        #region NestedClasses
        
        [Serializable]
        class PlayerFinalScore
        {
            [SerializeField] private Image _scoreImage;

            [SerializeField] private TextMeshProUGUI _scoreTxt;
            public void SetScore(int score) => _scoreTxt.text = score.ToString();
            public void SetImgColor(Color newColor) => _scoreImage.color = newColor;
            public void ToggleImage(bool activate = true) => _scoreImage.gameObject.SetActive(activate);
        }
        
        
        #endregion

        #region MonoBehaviour Methods

        public void Awake()
        {
            _transitionWindow = GetComponentInChildren<TransitionWindow>();
            _channelWindow = GetComponentInChildren<ChannelNameSlide>();
        }

        private void Start()
        {
            _maxScoreDisplays = _playerScores.Length;
            HideInstructions(true);
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

        public void ShowRoundEndScreen()
        {
            _endScreen.SetActive(true);
            _eventSystem.SetSelectedGameObject(_firstEndScreenBtn);
        }

        public void ShowInstructions(Sprite instructions=null, Sprite channel=null, bool immediate=false)
        {
            _transitionWindow.ShowWindow(instructions, immediate);
            _channelWindow.ShowWindow(channel, immediate);
            _instructionsBackground.gameObject.SetActive(true);
        }
        
        public void HideInstructions(bool immediate=false)
        {
            _transitionWindow.HideWindow(immediate);
            _channelWindow.HideWindow(immediate);
            _instructionsBackground.gameObject.SetActive(false);
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
                    ShowInstructions();
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
                    HideInstructions();
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
            HideInstructions();
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

            _scoreBlockParent.anchoredPosition = new Vector2(
                _activeScoreDisplays % 2 == 0 ? 0 : -130,
                _scoreBlockParent.anchoredPosition.y);
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
            
            timerText.text = timer == CountDownTimer.Game ? FormatTime((int) time) : Mathf.CeilToInt(time).ToString();
            
            if (time > 0)
                timerText.enabled = true;

            if (timer == CountDownTimer.Game && time <= 5)
            {
                _mainTimeText.enabled = true;
                _mainTimeText.text = Mathf.CeilToInt(time).ToString();
            }

            if (timer == CountDownTimer.Main || time <= 5)
            {
                if(_fadeCoroutine != null)
                    StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(MainTextFade(2, 0));
            }

            if (time <= 0)
            {
                _mainTimeText.enabled = false;
                _gameTimeText.enabled = false;
            }
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
        
        public void StopCountdown()
        {
            _transitionWindow.StopCountdown();
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

        #region Private Methods

        private IEnumerator MainTextFade(float duration, float targetScale)
        {
            var color = _mainTimeText.color;
            var scale = _mainTimeText.transform.localScale;

            float t = 1;

            color.a = t;
            scale.x = t;
            scale.y = t;

            _mainTimeText.color = color;
            _mainTimeText.transform.localScale = scale;

            yield return null;

            float time = 0;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                t = (duration - time) / duration;
                color.a = t;
                _mainTimeText.color = color;
                
                scale.x = t + (1 - t) * targetScale;
                scale.y = t + (1 - t) * targetScale;
                _mainTimeText.transform.localScale = scale;
                
                yield return null;
            }

            color.a = 0;
            scale.x = targetScale;
            scale.y = targetScale;

            _mainTimeText.color = color;
            _mainTimeText.transform.localScale = scale;

            _fadeCoroutine = null;
        }

        private void SetDisplaysScoreAndColor()
        {
            
        }
        

        #endregion
        
        public enum CountDownTimer
        {
            Game,
            Main,
            Transition,
        }

        public void StopFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }
    }
}
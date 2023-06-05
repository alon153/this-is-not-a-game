using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Managers
{
    public class UIManager : SingletonPersistent<UIManager>
    {

        #region Serialized Fields
        
        [Header("General Texts")]
        [SerializeField] private TextMeshProUGUI _centerText;
        
        [Header("Timers")]
        [SerializeField] private TextMeshProUGUI _gameTimeText;
        [SerializeField] private TextMeshProUGUI _mainTimeText;

        [Header ("Misc")]
        [SerializeField] private TextMeshProUGUI[] playerScoreTexts;
        [SerializeField] private Image _flash;

        #endregion
        
        #region Non-Serialized Fields
        
        private Dictionary<int, TextMeshProUGUI> _playerScoreDisplay = new Dictionary<int, TextMeshProUGUI>();
        
        private int _activeScoreDisplays = 0;

        private int _maxScoreDisplays;

        private TransitionWindow _transitionWindow;

        #endregion

        #region Constants

        private const string InitialScore = "0";

        private const int NewPlayerRegistered = 1;

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
        
        public void ShowInstructions(string title, Sprite instructions, Action onEnd=null)
        {
            _transitionWindow.ShowWindow(title, instructions, onEnd: onEnd);
        }
        
        public void HideInstructions()
        {
            _transitionWindow.HideWindow();
        }

        public void ToggleFlash(bool show) => _flash.gameObject.SetActive(show);

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
            
            
            playerScoreTexts[_activeScoreDisplays].color = GameManager.Instance.PlayerColor(playerId);
            _playerScoreDisplay.Add(playerId, playerScoreTexts[_activeScoreDisplays]);
            _activeScoreDisplays += NewPlayerRegistered;
        }
        
        /// <summary>
        /// called when starting the round, function sets all displays that are linked with a player
        /// and sets score to zero. 
        /// </summary>
        public void ActivateScoreDisplays()
        {
            foreach (var key in _playerScoreDisplay.Keys)
            {
                _playerScoreDisplay[key].text = InitialScore;
                _playerScoreDisplay[key].gameObject.SetActive(true);
            }
        }

        public void SetScoreToPlayerDisplay(int playerId, float newScore)
        {
            // todo: make this juicier! add a coroutine or something!
            _playerScoreDisplay[playerId].text = newScore.ToString("0.00");
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
            _playerScoreDisplay.Clear();
            _activeScoreDisplays = 0;

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
    }
}
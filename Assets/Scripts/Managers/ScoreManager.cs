using System;
using System.Collections.Generic;
using Basics.Player;
using GameMode;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class ScoreManager : SingletonPersistent<ScoreManager>
    {

        #region Serialized Fields

        [SerializeField] private FloatingPoints _floatingPointsPrefab;        

        #endregion
        
        #region Non-Serialized Fields
        
        private Dictionary<int, float> _playerScores = new Dictionary<int, float>();
        
        #endregion

        #region Constants

        private const float InitialScore = 0f; 

        #endregion

        #region Mono-Beahviour Methods

        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// called by gameManager when a new player is registered in game. 
        /// </summary>
        public void SetNewPlayerScore(int playerId)
        {
            _playerScores.Add(playerId, InitialScore);
            UIManager.Instance.SetNewPlayerDisplay(playerId);
        }
        
        public float GetPlayerScore(int playerId)
        {
            return _playerScores[playerId];
        }

        public int GetWinner()
        {
            int maxInd = -1;
            float maxScore = 0;
            for (int i = 0; i < _playerScores.Count; i++)
            {
                if (_playerScores[i] >= maxScore)
                {
                    maxScore = _playerScores[i];
                    maxInd = i;
                }
            }

            return maxInd;
        }
        
        /// <summary>
        /// used to add or subtract score to player. 
        /// </summary>
        /// <param name="playerId">
        /// unique player id determined by game manager. 
        /// </param>
        /// <param name="score">
        /// new score. 
        /// </param>
        /// <param name="shouldAdd">
        /// if true, the points will be added to score. if false, the points will be subtracted from player score.   
        /// </param>
        public void SetPlayerScore(int playerId, float score, bool shouldAdd=true, bool showFloat=true)
        {
            switch (shouldAdd)
            {
                case true:
                    _playerScores[playerId] += score;
                    if (showFloat)
                    {
                        PlayerController player = GameManager.Instance.Players[playerId];
                        FloatingPoints fp = Instantiate(_floatingPointsPrefab, player.transform.position, Quaternion.identity);
                        fp.Color = player.Color;
                        fp.Text = score >= 0 ? $"+{score}" : $"{score}";
                        fp.Float();
                    }
                    break;
                
                case false:
                    _playerScores[playerId] -= score;
                    if (_playerScores[playerId] < InitialScore)
                        _playerScores[playerId] = InitialScore;
                    break;
            }

            UIManager.Instance.SetScoreToPlayerDisplay(playerId, _playerScores[playerId]);
        }

        /// <summary>
        /// Adds scores for multiple players
        /// </summary>
        /// <param name="scores">Pairings of playerId (keys) and score to add (value)</param>
        /// <param name="shouldAdd">
        /// if true, the points will be added to score. if false, the points will be subtracted from player score.
        /// </param>
        public void SetPlayerScores(Dictionary<int,float> scores, bool shouldAdd = true, bool showFloat = true)
        {
            foreach (var pair in scores)
            {
                SetPlayerScore(pair.Key, pair.Value, shouldAdd, showFloat);
            }
        }

        public void ResetScore()
        {
            _playerScores.Clear();
        }

        #endregion
        
        
        
    }
}

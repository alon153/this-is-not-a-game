using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Basics;
using Basics.Player;
using Object = UnityEngine.Object;


namespace GameMode.Juggernaut
{   
    [Serializable]
    public class JuggernautGameMode : GameModeBase
    {   
        
        #region Serialized Fields  
        
        [SerializeField] private Totem totemPrefab;

        [Tooltip("how many hits can a player take before dropping the totem")]
        [SerializeField] private int hitAmount = 60;
        
        [Tooltip("How many hits does bashing a player from the arena counts")]
        [SerializeField] private int hitsPerDeath = 15;
        
        [SerializeField] private int totalRoundScore = 100;
        
        #endregion

        #region Non-Serialized Fields

        private Totem _totem;

        private bool _isAPlayerHoldingTotem = false;

        private PlayerController _currTotemHolder = null;
        
        #endregion

        #region GameModeBase
        protected override void InitRound_Inner()
        {
            GameManager.Instance.GameModeUpdateAction += JuggernautModeUpdate;
            foreach (var player in GameManager.Instance.Players)
                player.Addon = new JuggernautPlayerAddOn();

        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            GameManager.Instance.CurrArena = arena;
            _totem = Object.Instantiate(totemPrefab, arena.Center, Quaternion.identity);
            _totem.OnTotemPickedUp += OnTotemPickedUp;
        }

        protected override void ClearRound_Inner()
        {
            Object.Destroy(_totem.gameObject);
            foreach (var player in GameManager.Instance.Players)
                player.Addon = null;
        }

        protected override void OnTimeOver_Inner()
        {
            GameManager.Instance.GameModeUpdateAction -= JuggernautModeUpdate;
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            Dictionary<int, float> scores = new Dictionary<int, float>();
            int roundLen = GameManager.Instance.GetRoundLength();
            foreach (var player in GameManager.Instance.Players)
            {   
                PlayerAddon.CheckCompatability(player.Addon, GameModes.Juggernaut);
                var timeWithTotem = ((JuggernautPlayerAddOn) player.Addon).TotalTimeYieldingTotem / roundLen;
                scores.Add(player.Index, timeWithTotem * totalRoundScore);    
            }

            return scores;
        }
        #endregion
        
        #region Private Methods

        private void OnTotemPickedUp(PlayerController player)
        {
            _totem.gameObject.SetActive(false);
            _currTotemHolder = player;
            _isAPlayerHoldingTotem = true;
            Debug.Log("totem was picked up by player: " + player.Index);
        }

        private void JuggernautModeUpdate()
        {
            if (_isAPlayerHoldingTotem)
            {   
                Debug.Log("got here: " + _isAPlayerHoldingTotem);
                PlayerAddon.CheckCompatability(_currTotemHolder.Addon, GameModes.Juggernaut);
                ((JuggernautPlayerAddOn) _currTotemHolder.Addon).TotalTimeYieldingTotem += Time.deltaTime;
            }
        }

        #endregion
        
        #region Public Methods
        
        
        #endregion
        
    }
}

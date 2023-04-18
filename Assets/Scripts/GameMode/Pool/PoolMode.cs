using System;
using System.Collections.Generic;
using Basics;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Interfaces;
using Utilities.Interfaces;
using Object = UnityEngine.Object;

namespace GameMode.Pool
{   
    [Serializable]
    
    public class PoolMode : GameModeBase, IOnFallListener
    {   
        #region Serialized Fields
        
        [Tooltip("list stores pool holes object deactivated, will be activated when starting a new pool round")]

        [SerializeField] private float scoreOnHit = 10f;

        #endregion
        
        #region Non Serialized Fields

        private List<GameObject> _poolHoles = new List<GameObject>();
        private Dictionary<int, float> _hits = new Dictionary<int, float>();

        private GameObject _arenaBorders;

        #endregion

        #region GameModeBase Methods
        protected override void InitRound_Inner()
        {
            foreach (PlayerController player in GameManager.Instance.Players)
                player.RegisterFallListener(this);              
            
        }

        protected override void InitArena_Inner()
        {
            _poolHoles.Clear();
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            foreach (Transform parentArenaObj in arena.transform)
            {
                foreach (Transform arenaObj in parentArenaObj.transform)
                {
                    var poolArenaObj = arenaObj.gameObject;
                                   
                    if (poolArenaObj.CompareTag("PoolHole"))
                        _poolHoles.Add(poolArenaObj);
                    
                    else if (poolArenaObj.CompareTag("PoolBorders"))
                        _arenaBorders = poolArenaObj;
                }
            }

            GameManager.Instance.CurrArena = arena;
        }

        protected override void ClearRound_Inner()
        {
            foreach (PlayerController player in GameManager.Instance.Players)
                player.UnRegisterFallListener(this);              
            
            foreach (var hole in _poolHoles)
                Object.Destroy(hole);
            
            _poolHoles.Clear();
            Object.Destroy(_arenaBorders);
            _arenaBorders = null;

        }

        protected override void OnTimeOver_Inner() { }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            Dictionary<int, float> scores = new Dictionary<int, float>();
            foreach (var pair in _hits)
            {
                scores[pair.Key] = _hits[pair.Key] * scoreOnHit;
            }

            return scores;
        }

        /// <summary>
        ///  called by adding player on fall listeners. checks if the player that fell was bashed
        ///  by another player. if so it grants points to the player. 
        /// </summary>
        /// <param name="playerFalling">
        /// the player that fell.
        /// </param>
        public void OnFall(PlayerController playerFalling)
        {
            PlayerController playerBashing = playerFalling.GetBashingPlayer();
            if (playerBashing != null)
            {
                if (!_hits.ContainsKey(playerBashing.Index))
                    _hits[playerBashing.Index] = 0;
                _hits[playerBashing.Index]++;
            }
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// toggles pool holes activation, not currently used. 
        /// </summary>
        /// <param name="activate">
        /// if true, holes will be activated. otherwise, holes will be Deactivated.  
        /// </param>
        private void TogglePoolHoles(bool activate)
        {
            foreach (GameObject hole in _poolHoles)
                hole.SetActive(activate);
        }

        #endregion
    }
}

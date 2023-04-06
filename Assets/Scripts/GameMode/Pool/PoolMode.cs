using System;
using System.Collections.Generic;
using Basics.Player;
using Managers;
using UnityEngine;
using Utilities.Listeners;

namespace GameMode.Pool
{   
    [Serializable]
    
    public class PoolMode : GameModeBase, IOnFallIntoHoleListener
    {   
        #region Serialized Fields
        
        [Tooltip("list stores pool holes object deactivated, will be activated when starting a new pool round")]
        [SerializeField] private GameObject poolHolesParent;

        [SerializeField] private float scoreOnHit = 10f;

        #endregion
        
        #region Non Serialized Fields

        private List<GameObject> _poolHoles = new List<GameObject>();
        
        #endregion

        #region GameModeBase Methods
        public override void InitRound()
        {   
           
            foreach (Transform hole in poolHolesParent.transform)
                _poolHoles.Add(hole.gameObject);
            
            TogglePoolHoles(true);
        }

        public override void ClearRound()
        {
            TogglePoolHoles(false);
        }

        public override void OnTimeOVer()
        {
            GameManager.Instance.FreezePlayers(timed: false);
        }
        
        public void OnFallIntoHall(PlayerController playerFalling)
        {
            PlayerController playerBashing = playerFalling.GetBashingPlayer();
            if (playerBashing != null)
            {   
                playerFalling.Freeze();
                playerFalling.Fall();
                ScoreManager.Instance.SetPlayerScore(playerBashing.GetInstanceID(), scoreOnHit);
            }
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// toggles pool holes activation
        /// </summary>
        /// <param name="activate">
        /// if true, holes will be activated. otherwise, holes will be Deactivated.  
        /// </param>
        private void TogglePoolHoles(bool activate)
        {
            foreach (var hole in _poolHoles)
                hole.SetActive(activate);
        }

        #endregion

       
    }
}

using System;
using System.Collections.Generic;
using Basics.Player;
using Managers;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Utilities.Listeners;
using Object = UnityEngine.Object;

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
            Debug.Log("got here");
            GameObject parent = Object.Instantiate(poolHolesParent);
            foreach (Transform hole in parent.transform)
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

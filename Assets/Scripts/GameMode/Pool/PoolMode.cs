using System;
using System.Collections.Generic;
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
        [SerializeField] private GameObject poolHolesParent;

        [SerializeField] private float scoreOnHit = 10f;

        #endregion
        
        #region Non Serialized Fields

        private List<GameObject> _poolHoles = new List<GameObject>();

        #endregion

        #region GameModeBase Methods
        public override void InitRound()
        {   
           
            GameObject parent = Object.Instantiate(poolHolesParent, poolHolesParent.transform.position, 
                Quaternion.identity);
            foreach (Transform hole in parent.transform)
            {
                var holeObj = hole.gameObject;
                holeObj.SetActive(true);
                _poolHoles.Add(holeObj);
            }
            
            // todo fix null bug here!!!
            //TogglePoolHoles(true);

            foreach (PlayerController player in GameManager.Instance.Players)
            {
                player.RegisterFallListener(this);              
            }
        }

        public override void ClearRound()
        {
            TogglePoolHoles(false);
            foreach (PlayerController player in GameManager.Instance.Players)
            {
                player.UnRegisterFallListener(this);              
            }
        }

        public override void OnTimeOVer()
        {
            GameManager.Instance.FreezePlayers(timed: false);
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
                ScoreManager.Instance.SetPlayerScore(playerBashing.Index, scoreOnHit);
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
            foreach (GameObject hole in _poolHoles)
                hole.SetActive(activate);
        }

        #endregion
    }
}

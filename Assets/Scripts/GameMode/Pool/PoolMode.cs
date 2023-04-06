using System;
using System.Collections.Generic;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Events;
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

        private UnityEvent<PlayerController> _onFallEvent = new UnityEvent<PlayerController>();
        
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
                holeObj.GetComponent<PoolHole>().SetUpPoolMode(this);
                _poolHoles.Add(holeObj);
            }
            
            // todo fix null bug here!!!
            //TogglePoolHoles(true);
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
               ScoreManager.Instance.SetPlayerScore(playerBashing.Index, scoreOnHit);
            }
            playerFalling.Freeze();
            playerFalling.Fall();
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

using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace GameMode.Pool
{   
    [Serializable]
    
    public class PoolMode : GameModeBase
    {   
        #region Serialized Fields
        
        [Tooltip("list stores pool holes object deactivated, will be activated when starting a new pool round")]
        [SerializeField] private List<GameObject> poolHoles = new List<GameObject>();

        #endregion
        
        #region Non Serialized Fields
        
        
        #endregion

        #region GameModeBase Methods
        public override void InitRound()
        {
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
            foreach (var hole in poolHoles)
                hole.SetActive(activate);
        }

        #endregion
    }
}

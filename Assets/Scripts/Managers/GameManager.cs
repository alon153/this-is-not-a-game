using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {

        #region None-Serialized Fields
        
        private List<PlayerController> _players = new();
        private HashSet<int> _playerIds = new();

        #endregion
        
        #region Properties

        public static GameManager Instance { get; private set; }
        public static List<PlayerController> Players => Instance._players; 

        #endregion

        #region Event Functions

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion

        #region Public Methods

        public void RegisterPlayer(PlayerController controller)
        {
            if(_playerIds.Contains(controller.GetInstanceID()))
                return;

            controller.Index = _players.Count;
            _players.Add(controller);
            _playerIds.Add(controller.GetInstanceID());
        }

        #endregion
    }
}
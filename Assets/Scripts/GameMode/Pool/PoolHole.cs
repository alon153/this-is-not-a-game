using System;
using System.Collections;
using System.Collections.Generic;
using Basics.Player;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Pool
{
    public class PoolHole : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private List<BoxCollider2D> holeBordersColliders = new List<BoxCollider2D>();
        
        [SerializeField] Action<PlayerController> fallenToHoleEvent;

        #endregion

        #region Non Serlized Fields

        private enum Borders {Top, Bottom, Left, Right}

        #endregion
        
        #region MonoBehaviour Methods
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                // player falls inside the hole, will probably change later
                playerController.Fall();
                
                // player was pushed to the hole by another player.
                if (playerController.GetBashingPlayer() != null)
                {   
                    // need to check how to pass arguments to event 
                    fallenToHoleEvent.Invoke(playerController);
                }
            }
        }

        #endregion

        #region Private Methods
        
        
        
        #endregion

        #region Public Methods

        

        #endregion
        
        
        
    }
    
}

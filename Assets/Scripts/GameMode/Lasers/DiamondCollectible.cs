using System;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Lasers
{
    public class DiamondCollectible : MonoBehaviour
    {
        #region Serialized Fields
        
        [Tooltip("how much points the player gets for collecting that diamond")]
        [SerializeField] private int diamondValue;

        public UnityAction<DiamondCollectible> OnDiamondPickedUp;
       
        #endregion

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                foreach (var player in GameManager.Instance.Players)
                {
                    if (player.gameObject.GetInstanceID().Equals(other.gameObject.GetInstanceID()))
                    {   
                        // todo: is there a better way to do it or there's something i do not understand?
                        LaserPlayerAddon playerAddon = (LaserPlayerAddon) player.Addon;
                        playerAddon.DiamondsCollected += 1;
                    }
                }
                OnDiamondPickedUp.Invoke(this);
            }
        }
    }
}
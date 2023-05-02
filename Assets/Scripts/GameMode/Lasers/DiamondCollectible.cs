using System;
using Basics;
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
                        PlayerAddon.CheckCompatability(player.Addon, GameModes.Lasers);
                        LaserPlayerAddon playerAddon = (LaserPlayerAddon) player.Addon;
                        playerAddon.DiamondsCollected += 1;
                    }
                }
                OnDiamondPickedUp.Invoke(this);
            }
        }
    }
}
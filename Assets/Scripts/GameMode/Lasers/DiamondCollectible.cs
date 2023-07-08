using System;
using Audio;
using Basics;
using Basics.Player;
using FMODUnity;
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
        [SerializeField] private EventReference _slurp;

        public UnityAction<DiamondCollectible, int> OnDiamondPickedUp;
       
        #endregion

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                int playerIdx;  
                foreach (var player in GameManager.Instance.Players)
                {
                    if (player.gameObject.GetInstanceID().Equals(other.gameObject.GetInstanceID()))
                    {   
                       PlayerAddon.CheckCompatability(player.Addon, GameModes.Lasers);
                       LaserPlayerAddon playerAddon = (LaserPlayerAddon) player.Addon;
                       playerAddon.DiamondsCollected += 1; 
                       playerIdx = player.Index;
                       OnDiamondPickedUp.Invoke(this, playerIdx);
                       
                       AudioManager.PlayOneShot(_slurp);
                       return;
                    }
                }
            }
        }
    }
}
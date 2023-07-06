using System;
using System.Collections.Generic;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Lasers
{
    public class LaserBeam : MonoBehaviour
    {
        
        #region Non-Serialized Fields

        private Collider2D _laserCollider;

        private SpriteRenderer _spriteRenderer;
        public UnityAction<PlayerController, Vector2> OnLaserHit { set; get; }

        #endregion

        #region MonoBehaviour methods

        private void Start()
        {
            _laserCollider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // player has collided upon laser.
            if (other.CompareTag("Player"))
            {   
                foreach (var player in GameManager.Instance.Players)
                {
                    if (player.gameObject.GetInstanceID().Equals(other.gameObject.GetInstanceID()))
                    {   
                        if (((LaserPlayerAddon)player.Addon).InHit) return;
                        OnLaserHit.Invoke(player, Vector2.zero);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        public void ToggleLaser(bool shouldActivate)
        {
            _laserCollider.enabled = shouldActivate;
            _spriteRenderer.color = shouldActivate ? Color.white : new Color(1, 1, 1, 0);
            
        } 
        
        
        

        #endregion

    }
}

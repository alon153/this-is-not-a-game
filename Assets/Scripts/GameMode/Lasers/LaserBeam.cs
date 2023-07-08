using System;
using System.Collections.Generic;
using Basics.Player;
using FMODUnity;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Lasers
{
    public class LaserBeam : MonoBehaviour
    {
        
        #region Non-Serialized Fields

        private StudioEventEmitter _beamSound;
        private Collider2D _laserCollider;

        private SpriteRenderer _spriteRenderer;
        public UnityAction<PlayerController, Vector2> OnLaserHit { set; get; }

        #endregion

        #region MonoBehaviour methods

        private void Start()
        {
            _laserCollider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _beamSound = GetComponent<StudioEventEmitter>();
        }

        private void OnDestroy()
        {
            if(_beamSound.IsPlaying())
                _beamSound.Stop();
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
            if(shouldActivate)
                _beamSound.Play();
            else
                _beamSound.Stop();
            _laserCollider.enabled = shouldActivate;
            _spriteRenderer.color = shouldActivate ? Color.white : new Color(1, 1, 1, 0);
            
        } 
        
        
        

        #endregion

    }
}

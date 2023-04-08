﻿using System;
using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Basics.Player
{
    public partial class PlayerController
    {
        [field: SerializeField] public float FallTime { get; set; } = 2;
        [SerializeField] private float _fallDrag = 6;
        public bool IsBashed { get; set; } = false;

        #region Private Fields
        
        // this will only have a value when this player is knocked by another. 
        [CanBeNull] private PlayerController _playerKnockedBy;

        #endregion
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Arena"))
            {
                Fall();
            }
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {   
                // was the player bashing or only pushing the other player
                if (dashing)
                {   
                    // player has bashed another player so a knockback needed.
                    dashing = false;
                    bool isMutual = other.gameObject.GetComponent<PlayerController>().GetIsDashing();
                    KnockBackPlayer(other.gameObject, isMutual);
                }

                else
                {
                    other.gameObject.GetComponent<PlayerController>().SetPushingPlayer(this, false);
                }

            }
        }
        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var controller = other.gameObject.GetComponent<PlayerController>();

                if (!controller.IsBashed)
                    controller.SetPushingPlayer(null, false);
            }
        }

        #region Public Methods
        public void Fall()
        {
            foreach (var listener in _fallListeners)
            {
                listener.OnFall(this);    
            }
            
            
            var vel = Rigidbody.velocity;
            Freeze();
            Rigidbody.drag = _fallDrag;
            Rigidbody.AddForce(vel, ForceMode2D.Impulse);
            StartCoroutine(Fall_Inner());
        }

        #endregion
        
        #region Private Methods
        
        private IEnumerator Fall_Inner()
        {   
           
            float duration = 0f;
            float scaleFactor = 1f;
            var color = Renderer.color;
            var scale = transform.localScale;
            yield return null;
            
            while (duration < FallTime)
            {
                //fade out
                color.a = 1 - duration / FallTime;
                Renderer.color = color;
                
                //shrink
                scaleFactor = 1 - 0.5f * duration / FallTime;
                transform.localScale = scale * scaleFactor;

                duration += Time.deltaTime;
                yield return null;
            }

            color.a = 0f;
            Renderer.color = color;
            transform.localScale = scale;
            yield return null;

            Respawn();
        }
        
        
        
        /// <summary>
        /// called when Player is knocking another player.
        /// Note: this method is called by the player BASHING, not the player BASHED!
        /// </summary>
        /// <param name="player">
        /// the player who has been knocked.
        /// </param>
        /// <param name="mutualCollision">
        /// true if both players bashed each other, false otherwise. 
        /// </param>
        private void KnockBackPlayer(GameObject player, bool mutualCollision)
        {
            
            _onBeginKickBack?.Invoke();
            
            PlayerController otherPlayerController = player.GetComponent<PlayerController>();
            Rigidbody2D otherPlayerRb = otherPlayerController.Rigidbody;
            
            
            // calculate bash direction and force
            Vector2 knockDir = (player.transform.position - transform.position).normalized;
            float force = mutualCollision ? _mutualKnockBackForce : _knockBackForce;
            
            // set bashed player 
            otherPlayerController.SetMovementAbility(false);
            otherPlayerController.SetPushingPlayer(this, true);
            otherPlayerRb.AddForce(knockDir * force, ForceMode2D.Impulse);
            StartCoroutine(otherPlayerController.ResetMovementAfterKnockBack(otherPlayerRb));
        }

       

        #endregion

        #region Public Methods
        
        /// <summary>
        /// sets the _playerKnockedBy to the last pushing player. 
        /// </summary>
        /// <param name="playerKnocking">
        /// The player which pushed or bashed. 
        /// </param>
        /// <param name="wasBashed">
        /// was it actually bashed or only pushed by player?
        /// </param>
        public void SetPushingPlayer(PlayerController playerKnocking, bool wasBashed = false)
        {
            _playerKnockedBy = playerKnocking;
            IsBashed = wasBashed;
        }

        [CanBeNull]
        public PlayerController GetBashingPlayer()
        {
            return _playerKnockedBy;
        }

        /// <summary>
        /// coroutine activated after a player is knocked by something. it resets its velocity to zero after
        /// a given time.
        /// </summary>
        /// <param name="playerRb"></param>
        /// <param name="playerControl"></param>
        /// <returns></returns>
       public IEnumerator ResetMovementAfterKnockBack(Rigidbody2D playerRb)
        {   
            StopAllCoroutines();
            float time = 0f;
            Vector2 initialVelocity = playerRb.velocity;
            Vector2 noVelocity = Vector2.zero;
            while (time < _knockBackDelay)
            {
                time += Time.deltaTime;
                Vector2 curVelocity = Vector2.Lerp(initialVelocity, noVelocity, time / _knockBackDelay);
                playerRb.velocity = curVelocity;
                yield return null;
            }
            
            playerRb.velocity = Vector2.zero;
            SetMovementAbility(true);
            _playerKnockedBy = null;
            _onDoneKickBack?.Invoke(); 
        }

        #endregion
    }
}
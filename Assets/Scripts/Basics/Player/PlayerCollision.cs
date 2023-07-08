﻿using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using JetBrains.Annotations;
using Managers;
using UnityEngine;
using Utilities;

namespace Basics.Player
{
    public partial class PlayerController
    {
        [SerializeField] private ParticleSystem _collisionParticles;
        [field: SerializeField] public float FallTime { get; set; } = 2;
        [SerializeField] private float _fallDrag = 6;
        [SerializeField] private ParticleSystem _fallParticles;
        public bool IsBashed { get; set; } = false;

        private InteractableObject _interactable;
        private Action _afterFallAnimation;

        public InteractableObject Interactable
        {
            get => _interactable;
            set
            {
                if ((_interactable == null && value == null) ||
                    (_interactable != null && value != null)) return;
                _interactable = value;
                if (GameManager.Instance.Mode != _pressPrompt.Mode)
                {
                    _pressPrompt.SetPrompt(_interactable);
                    _pressPrompt.Mode = GameManager.Instance.Mode;
                }

                if (value == null)
                {
                    if(_longActionEmitter.IsPlaying())
                        _longActionEmitter.Stop();
                    _pressPrompt.Pressed = false;
                }
                    

                _pressPrompt.Toggle(_interactable != null);
            }
        }

        #region Private Fields
        
        // this will only have a value when this player is knocked by another. 
        [CanBeNull] public PlayerController _playerKnockedBy;
        
        private Coroutine _resetMoveCoroutine = null;
        private static readonly int Dead = Animator.StringToHash("Dead");

        #endregion

        #region Event Functions

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {   
                bool isOtherDashing = other.gameObject.GetComponent<PlayerController>().GetIsDashing();
                if (isOtherDashing)
                {
                    SetVibration(1, 0.2f);
                    PlayCollisionParticles(other.gameObject.transform.position);
                }

                // was the player bashing or only pushing the other player
                if (Dashing || _isInPostDash)
                {
                    AudioManager.PlayCollision();
                    
                    // player has bashed another player so a knockback needed.
                    bool isMutual = isOtherDashing;
                    GameManager.Instance.CameraScript.Shake(isMutual);
                    TimeManager.Instance.DelayInvoke((() => Dashing = false), 0.1f);
                    PlayerByPlayerKnockBack(other.gameObject, isMutual);
                    TimeManager.Instance.DelayInvoke((() =>
                    {
                        CancelDash();
                        Rigidbody.velocity = Vector2.zero;
                    }),0.05f);
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

        #endregion

        #region Public Methods

        public void Fall(bool shouldRespawn = true, bool stun = true)
        {
            var vel = Rigidbody.velocity;
            Rigidbody.drag = _fallDrag;
            Rigidbody.AddForce(vel, ForceMode2D.Impulse);
            Fall_Inner(shouldRespawn, stun);
            AudioManager.PlayFall();
            
            foreach (var listener in _fallListeners)
            {
                listener.OnFall(this);    
            }
           
        }

        #endregion
        
        #region Private Methods
        
        private void PlayCollisionParticles(Vector3 to)
        {
            var direction = ((Vector2) (transform.position - to)).normalized;
            float z = Vector2.Angle(Vector2.right, direction);
            _collisionParticles.transform.rotation = Quaternion.Euler(0,0,z);
            _collisionParticles.Play();
        }
        
        private void Fall_Inner(bool shouldRespawn, bool stun=true)
        {
            // Renderer.Animator.SetTrigger(Dead);
            Renderer.SetActive(false);
            _fallParticles.Play();
            Freeze();
            if (shouldRespawn)
                TimeManager.Instance.DelayInvoke((() => { Respawn(true); }), FallTime);
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
        private void PlayerByPlayerKnockBack(GameObject player, bool mutualCollision)
        {
            PlayerController otherPlayerController = player.GetComponent<PlayerController>();   
            Rigidbody2D otherPlayerRb = otherPlayerController.Rigidbody;

            foreach (var l in _pushedListeners)
            {
                l.OnPushed(otherPlayerController, this);
            }
            
            // calculate bash direction and force
            Vector2 knockDir = (player.transform.position - transform.position).normalized;
            float force = mutualCollision ? _mutualKnockBackForce : _knockBackForce;
            
            // set bashed player 
            otherPlayerController.SetMovementAbility(false);
            otherPlayerController.SetPushingPlayer(this, true);
            otherPlayerRb.AddForce(knockDir * force, ForceMode2D.Impulse);
            if (_resetMoveCoroutine != null)
                StopCoroutine(_resetMoveCoroutine); 
                
            _resetMoveCoroutine = StartCoroutine(otherPlayerController.ResetMovementAfterKnockBack(otherPlayerRb));
            Rigidbody.velocity = Vector2.zero;
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
            _resetMoveCoroutine = null;

        }

        public void AfterDeathAnimation()
        {
            // if(_afterFallAnimation == null) return;
            //
            // _afterFallAnimation.Invoke();
            // _afterFallAnimation = null;
        }

        public void PlayerByItemKnockBack(float? force, Vector2? dir)
        {
            // calculate bash direction and force
            var knockDir = dir ?? -Rigidbody.velocity;
            // set bashed player
            var curForce = force ?? _knockBackForce;
            Rigidbody.AddForce(knockDir * curForce, ForceMode2D.Impulse);
            if (_resetMoveCoroutine != null)
                StopCoroutine(_resetMoveCoroutine); 
                
            _resetMoveCoroutine = StartCoroutine(ResetMovementAfterKnockBack(Rigidbody));
            Rigidbody.velocity = Vector2.zero;
        }
        
        public void SetCollisionParticles(List<Sprite> newSprites)
        {   
            
            if (newSprites == null || newSprites.Count == Constants.Empty) return;
            
            var texSheet = _collisionParticles.textureSheetAnimation;
            
            if (texSheet.enabled == false) texSheet.enabled = true;

            for (int i = 0; i < newSprites.Count; i++)
            {
                texSheet.SetSprite(i ,newSprites[i]);
            }
        }
        #endregion
    }
}
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Basics.Player
{
    public partial class PlayerController
    {
        [field: SerializeField] public float FallTime { get; set; } = 2;
        [SerializeField] private float _fallDrag = 6;

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
            if (other.gameObject.CompareTag("Player") && _dashing)
            {
                _dashing = false;
                bool isMutual = other.gameObject.GetComponent<PlayerController>().GetIsDashing();
                Debug.Log("is mutual: " + isMutual);
                KnockBackPlayer(other.gameObject, isMutual);
            }
        }


        #region Public Metho
        public void Fall()
        {
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
        
        private IEnumerator ResetMovementAfterKnockBack(Rigidbody2D playerRb, PlayerController playerControl)
        {
            yield return new WaitForSeconds(_knockBackDelay);
            playerRb.velocity = Vector2.zero;
            playerControl.SetMovementAbility(true);
            _playerKnockedBy = null;
            _onDoneKickBack?.Invoke();
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
            StopAllCoroutines();
            _onBeginKickBack?.Invoke();
            
            Rigidbody2D otherPlayerRb = player.GetComponent<Rigidbody2D>();
            PlayerController otherPlayerController = player.GetComponent<PlayerController>();
            
            // calculate bash direction and force
            Vector2 knockDir = (player.transform.position - transform.position).normalized;
            float force = mutualCollision ? _mutualKnockBackForce : _knockBackForce;
            
            // set bashed player 
            otherPlayerController.SetMovementAbility(false);
            otherPlayerController.SetKnockingPlayer(this);
            otherPlayerRb.AddForce(knockDir * force, ForceMode2D.Impulse);
            StartCoroutine(otherPlayerController.ResetMovementAfterKnockBack(otherPlayerRb, 
                otherPlayerController));
        }
        
        #endregion

        #region Public Methods

        public void SetKnockingPlayer(PlayerController playerKnocking)
        {
            _playerKnockedBy = playerKnocking;
        }

        [CanBeNull]
        public PlayerController GetBashingPlayer()
        {
            return _playerKnockedBy;
        }

        #endregion
    }
}
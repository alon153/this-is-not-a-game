using System;
using UnityEngine;
using System.Collections;
using Managers;
using Object = System.Object;

namespace GameMode.Boats
{
    public class RiverObstacle : MonoBehaviour
    {
        #region Non Serialized fields

        private SpriteRenderer _spriteRenderer;

        private float _timeToFade = 1f;

        #endregion

        #region Constants
        
        private const int MaxAlpha = 1;

        private const int MinAlpha = 0;
            
        #endregion

        #region Properties
        public bool IsInMode { get; set; } = true;
        public Rigidbody2D ObstacleRigidbody2D { get; private set; }

        #endregion

        private void Start()
        {
            ObstacleRigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetDrag(float newDrag)
        {
            ObstacleRigidbody2D.drag = newDrag;
        }

        public void SetGravity(float newGravity)
        {
            ObstacleRigidbody2D.gravityScale = newGravity;
        }

        public void FreezeObstacle()
        {   
            if (isActiveAndEnabled)
                ObstacleRigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void DeactivateObstacleOnRound()
        {
            StartCoroutine(DeactivateObstacleInner());
        }

        public void DeactivateObstacleOnRoundEnd()
        {
            this.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Coroutine is called when an obstacle in the boats game mode has
        /// fallen from the game arena. the object is scaled and fades out and finally destroys.
        /// </summary>
        private IEnumerator DeactivateObstacleInner()
        {
            float timePassed = 0;
            var color = _spriteRenderer.color;
            var initScale = transform.localScale;
            
            while (timePassed < _timeToFade)
            {
                float progress = timePassed / _timeToFade;
                var alpha = Mathf.Lerp(MaxAlpha, MinAlpha, progress);
                var newColor = new Color(color.r, color.g, color.b, alpha);
                var curScale = Vector3.Lerp(initScale, Vector3.zero, progress);

                _spriteRenderer.color = newColor;
                transform.localScale = curScale;
                
                timePassed += Time.deltaTime;
                yield return null;
            }
            
            gameObject.SetActive(false);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Arena") && IsInMode)
                BoatsInRiverMode.ObstaclesPool.Release(this);
        }
    }
    
}

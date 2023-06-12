using System;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using Utilities;

namespace GameMode.Boats
{
    public class RiverObstacle : MonoBehaviour
    {
        #region Non Serialized fields

        private SpriteRenderer _spriteRenderer;

        private float _timeToFade = 1f;

        private bool _inDeactivation;

        private bool _inRiver;

        private Color _obsColor;

        [CanBeNull] public Coroutine _fadeCoroutine = null;

        public float speed = 0.01f;

        #endregion

        #region Properties
        
        /// <summary>
        /// Flag used to control whether obstacle should do on triggerExit content or not.
        /// it will be switched to false when   
        /// </summary>
        public bool IsInMode { get; set; } = true;
        public Rigidbody2D ObstacleRigidbody2D { get; private set; }

        #endregion

        private void Start()
        {
            ObstacleRigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _obsColor = _spriteRenderer.color;
        }

        public void FreezeObstacle()
        {   
            if (gameObject.activeSelf)
                ObstacleRigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void DeactivateObstacleOnRound()
        {
            if (gameObject.activeSelf && !_inDeactivation)
                _fadeCoroutine = StartCoroutine(DeactivateObstacleInner());
        }

        public void StopFadeCoroutine()
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
            
            transform.localScale = Vector3.one;
            _spriteRenderer.color = _obsColor;

        }
        
        /// <summary>
        /// Coroutine is called when an obstacle in the boats game mode has
        /// fallen from the game arena. the object is scaled and fades out and finally destroys.
        /// </summary>
        private IEnumerator DeactivateObstacleInner()
        {
            _inDeactivation = true;
            float timePassed = 0;
            var color = _spriteRenderer.color;
            var initScale = transform.localScale;
            
            while (timePassed < _timeToFade)
            {
                float progress = timePassed / _timeToFade;
                var alpha = Mathf.Lerp(Constants.MaxProgress, Constants.MinProgress, progress);
                var newColor = new Color(color.r, color.g, color.b, alpha);
                var curScale = Vector3.Lerp(initScale, Vector3.zero, progress);

                _spriteRenderer.color = newColor;
                transform.localScale = curScale;
                
                timePassed += Time.deltaTime;
                yield return null;
            }
            _inDeactivation = false;
            _inRiver = false;
            gameObject.SetActive(false);
           
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Arena") && IsInMode)
            {
                _inRiver = true;
                BoatsInRiverMode.ObstaclesPool.Release(this);
            }
        }

        private void Update()
        {
            var position = transform.position;
            float newY = position.y + speed;
            Vector3 newPosition = new Vector3(position.x, newY, position.z);
            position = newPosition;
            transform.position = position;
        }
    
    }
    
}

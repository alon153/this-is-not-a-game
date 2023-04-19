using System;
using UnityEngine;
using System.Collections;
using Managers;

namespace GameMode.Boats
{
    public class RiverObstacle : MonoBehaviour
    {
        #region Non Serialized fields
        public Rigidbody2D ObstacleRigidbody2D { get; private set; }
        
        private float _timeToFade = 1f;

        #endregion

        #region Constants
        
        private const int MaxAlpha = 1;

        private const int MinAlpha = 0;
            
        #endregion

        #region Properties

        public bool IsInMode { get; set; } = true;

        #endregion

        private void Start()
        {
            ObstacleRigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void SetDrag(float newDrag)
        {
            ObstacleRigidbody2D.drag = newDrag;
        }

        public void SetGravity(float newGravity)
        {
            ObstacleRigidbody2D.gravityScale = newGravity;
        }
        
        /// <summary>
        /// Coroutine is called when an obstacle in the boats game mode has
        /// fallen from the game arena. the object is scaled and fades out and finally destroys.
        /// </summary>
        private IEnumerator FadeAndScaleOnFall(SpriteRenderer objRend, Transform objTrans)
        {
            float timePassed = 0;
            var color = objRend.color;
            var initScale = objTrans.localScale;
            
            while (timePassed < _timeToFade)
            {
                float progress = timePassed / _timeToFade;
                var alpha = Mathf.Lerp(MaxAlpha, MinAlpha, progress);
                var newColor = new Color(color.r, color.g, color.b, alpha);
                var curScale = Vector3.Lerp(initScale, Vector3.zero, progress);

                objRend.color = newColor;
                objTrans.localScale = curScale;
                
                timePassed += Time.deltaTime;
                yield return null;
            }
            
            Destroy(this.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Arena") && IsInMode)
            {
                StartCoroutine(FadeAndScaleOnFall(GetComponent<SpriteRenderer>(),
                    GetComponent<Transform>()));
            }
        }
    }
    
}

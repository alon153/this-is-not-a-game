using System;
using UnityEngine;
using System.Collections;

namespace GameMode.Boats
{
    public class RiverObstacle : MonoBehaviour
    {
        #region Non Serialized fields

        private Rigidbody2D _obstacleRigidbody2D;
        
        private float _timeToFade = 1f;

        #endregion

        #region Constans
        
        private const int MaxAlpha = 1;

        private const int MinAlpha = 0;
            
        #endregion

        private void Start()
        {
            _obstacleRigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void SetDrag(float newDrag)
        {
            _obstacleRigidbody2D.drag = newDrag;
        }

        public void SetGravity(float newGravity)
        {
            _obstacleRigidbody2D.gravityScale = newGravity;
        }

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
            if (other.CompareTag("Arena"))
            {
                StartCoroutine(FadeAndScaleOnFall(GetComponent<SpriteRenderer>(),
                    GetComponent<Transform>()));
            }
        }
    }
    
}

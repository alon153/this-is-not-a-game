using System.Collections;
using UnityEngine;

namespace Basics.Player
{
    public partial class PlayerController
    {
        [field: SerializeField] public float FallTime { get; set; } = 2;
        [SerializeField] private float _fallDrag = 6;
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Arena"))
            {
                Fall();
            }
        }

        #region Public Methods

        private void Fall()
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
            var color = _renderer.color;
            var scale = transform.localScale;
            yield return null;
            
            while (duration < FallTime)
            {
                //fade out
                color.a = 1 - duration / FallTime;
                _renderer.color = color;
                
                //shrink
                scaleFactor = 1 - 0.5f * duration / FallTime;
                transform.localScale = scale * scaleFactor;

                duration += Time.deltaTime;
                yield return null;
            }

            color.a = 0f;
            _renderer.color = color;
            transform.localScale = scale;
            yield return null;

            Respawn();
        }
        
        #endregion
    }
}
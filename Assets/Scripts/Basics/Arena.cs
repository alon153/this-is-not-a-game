using System;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Interfaces;
using Random = UnityEngine.Random;

namespace Basics
{   
    [Serializable]
    public class Arena : MonoBehaviour
    {
        [SerializeField] private LayerMask _respawnBlockers;
        [SerializeField] private Transform _perimeter;
        
        public UnityAction<int> OnPlayerDisqualified;
        public SpriteRenderer Renderer { get; private set; }
        public Vector3 Dimensions => new Vector3(_perimeter.lossyScale.x, _perimeter.lossyScale.y, 0);
        public Vector3 BottomLeft => _perimeter.position - Dimensions / 2;
        public Vector3 TopRight => _perimeter.position + Dimensions / 2;
        public Vector3 TopLeft => BottomLeft + new Vector3(0,Dimensions.y,0);
        public Vector3 BottomRight => TopRight - new Vector3(0,Dimensions.y,0);
        public Vector3 TopMiddle => (TopLeft + TopRight) / 2;
        public Vector3 BottomMiddle => (BottomLeft + BottomRight) / 2;
        public Vector3 RightMiddle => (TopRight + BottomRight) / 2;
        public Vector3 LeftMiddle => (TopLeft + BottomLeft) / 2;
        public Vector3 Center => _perimeter.position;

        private void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            if (_perimeter == null)
                _perimeter = transform;
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (GameManager.Instance.CurrArena.GetInstanceID() == this.GetInstanceID())
            {
                IFallable fallable = other.gameObject.GetComponent<IFallable>();
                if (fallable == null)
                    return;

                fallable.Fall();
            }
        }

        public Vector3 GetRespawnPosition(GameObject obj)
        {
            var pos = _perimeter.position;
            var scale = _perimeter.lossyScale;
            var objScale = obj.transform.lossyScale;
            var margin = new Vector2(scale.x - objScale.x, scale.y - objScale.y) / 2;
            
            Vector3 respawnPos = Vector3.zero;
            bool found = false;
            while (!found)
            {
                float x = Random.Range(pos.x - margin.x, pos.x + margin.x);
                float y = Random.Range(pos.y - margin.y, pos.y + margin.y);

                respawnPos.x = x;
                respawnPos.y = y;
                Collider2D collision = Physics2D.OverlapCircle(respawnPos, objScale.x / 2, _respawnBlockers);
                found = collision == null || collision.gameObject == null;
            }

            return respawnPos;
        }

        public bool OutOfArena(Vector3 pos) => pos.x < BottomLeft.x 
                                               || pos.x > TopRight.x 
                                               || pos.y < BottomLeft.y 
                                               || pos.y > TopRight.y;
    }
}
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Basics
{
    public class Arena : MonoBehaviour
    {
        [SerializeField] private LayerMask _respawnBlockers;
        [SerializeField] private SpriteMask _spriteMask;

        public SpriteRenderer Renderer { get; private set; }

        private void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            _spriteMask = GetComponent<SpriteMask>();
            _spriteMask.sprite = Renderer.sprite;
        }

        public Vector3 GetRespawnPosition(GameObject obj)
        {
            var pos = transform.position;
            var scale = transform.lossyScale;
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
    }
}
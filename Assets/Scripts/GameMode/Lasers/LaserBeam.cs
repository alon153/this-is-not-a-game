using System;
using System.Collections.Generic;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Lasers
{
    public class LaserBeam : MonoBehaviour
    {
        #region Serailized Fields

        [SerializeField] private List<bool> laserCycles = new List<bool>();

        [SerializeField] private float laserToggleTime = 3;
        
        #endregion
        
        #region Non-Serialized Fields

        private Collider2D _laserCollider;

        private SpriteRenderer _spriteRenderer;

        private float _timer = 0f;

        private int _cycleIdx = 0;

        public UnityAction<PlayerController> OnLaserHit { set; get; }
        
        #endregion
        
        #region Constants

        private const int ResetCycle = 0;
        
        #endregion
        
        #region MonoBehaviour methods

        private void Start()
        {
            _laserCollider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= laserToggleTime)
            {
                _timer = 0f;
                if (laserCycles[_cycleIdx])
                    ToggleLaser();                     
                
                _cycleIdx++;
                if (_cycleIdx == laserCycles.Count)
                    _cycleIdx = ResetCycle;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {   
            // player has collided upon laser.
            if (other.CompareTag("Player"))
            {
                foreach (var player in GameManager.Instance.Players)
                {
                    if (player.gameObject.GetInstanceID().Equals(other.gameObject.GetInstanceID()))
                    {
                        OnLaserHit.Invoke(player);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private void ToggleLaser()
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            _laserCollider.enabled = !_laserCollider.enabled;
        }
        

        #endregion

    }
}

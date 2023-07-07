using System;
using GameMode.Lasers;
using UnityEngine;
using System.Collections.Generic;
using Basics.Player;
using Managers;
using Unity.Mathematics;
using UnityEngine.Events;
using Utilities;
using Random = System.Random;

namespace GameMode.Lasers
{
    public class Window : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private float laserToggleTime = 3;
        
        [Range(0, 1)] 
        [SerializeField] private float maxGlow = 1f; 
        
        [SerializeField] private Material windowGlow;

        [SerializeField] private bool isSideWindow = false;

        [SerializeField] private int defaultVelocityMultiplier = 7;

        #endregion
        
        #region Private Fields
        
        private LaserBeam _laserBeam;

        private SpriteRenderer _windowRenderer;

        private bool _shouldCastBeam;
        
        private float _timer = 0f;

        private bool _inMaxGlow = false;

        private readonly Random _randomBool = new Random();
        
        private readonly int _colorFactor = Shader.PropertyToID("_ColorFactor");

        #endregion

        private void Awake()
        {
            _laserBeam = GetComponentInChildren<LaserBeam>();
            _windowRenderer = GetComponent<SpriteRenderer>();
            _windowRenderer.material = new Material(windowGlow);

        }


        // Update is called once per frame
        void Update()
        {   
            if (GameManager.Instance.State != GameState.Playing) return;
            
            _timer += Time.deltaTime;
            if (_timer >= laserToggleTime)
            {
                _timer = 0f;
                OnCurCycleEnd(_shouldCastBeam);
                _shouldCastBeam = _randomBool.Next(2) == 1;
            }

            else if (!_inMaxGlow && _shouldCastBeam)
            {   
                
                var glow = Mathf.Lerp(Constants.MinProgress, maxGlow, _timer / laserToggleTime);
                _windowRenderer.material.SetFloat(_colorFactor, glow);
            }
        }

        private Vector2 GetDefaultVelocity(PlayerController playerController)
        {
            Vector3 playerPosition = playerController.transform.position;
            Vector3 windowPosition = transform.position;
            
            if (isSideWindow)
                return playerPosition.y > windowPosition.y ? Vector2.up : Vector2.down;
            
            return playerPosition.x > windowPosition.x ? Vector2.right : Vector2.left;
        }

        private void OnCurCycleEnd(bool activate)
        {
            _laserBeam.ToggleLaser(activate);
            _inMaxGlow = activate;
            _windowRenderer.material.SetFloat(_colorFactor, _inMaxGlow ? maxGlow : Constants.MinProgress);
            if (activate)
            {
                var rayDir = isSideWindow ? Vector2.left : Vector2.down;
                RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, rayDir, Mathf.Infinity, LayerMask.GetMask("Player"));
                foreach (var player in hit)
                {
                    var controller = player.collider.GetComponent<PlayerController>();
                    var velocity = defaultVelocityMultiplier * GetDefaultVelocity(controller);
                    _laserBeam.OnLaserHit(controller, velocity);
                }
            }
        }

        public void SetOnLaserHit(UnityAction<PlayerController, Vector2> action) => _laserBeam.OnLaserHit += action;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var rayDir = isSideWindow ? Vector2.left : Vector2.down;
            Gizmos.DrawRay(transform.position, rayDir);
        }
    }
}

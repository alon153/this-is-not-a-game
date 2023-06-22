using System;
using GameMode.Lasers;
using UnityEngine;
using System.Collections.Generic;
using Basics.Player;
using Managers;
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

        private void OnCurCycleEnd(bool activate)
        {
            _laserBeam.ToggleLaser(activate);
            _inMaxGlow = activate;
            _windowRenderer.material.SetFloat(_colorFactor, _inMaxGlow ? maxGlow : Constants.MinProgress);
        }

        public void SetOnLaserHit(UnityAction<PlayerController> action) => _laserBeam.OnLaserHit += action;

    }
}

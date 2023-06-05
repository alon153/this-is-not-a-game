using System;
using GameMode.Lasers;
using UnityEngine;
using System.Collections.Generic;
using Basics.Player;
using UnityEngine.Events;

namespace GameMode.Lasers
{
    public class Window : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private List<bool> laserCycles = new List<bool>();

        [SerializeField] private float laserToggleTime = 3;

        [SerializeField] private Material windowGlow;
        
        #endregion
        
        #region Private Fields
        
        private LaserBeam _laserBeam;

        private SpriteRenderer _windowRenderer;
        
        private float _timer = 0f;

        private int _cycleIdx = 0;

        private bool _inMaxGlow = false;
        
        private static readonly int ColorFactor = Shader.PropertyToID("_ColorFactor");

        #endregion
        
        #region Constants
        
        private const int ResetCycle = 0;

        private const float MinGlow = 0.1f;

        private const float MaxGlow = 1f;
        
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
            _timer += Time.deltaTime;
            if (_timer >= laserToggleTime)
            {
                _timer = 0f;
                OnCurCycleEnd(laserCycles[_cycleIdx]);
                
                _cycleIdx++;
                if (_cycleIdx == laserCycles.Count)
                    _cycleIdx = ResetCycle;
            }

            else if (!_inMaxGlow && laserCycles[_cycleIdx])
            {   
                
                var glow = Mathf.Lerp(MinGlow, MaxGlow, _timer / laserToggleTime);
                _windowRenderer.material.SetFloat(ColorFactor, glow);
            }
        }

        private void OnCurCycleEnd(bool activate)
        {
            _laserBeam.ToggleLaser(activate);
            _inMaxGlow = activate;
            _windowRenderer.material.SetFloat(ColorFactor, _inMaxGlow ? MaxGlow : MinGlow);
        }

        public void SetOnLaserHit(UnityAction<PlayerController> action) => _laserBeam.OnLaserHit += action;

    }
}

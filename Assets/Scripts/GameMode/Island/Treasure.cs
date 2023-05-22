using System;
using Basics;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Pool;
using Utilities.Interfaces;

namespace GameMode.Island
{
    public class Treasure : InteractableObject
    {
        #region Non-Serialized Fields

        private float _diggingTimeLeft;
        private PlayerController _digger;
        private float _diggingTime;
        private SpriteRenderer _renderer;

        #endregion
        
        #region Properties

        public ObjectPool<Treasure> Pool { get; set; } = null;

        public float DiggingTime
        {
            get => _diggingTime;
            set
            {
                _diggingTime = value;
                _diggingTimeLeft = value;
            }
        }
        
        public float Score { get; set; }

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            _diggingTimeLeft = _diggingTime;
            _renderer = GetComponent<SpriteRenderer>();

            var color = _renderer.color;
            color.a = 0;
            _renderer.color = color;
            
            IsHold = true; // Make the interaction with this object a hold interaction and not a press interaction
        }

        protected override void Update()
        {
            base.Update();
            if (_digger != null)
            {
                _diggingTimeLeft -= Time.deltaTime;

                UpdateProgress();
                
                if (_diggingTimeLeft <= 0)
                    Dig();
            }
        }

        #endregion

        #region Public Methods

        public void Release()
        {
            if (_digger != null && _digger.Interactable.gameObject.GetInstanceID() == gameObject.GetInstanceID())
            {
                _digger.Interactable = null;
                _digger.Gamepad.SetMotorSpeeds(0,0);
                _digger.UnFreeze();
                _digger = null;
            }
            CanInteract = false;
            gameObject.SetActive(false);
        }

        #endregion

        #region Private Methods
        
        private void UpdateProgress()
        {
            var color = _renderer.color;
            color.a = 1 - (_diggingTimeLeft / _diggingTime);
            _renderer.color = color;
        }

        private void Dig()
        {
            if(_digger == null)
                return;
            
            PlayerAddon.CheckCompatability(_digger.Addon, GameModes.Island);
            float score = Mathf.Max(Score, 0);
            ((IslandPlayerAddon) _digger.Addon).Score += score; 
            ScoreManager.Instance.SetPlayerScore(_digger.Index, score);
            
            _digger = null;
            if (Pool != null)
            {
                Pool.Release(this);
            }
            else
            {
                Release();
                Destroy(gameObject);
            }
        }

        #endregion

        #region InteractableObject

        protected override void OnStopInteract_Inner(PlayerController player)
        {
            if(_digger == null || player.GetInstanceID() != _digger.GetInstanceID())
                return;
            
            _digger.UnFreeze();
            _digger = null;
        }

        protected override void TogglePrompt_Inner(bool showPrompt) { }

        protected override void OnInteract_Inner(PlayerController player)
        {
            if(_digger != null)
                return;

            _digger = player;
            _digger.Freeze(false);
        }

        protected override bool CanPlayerInteract(PlayerController player)
        {
            return _digger == null || player.GetInstanceID() == _digger.GetInstanceID();
        }

        #endregion
    }
}
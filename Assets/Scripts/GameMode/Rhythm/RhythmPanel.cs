using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Basics;
using Basics.Player;
using Managers;
using UnityEngine;

namespace GameMode.Rhythm
{
    public class RhythmPanel : InteractableObject, IOnBeatListener
    {
        #region Serialized Fields

        [SerializeField] private List<int> _beats;
        [SerializeField] private RhythmRing _ringPrefab;
        [SerializeField] private float _coyoteTime = 0.5f;

        #endregion

        #region Non-Serialized Fields
        
        private Dictionary<int, RhythmRing> _rings = new();
        private RingTrigger _ringTrigger;
        
        private Guid _beatInvoke = Guid.Empty;

        #endregion

        #region Properties

        #endregion

        #region Function Events

        private void Awake()
        {
            InitRings();
            _ringTrigger = GetComponentInChildren<RingTrigger>();
        }

        private void OnDestroy()
        {
            if (_beatInvoke != Guid.Empty)
                TimeManager.Instance.CancelInvoke(_beatInvoke);
        }

        private void InitRings()
        {
            foreach (var beat in _beats)
            {
                if (!_rings.ContainsKey(beat))
                {
                    _rings[beat] = Instantiate(_ringPrefab, transform);
                    _rings[beat].transform.localScale = Vector3.zero;
                }
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region IOnBeatListener

        public void OnBeat(int beat)
        {
            if(GameManager.Instance.CurrentState != GameState.Playing)
                return;
            
            if (_beatInvoke != Guid.Empty)
                TimeManager.Instance.CancelInvoke(_beatInvoke);
            _beatInvoke = TimeManager.Instance.DelayInvoke((() =>
            {
                if (_rings.ContainsKey(beat))
                {
                    var ring = _rings[beat];
                    ring.StartRing();
                }
                _beatInvoke = Guid.Empty;
            }), _coyoteTime);
        }

        #endregion

        #region InteractableObject

        protected override void TogglePrompt_Inner(bool showPrompt)
        {
        
        }

        protected override void OnInteract_Inner(PlayerController player)
        {
            bool onBeat = _ringTrigger.Beat();
            ScoreManager.Instance.SetPlayerScore(player.Index, onBeat ? 10 : -5);
        }

        protected override bool CanPlayerInteract(PlayerController player)
        {
            return true;
        }

        #endregion

        public void StopRings()
        {
            foreach (var key in _rings.Keys)
            {
                _rings[key].ResetRing();
            }
        }
    }
}
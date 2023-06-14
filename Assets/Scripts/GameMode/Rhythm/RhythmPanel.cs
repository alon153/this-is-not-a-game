using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Basics;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace GameMode.Rhythm
{
    public class RhythmPanel : InteractableObject, IOnBeatListener
    {
        #region Serialized Fields
        
        [SerializeField] private RhythmRing _ringPrefab;
        [SerializeField] private RingTrigger _ringTrigger;

        #endregion

        #region Non-Serialized Fields

        private LinkedPool<RhythmRing> _rings;

        private Guid _beatInvoke = Guid.Empty;

        #endregion

        #region Properties

        #endregion

        #region Function Events

        private void Awake()
        {
            InitRings();
        }

        private void OnDestroy()
        {
            if (_beatInvoke != Guid.Empty)
                TimeManager.Instance.CancelInvoke(_beatInvoke);
        }

        private void InitRings()
        {
            _rings = new LinkedPool<RhythmRing>(
                createFunc: (() =>
                {
                    var ring = Instantiate(_ringPrefab, transform);
                    ring.transform.localScale = Vector3.zero;
                    ring.Pool = _rings;
                    return ring;
                }),
                actionOnGet:(ring =>
                {
                    ring.StartRing();
                }),
                actionOnRelease:(ring => ring.ResetRing()));
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region IOnBeatListener

        public void OnBeat(int beat)
        {
            if(GameManager.Instance.State != GameState.Playing)
                return;
            
            _rings.Get();
        }

        #endregion

        #region InteractableObject

        protected override void TogglePrompt_Inner(bool showPrompt)
        {
        
        }

        protected override void OnInteract_Inner(PlayerController player)
        {
            bool onBeat = _ringTrigger != null && _ringTrigger.Beat();
            ScoreManager.Instance.SetPlayerScore(player.Index, onBeat ? 10 : -5);
        }

        protected override bool CanPlayerInteract(PlayerController player)
        {
            return true;
        }

        #endregion

        public void StopRings()
        {
            _rings.Clear();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Basics;
using Basics.Player;
using FMODUnity;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace GameMode.Rhythm
{
    public class RhythmPanel : InteractableObject, IOnBeatListener
    {
        #region Serialized Fields

        [SerializeField] private EventReference _goodSound;
        [SerializeField] private EventReference _badSound;
        
        [SerializeField] private RhythmRing _ringPrefab;
        [SerializeField] private RingTrigger _ringTrigger;
        [SerializeField] private SpriteRenderer _upperSprite;

        [SerializeField] private Sprite _greenGlow;
        [SerializeField] private Sprite _redGlow;

        [SerializeField] private float _glowFadeOut = 0.2f;

        #endregion

        #region Non-Serialized Fields

        private float _lastPress = 0;
        private float _pressThreshold = 0.5f;

        private LinkedPool<RhythmRing> _rings;

        private Guid _beatInvoke = Guid.Empty;

        private Coroutine _beatCoroutine = null;

        private bool _flippedX;

        #endregion

        #region Properties

        #endregion

        #region Function Events

        private void Awake()
        {
            _flippedX = GetComponent<SpriteRenderer>().flipX;
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
                    ring.GetComponent<SpriteRenderer>().flipX = _flippedX;
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
            var ring = _rings.Get();
            
        }

        #endregion

        #region InteractableObject

        protected override void TogglePrompt_Inner(bool showPrompt)
        {
        
        }

        protected override void OnInteract_Inner(PlayerController player)
        {
            bool onBeat = _ringTrigger != null && _ringTrigger.Beat();
            
            if (!onBeat && Time.time - _lastPress < _pressThreshold)
                return;
            
            _lastPress = Time.time;
            
            if(_beatCoroutine != null)
                StopCoroutine(_beatCoroutine);
            AudioManager.PlayOneShot(onBeat ? _goodSound : _badSound);
            _beatCoroutine = StartCoroutine(UpperGlow_Inner(onBeat));
            
            ScoreManager.Instance.SetPlayerScore(player.Index, onBeat ? 10 : -5);
        }

        private IEnumerator UpperGlow_Inner(bool green)
        {
            _upperSprite.sprite = green ? _greenGlow : _redGlow;

            var color = Color.white;
            color.a = 1;
            _upperSprite.color = color;

            yield return null;

            float duration = 0;
            while (duration <= _glowFadeOut)
            {
                duration += Time.deltaTime;
                color.a = (_glowFadeOut - duration) / _glowFadeOut;
                _upperSprite.color = color;
                yield return null;
            }

            color.a = 0;
            _upperSprite.color = color;
            _beatCoroutine = null;
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
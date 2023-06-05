using System;
using System.Collections;
using GameMode;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class TransitionWindow : MonoBehaviour
    {
        #region Serialized Fields
        
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private RectTransform _timer;
        [SerializeField] private Image _instructions;
        [SerializeField] private float _transitionTime;
        
        #endregion
        
        #region Non-Serialized Fields

        private RectTransform _rect;
        private float _timerOrigX;
        private float _rectOrigY;
        private float _destY;

        private Coroutine _transitionCoroutine;

        #endregion

        #region Properties

        private float Speed => _transitionTime == 0 ? Mathf.Infinity : _rectOrigY / _transitionTime;

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _timerOrigX = _timer.rect.width;
            _rectOrigY = _rect.rect.height;
        }

        private void Update()
        {
            if (_rect.anchoredPosition.y != _destY)
            {
                MoveWindow();
            }
        }

        #endregion

        #region Public Method

        public void ShowWindow(string title, Sprite instructions, bool immediate=false, Action onEnd=null)
        {
            if (immediate)
            {
                _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, _rectOrigY);
                return;
            }
            
            if(_rect.anchoredPosition.y >= _rectOrigY) return;
            
            _destY = _rectOrigY;
            StartTimer(GameManager.Instance.InstructionsTime, onEnd);
        }

        public void HideWindow(bool immediate=false)
        {
            if (immediate)
            {
                _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, 0);
                return;
            }
            
            if(_rect.anchoredPosition.y <= 0) return;
            
            _destY = 0;
        }

        #endregion

        #region Private Method

        private void MoveWindow()
        {
            var destPos = _rect.anchoredPosition;
            destPos.y = Speed == Mathf.Infinity ? _destY : destPos.y + Time.deltaTime * Speed * Mathf.Sign(_destY - destPos.y);
            
            if (destPos.y < 0 || destPos.y > _rectOrigY) destPos.y = _destY;

            _rect.anchoredPosition = destPos;
        }

        private void SetInformation(string title, Sprite instructions)
        {
            _title.text = title;
            _instructions.sprite = instructions;
        }

        private void StartTimer(float duration, Action onEnd=null)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(Timer_Inner(duration, onEnd));
        }

        private IEnumerator Timer_Inner(float duration, Action onEnd=null)
        {
            var pos = _timer.anchoredPosition;
            pos.x = _timerOrigX;
            _timer.anchoredPosition = pos;
            float endTime = Time.time + duration;

            yield return null;

            float t = 1;
            while (t > 0)
            {
                t = (endTime - Time.time) / duration;
                pos.x = _timerOrigX * t;
                _timer.anchoredPosition = pos;
                yield return null;
            }
            
            pos.x = 0;
            _timer.anchoredPosition = pos;

            _transitionCoroutine = null;

            onEnd?.Invoke();
        }

        #endregion
    }
}
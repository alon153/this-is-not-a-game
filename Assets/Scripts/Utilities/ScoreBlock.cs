using TMPro;
using UnityEngine;

namespace Utilities
{
    public class ScoreBlock : MonoBehaviour
    {
        #region Serialized Fields
        
        [SerializeField] private TextMeshProUGUI _scoreTxt;
        [SerializeField] private float _transitionTime = 3;
        [SerializeField] private float _downHeight = 45;
        [SerializeField] private float _upHeight = 90;
        
        #endregion
        
        #region Non-Serialized Fields

        private RectTransform _rect;
        private float _destY;
        private float _score = 0;

        private Coroutine _transitionCoroutine;

        #endregion

        #region Properties

        private float Speed => _transitionTime == 0 ? Mathf.Infinity : (_upHeight - _downHeight) / _transitionTime;

        public float Score
        {
            get => _score;
            set
            {
                _score = value;
                _scoreTxt.text = $"{(int) _score}";
            }
        }

        public float AnchorMinX
        {
            get => _rect.anchorMin.x;
            set => _rect.anchorMin = new Vector2(value, _rect.anchorMin.y);
        }
        
        public float AnchorMaxX
        {
            get => _rect.anchorMax.x;
            set => _rect.anchorMax = new Vector2(value, _rect.anchorMax.y);
        }

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            MoveTo(0,true);
            Score = 0;
        }

        private void Update()
        {
            if (_rect.anchoredPosition.y != _destY)
                MoveWindow();
        }

        #endregion

        #region Public Method

        public void MoveTo(float destY, bool immediate=false)
        {
            if (immediate || Mathf.Abs(_rect.anchoredPosition.y - destY) < 0.5f)
            {
                _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, destY);
            }

            _destY = destY;
        }

        public void Raise()
        {
            MoveTo(_upHeight);
        }
        
        public void Lower()
        {
            MoveTo(_downHeight);
        }

        #endregion

        #region Private Method

        private void MoveWindow()
        {
            var destPos = _rect.anchoredPosition;
            destPos.y = Speed == Mathf.Infinity ? _destY : destPos.y + Time.deltaTime * Speed * Mathf.Sign(_destY - destPos.y);
            
            if (destPos.y >= _upHeight || (_destY <= _downHeight && destPos.y <= _destY)) destPos.y = _destY;

            _rect.anchoredPosition = destPos;
        }

        #endregion
    }
}


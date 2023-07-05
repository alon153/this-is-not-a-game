using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class ChannelNameSlide : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private float _transitionTime = 3;
    [SerializeField] private Image _channel;

    #endregion

    #region Non-Serialized Fields

    private RectTransform _rect;
    private float _rectOrigY;
    private float _destY;

    private Coroutine _slideCoroutine;

    #endregion

    #region Properties

    private float Speed => _transitionTime == 0 ? Mathf.Infinity : _rectOrigY / _transitionTime;
    
    #endregion

    #region Function Events

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
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

    public void ShowWindow(Sprite channel=null, bool immediate = false)
    {
        SetInformation(channel);
        
        if (immediate)
        {
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, -_rectOrigY);
            return;
        }

        if (_rect.anchoredPosition.y <= -_rectOrigY) return;

        _destY = -_rectOrigY;
    }

    public void HideWindow(bool immediate = false)
    {
        if (immediate)
        {
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, 0);
            return;
        }

        if (_rect.anchoredPosition.y >= 0) return;

        _destY = 0;
    }

    #endregion

    #region Private Method
    
    private void SetInformation(Sprite instructions)
    {
        if (instructions != null)
            _channel.sprite = instructions;
    }

    private void MoveWindow()
    {
        var destPos = _rect.anchoredPosition;
        destPos.y = Speed == Mathf.Infinity
            ? _destY
            : destPos.y + Time.unscaledDeltaTime * Speed * Mathf.Sign(_destY - destPos.y);

        if (destPos.y > 0 || destPos.y < -_rectOrigY) destPos.y = _destY;

        _rect.anchoredPosition = destPos;
    }

    #endregion

    #region Private Methods

    #endregion
}
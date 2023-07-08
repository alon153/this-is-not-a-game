using System;
using Audio;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class RhythmRing : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Color _onBeatColor = Color.green;
    [SerializeField] private float _startingAlpha = 0.5f;
    [SerializeField] private float _timeToFullAlpha = 2;

    #endregion

    #region Non-Serialized Fields

    private Vector3 _originalScale;
    private float _time;
    private float _sizePerSecond;
    private RingTrigger _ringTrigger;

    private SpriteRenderer _renderer;
    private Guid _removeRing = Guid.Empty;

    private Color _origColor;
    private Color _currColor;

    #endregion

    #region Properties

    public bool Run { get; private set; } = false;
    public LinkedPool<RhythmRing> Pool { get; set; } = null;

    #endregion

    #region Function Events

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalScale = Vector3.zero;
        
        _origColor = _renderer.color;
        _currColor = _origColor;
        _currColor.a = _startingAlpha;
        _renderer.color = _currColor;
    }

    private void Start()
    {
        _time = 4 * (AudioManager.Tempo / 60);
        _sizePerSecond = 1 / _time;
    }

    private void Update()
    {
        if (Run)
        {
            transform.localScale += new Vector3(_sizePerSecond, _sizePerSecond, 0) * Time.deltaTime;
            if (_currColor.a < 1)
            {
                _currColor.a += Mathf.Min(Mathf.Pow((1 - _startingAlpha) / _timeToFullAlpha,2), 1) * Time.deltaTime;
                _renderer.color = _currColor;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("RingTrigger"))
        {
            if (_ringTrigger != null)
                _ringTrigger.UnregisterRing(this);

            _ringTrigger = other.GetComponent<RingTrigger>();
            _ringTrigger.RegisterRing(this);
            _currColor = _onBeatColor;
            _renderer.color = _onBeatColor;
        }
        else if (other.CompareTag("RingPanel"))
        {
            if (_removeRing != Guid.Empty)
                TimeManager.Instance.CancelInvoke(_removeRing);
            _removeRing = TimeManager.Instance.DelayInvoke(
                (() => { Pool.Release(this); }), 
                0.5f
                );
        }
    }

    private void OnDestroy()
    {
        if(_removeRing != Guid.Empty)
            TimeManager.Instance.CancelInvoke(_removeRing);
    }

    #endregion

    #region Public Methods

    public void StartRing()
    {
        ResetRing();
        Run = true;
        _time = 4 * (AudioManager.Tempo / 60) * AudioManager.TimeFactor;
        _sizePerSecond = 1 / _time;
    }

    public void ResetRing()
    {
        if(transform == null) return; // in case the object was destroyed
        
        Run = false;
        transform.localScale = _originalScale;
        _currColor = _origColor;
        _currColor.a = _startingAlpha;
        _renderer.color = _currColor;
        if (_ringTrigger != null)
        {
            _ringTrigger.UnregisterRing(this);
        }
    }

    #endregion

    #region Private Methods

    #endregion
}
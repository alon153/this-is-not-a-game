using System;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class RhythmRing : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Color _onBeatColor = Color.green;

    #endregion

    #region Non-Serialized Fields

    private Vector3 _originalScale;
    private float _time;
    private float _sizePerSecond;
    private RingTrigger _ringTrigger;

    private SpriteRenderer _renderer;
    private Guid _removeRing = Guid.Empty;

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
    }

    private void Start()
    {
        _time = 4 * (AudioManager.Tempo / 60);
        _sizePerSecond = 1 / _time;
    }

    private void Update()
    {
        if (Run)
            transform.localScale += new Vector3(_sizePerSecond, _sizePerSecond, 0) * Time.deltaTime;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("RingTrigger"))
        {
            if (_ringTrigger != null)
                _ringTrigger.UnregisterRing(this);

            _ringTrigger = other.GetComponent<RingTrigger>();
            _ringTrigger.RegisterRing(this);
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
        _renderer.color = Color.white;
        if (_ringTrigger != null)
        {
            _ringTrigger.UnregisterRing(this);
        }
    }

    #endregion

    #region Private Methods

    #endregion
}
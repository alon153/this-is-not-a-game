using System;
using Managers;
using UnityEngine;

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
    private float _beatAt = 0;
    private float _beatFactor = 0;

    private SpriteRenderer _renderer;

    #endregion

    #region Properties

    public bool Run { get; private set; } = false;

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
        {
            transform.localScale += new Vector3(_sizePerSecond, _sizePerSecond, 0) * Time.deltaTime;
            if (transform.localScale.x >= 1)
                ResetRing();
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
            _renderer.color = _onBeatColor;
        }
    }

    #endregion

    #region Public Methods

    public void StartRing()
    {
        if (_beatAt == 0)
        {
            _beatAt = Time.time;
            return;
        }
        
        if (_beatFactor == 0)
        {
            _time = 4 * (AudioManager.Tempo / 60);
            _beatFactor = (Time.time - _beatAt) / _time;
            print(_beatFactor);
            _time *= _beatFactor;
            _beatAt = Time.time;
            return;
        }

        Run = true;
        _sizePerSecond = 1 / _time;
    }

    public void ResetRing()
    {
        Run = false;
        transform.localScale = _originalScale;
        _renderer.color = Color.white;
        if (_ringTrigger != null)
        {
            _ringTrigger.UnregisterRing(this);
            _ringTrigger = null;
        }
    }

    #endregion

    #region Private Methods

    #endregion
}
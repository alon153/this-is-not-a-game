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
    private float _shrinkPerSecond;
    private RingTrigger _ringTrigger;

    private SpriteRenderer _renderer;

    #endregion

    #region Properties

    public bool Run { get; private set; } = false;

    #endregion

    #region Function Events

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

    private void Start()
    {
        _time = 4 * (AudioManager.Tempo / 60);
        _shrinkPerSecond = _originalScale.magnitude / _time;
    }

    private void Update()
    {
        if (Run)
        {
            transform.localScale -= new Vector3(_shrinkPerSecond, _shrinkPerSecond, 0) * Time.deltaTime;
            if (transform.localScale.magnitude <= 0.05f)
                ResetRing();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
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
        Run = true;
        _time = 4 * (AudioManager.Tempo / 60);
        _shrinkPerSecond = _originalScale.magnitude / _time;
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
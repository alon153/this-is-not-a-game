using System;
using System.Collections;
using System.Collections.Generic;
using GameMode;
using GameMode.Ikea;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Utilities;
using Utilities.Interfaces;

namespace Basics.Player
{
    public partial class PlayerController : MonoBehaviour, IFallable
    {
        #region Constants

        private const int DefaultIndex = -1;
        
        private const float DecThreshold = 0.2f;

        #endregion
        
        #region Serialized Fields

        [SerializeField] private Material _bloomMaterialOrigin;

        [Header("UI")] 
        [SerializeField] private TextMeshProUGUI _txtReady;
        [SerializeField] private TextMeshProUGUI _txtInteract;
        [SerializeField] private TextMeshProUGUI _txtStun;

        [Header("Movement")] 
        [SerializeField] private float _speed = 2;
        [SerializeField] private float _maxSpeed = 2;
        [SerializeField] private float _acceleration = 2;
        [SerializeField] private float _deceleration = 2;

        [Header("Dash")] 
        [field: SerializeField] public float DashTime = 0.5f;
        [SerializeField] private float _dashBonus = 1;
        [SerializeField] private float _dashCooldown = 0.5f;
        [SerializeField] private float _knockBackForce = 20f;
        [SerializeField] private float _mutualKnockBackForce = 1.5f;
        [Tooltip("The time a player is knocked back")][SerializeField] private float _knockBackDelay = 0.15f;
       
        #endregion

        #region Non-Serialized Fields

        private Vector3 _origScale;
        
        private bool _ready;
        private Color _color;

        private Vector3 _dashDirection; // used so we can keep tracking the input direction without changing dash direction
        private bool _canDash = true;
        
        private Vector2 _direction;

        private Vector2 _pushbackVector;

        private bool _frozen = false;
        private Guid _freezeId = Guid.Empty;
        
        private bool _canMove = true;

        public SpriteRenderer Renderer { get; private set; }
        private Vector3 _originalScale;

        private Vector3 _lastPosition;
        private Color _origColor;

        private PlayerInput _input;

        private Material _bloomMat;

        #endregion

        #region Properties

        public Gamepad Gamepad { get; private set; } = null;
        public int Index { get; private set; } = DefaultIndex;

        private bool CanDash
        {
            get => _canDash;
            set
            {
                _canDash = value;
                Renderer.material.SetFloat("_ColorFactor",value ? 1 : 0.2f);
            }
        }
        private Vector2 DesiredVelocity => _direction * _speed;
        private float DashSpeed => _maxSpeed + _dashBonus;

        private Vector2 Direction
        {
            get => _direction;
            set => _direction = value.normalized;
        } 
        
        private bool dashing { get; set; } = false;
        public Rigidbody2D Rigidbody { get; set; }

        public bool Ready
        {
            get => _ready;
            set
            {
                _ready = value;
                _txtReady.enabled = value;
                GameManager.Instance.SetReady(Index, value);
            }
        }
        
        public Color Color { 
            get => _color;
            private set
            {
                _color = value;
                Renderer.material.SetColor("_Color",value.Intensify(1.5f));
            }
        }

        public PlayerAddon Addon { get; set; }

        #endregion

        #region Function Events

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Renderer = GetComponent<SpriteRenderer>();
            _input = GetComponent<PlayerInput>();

            _bloomMat = new Material(_bloomMaterialOrigin);
            Renderer.material = _bloomMat;
            
            if (_input.currentControlScheme == "Gamepad")
                Gamepad = _input.devices[0] as Gamepad;

            _lastPosition = transform.position;
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            Index = GameManager.Instance.RegisterPlayer(this);
            Color = GameManager.Instance.PlayerColors[Index];
            _origColor = Color;
            Ready = false;
            _txtInteract.enabled = false;
            _txtStun.enabled = false;
        }

        private void Update()
        {
            if (Interactable != null && !Interactable.CanInteract)
                Interactable = null;
            
            var pos = transform.position;
            if (pos != _lastPosition)
            {
                foreach (var l in _moveListeners)
                {
                    l.OnMove(this, _lastPosition, pos);
                }
                _lastPosition = pos;
            }
        }

        private void FixedUpdate()
        {
            ModifyPhysics();
            if(!_frozen)
                MoveCharacter();
        }

        #endregion

        #region ActionMap

        // =============================== Action Map ======================================================================

        public void OnMove(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    Direction = context.ReadValue<Vector2>();
                    break;
                case InputActionPhase.Canceled:
                    Direction = Vector2.zero;
                    break;
            }
        }
        
        public void OnDash(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if(!CanDash)
                        return;
                    
                    _dashDirection = _direction.normalized;
                    
                    dashing = true;
                    CanDash = false;

                    TimeManager.Instance.DelayInvoke(() => { CanDash = true; }, _dashCooldown);
                    
                    TimeManager.Instance.DelayInvoke(() =>
                    {
                        dashing = false;
                    }, DashTime);
                    break;
            }
        }
        
        public void OnAction(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if (Interactable != null)
                    {
                        Interactable.OnInteract(this);
                        if(Interactable && !Interactable.IsHold)
                            Interactable = null;
                    }
                    break;
                case InputActionPhase.Canceled:
                    if (Interactable != null)
                    {
                        Interactable.OnInteract(this, false);
                        Interactable = null;
                    }
                    break;
                    
            }
        }

        public void OnToggleReady(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if(GameManager.Instance.CurrentState == GameState.Lobby)
                        Ready = !Ready;
                    break;
            }
        }

        #endregion

        #region Public Methods

        private void ToggleInteractText(bool show, string text = "Press A")
        {
            _txtInteract.text = text;
            _txtInteract.enabled = show;
        }

        public void Freeze(bool timed=true, float time = 2, bool stunned=false)
        {
            if (_freezeId != Guid.Empty) {
                TimeManager.Instance.CancelInvoke(_freezeId);
                _freezeId = Guid.Empty;
            }
            Rigidbody.velocity = Vector2.zero;
            _frozen = true;
            
            if(timed)
                _freezeId = TimeManager.Instance.DelayInvoke(UnFreeze, time);

            if (stunned)
                _txtStun.enabled = true;
        }

        public void UnFreeze()
        {
            if (_freezeId != Guid.Empty) {
                TimeManager.Instance.CancelInvoke(_freezeId);
                _freezeId = Guid.Empty;
            }
            _frozen = false;
            _txtStun.enabled = false;
        }

        public bool GetIsDashing()
        { 
            return dashing;
        }

        public void SetMovementAbility(bool canMove)
        {
            _canMove = canMove;
        }
        
        public void Respawn(bool stun=false)
        {
            transform.position = GameManager.Instance.CurrArena.GetRespawnPosition(gameObject);
            Reset();
            Freeze(true,1,true);
        }

        
        #endregion

        #region Private Methods

        private void MoveCharacter()
        {
            if (!_canMove) return;
            
            if (dashing)
            {
                Rigidbody.velocity = _dashDirection * DashSpeed;
                return;
            }

            if (_pushbackVector != Vector2.zero)
            {
                Rigidbody.velocity = _pushbackVector;
                _pushbackVector = Vector2.zero;
            }
            else
            {
                Rigidbody.velocity = Vector2.Lerp(Rigidbody.velocity,
                    DesiredVelocity,
                    _acceleration * Time.fixedDeltaTime);

                if (DesiredVelocity.magnitude > _maxSpeed)
                {
                    Rigidbody.velocity = DesiredVelocity.normalized * _maxSpeed;
                }
                
            }
        }

        private void ModifyPhysics()
        {
            var changingDirection = Vector3.Angle(_direction, Rigidbody.velocity) >= 90;

            // Make "linear drag" when changing direction
            if (changingDirection)
            {
                Rigidbody.velocity = Vector2.Lerp(
                    Rigidbody.velocity,
                    Vector2.zero,
                    _deceleration * Time.fixedDeltaTime);
            }

            if (_direction.magnitude == 0 && Rigidbody.velocity.magnitude < DecThreshold)
            {
                Rigidbody.velocity *= Vector2.zero;
            }
        }

        private void Reset()
        {
            var color = Renderer.color;
            color.a = 1;
            transform.localScale = _originalScale;
            Renderer.color = color;
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.drag = 0;
            UnFreeze();
        }

       
        #endregion
    }
}
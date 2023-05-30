using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using GameMode;
using GameMode.Ikea;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utilities;
using Utilities.Interfaces;

namespace Basics.Player
{
    public partial class PlayerController : MonoBehaviour, IFallable, IAudible<PlayerSounds>
    {
        #region Constants

        private const int DefaultIndex = -1;

        private const float DecThreshold = 0.2f;

        #endregion

        #region Serialized Fields

        [SerializeField] private Material _bloomMaterialOrigin;

        [Header("UI")] [SerializeField] private TextMeshProUGUI _txtReady;
        [SerializeField] private TextMeshProUGUI _txtInteract;
        [SerializeField] private TextMeshProUGUI _txtStun;

        [Header("Movement")] [SerializeField] private float _speed = 2;
        [SerializeField] private float _maxSpeed = 2;
        [SerializeField] private float _acceleration = 2;
        [SerializeField] private float _deceleration = 2;

        [Header("Dash")] [field: SerializeField]
        public float DashTime = 0.5f;

        [SerializeField] private float _dashBonus = 1;
        [SerializeField] private float _dashCooldown = 0.5f;
        [SerializeField] private float _knockBackForce = 20f;
        [SerializeField] private float _mutualKnockBackForce = 1.5f;

        [Tooltip("How much time will a player be pushed after dash is finished")] [SerializeField]
        private float _postDashPushTime = 0.1f;

        [Tooltip("The time a player is knocked back")] [SerializeField]
        private float _knockBackDelay = 0.15f;

        #endregion

        #region Non-Serialized Fields

        private Vector3 _origScale;

        private bool _ready;
        private Color _color;

        private Vector3
            _dashDirection; // used so we can keep tracking the input direction without changing dash direction

        private bool _canDash = true;
        private bool _isInPostDash = false;

        private Vector2 _direction;

        private Vector2 _pushbackVector;

        private bool _frozen = false;
        private Guid _freezeId = Guid.Empty;
        private bool _stunned;

        private bool _canMove = true;
        private bool _dashing;

        [field: SerializeField] public PlayerRenderer Renderer { get; private set; }

        private Vector3 _originalScale;

        private Vector3 _lastPosition;
        private Color _origColor;

        private PlayerInput _input;

        private Material _bloomMat;
        private Guid _dashingId;
        private Guid _postDashId;

        private static readonly int Moving = Animator.StringToHash("Moving");
        private static readonly int Stunned1 = Animator.StringToHash("Stunned");
        private static readonly int Dashing1 = Animator.StringToHash("Dashing");
        private static readonly int MoveX = Animator.StringToHash("moveX");
        private static readonly int MoveY = Animator.StringToHash("moveY");

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
                Renderer.ToggleBloom(value);
            }
        }

        public bool Stunned
        {
            get => _stunned;
            private set
            {
                bool shouldChange = _stunned != value;
                if (shouldChange)
                    Renderer.Animator.SetBool(Stunned1, value);
                _stunned = value;
            }
        }

        private Vector2 DesiredVelocity => _direction * _speed;
        private float DashSpeed => _maxSpeed + _dashBonus;

        public Vector2 Direction
        {
            get => _direction;
            set
            {
                bool shouldChangeAnimation = (_direction.magnitude == 0 && value.magnitude != 0) ||
                                             (_direction.magnitude != 0 && value.magnitude == 0);
                if (shouldChangeAnimation)
                    Renderer.Animator.SetBool(Moving, value.magnitude != 0);

                _direction = value.normalized;
                bool facingBack = _direction.y > 0 && Mathf.Abs(_direction.x) <= 0.1f;
                if (facingBack != Renderer.FaceBack)
                    Renderer.FaceBack = facingBack;
                Renderer.Animator.SetFloat(MoveX, Mathf.Abs(_direction.x) <= 0.1f ? 0 : _direction.x);
                Renderer.Animator.SetFloat(MoveY, Mathf.Abs(_direction.y) <= 0.1f ? 0 : _direction.y);
            }
        }

        private bool Dashing
        {
            get => _dashing;
            set
            {
                bool shouldChange = _dashing != value;
                if (shouldChange)
                    Renderer.Animator.SetBool(Dashing1, value);
                _dashing = value;
            }
        }

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

        public Color Color
        {
            get => _color;
            private set
            {
                _color = value;
                Renderer.SetGlobalColor(value);
            }
        }

        public PlayerAddon Addon { get; set; }

        #endregion

        #region Function Events

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInput>();

            Renderer.BloomMaterial = new Material(_bloomMaterialOrigin);

            if (_input.currentControlScheme == "Gamepad")
                Gamepad = _input.devices[0] as Gamepad;

            _lastPosition = transform.position;
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            Index = GameManager.Instance.RegisterPlayer(this);
            _origColor = GameManager.Instance.PlayerColor(Index);
            Renderer.Animator.runtimeAnimatorController = GameManager.Instance.PlayerAnimatorOverride(Index);
            Color = _origColor;
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
            if (!_frozen)
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
                    if (Direction == Vector2.zero) return;

                    if (!CanDash)
                    {
                        ((IAudible<PlayerSounds>) this).PlayOneShot(PlayerSounds.DashCooldown);
                        return;
                    }

                    Dash();
                    break;
            }
        }

        public void OnAction(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if (Interactable != null)
                        Interactable.OnInteract(this);
                    else if (Addon is PlayerActionAddOn)
                    {
                        ((PlayerActionAddOn) Addon).OnAction(this);
                    }
                    break;
                case InputActionPhase.Canceled:
                    if (Interactable != null)
                        Interactable.OnInteract(this, false);

                    break;
            }
        }

        public void OnToggleReady(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if (GameManager.Instance.CurrentState == GameState.Lobby)
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

        public void Freeze(bool timed = true, float time = 2, bool stunned = false)
        {
            if (_freezeId != Guid.Empty)
            {
                TimeManager.Instance.CancelInvoke(_freezeId);
                _freezeId = Guid.Empty;
            }

            Rigidbody.velocity = Vector2.zero;
            _frozen = true;

            if (timed)
                _freezeId = TimeManager.Instance.DelayInvoke(UnFreeze, time);

            if (stunned)
                _txtStun.enabled = true;
        }

        public void UnFreeze()
        {
            if (_freezeId != Guid.Empty)
            {
                TimeManager.Instance.CancelInvoke(_freezeId);
                _freezeId = Guid.Empty;
            }

            _frozen = false;
            _txtStun.enabled = false;
        }

        public bool GetIsDashing()
        {
            return Dashing;
        }

        public void SetMovementAbility(bool canMove)
        {
            _canMove = canMove;
        }

        public void Respawn(bool stun = false)
        {
            transform.position = GameManager.Instance.CurrArena.GetRespawnPosition(gameObject);
            Reset();
            Freeze(stun, 1, stun);
        }

        #endregion

        #region Private Methods

        private void Dash()
        {
            _dashDirection = _direction.normalized;

            Dashing = true;
            CanDash = false;

            TimeManager.Instance.DelayInvoke(() => { CanDash = true; }, _dashCooldown);
            ((IAudible<PlayerSounds>) this).PlayOneShot(PlayerSounds.Dash);
            Renderer.FaceBack = true;

            _dashingId = TimeManager.Instance.DelayInvoke(() =>
            {
                Dashing = false;
                Renderer.FaceBack = _direction.y > 0 && Mathf.Abs(_direction.x) <= 0.1f;
                ;
                _isInPostDash = true;
                _postDashId = TimeManager.Instance.DelayInvoke(() => { _isInPostDash = false; }, _postDashPushTime);
            }, DashTime);
        }

        private void CancelDash()
        {
            Dashing = false;
            _isInPostDash = false;
            if (_dashingId != Guid.Empty)
            {
                TimeManager.Instance.CancelInvoke(_dashingId);
                _dashingId = Guid.Empty;
            }

            if (_postDashId != Guid.Empty)
            {
                TimeManager.Instance.CancelInvoke(_postDashId);
                _postDashId = Guid.Empty;
            }
        }

        private void MoveCharacter()
        {
            if (!_canMove) return;

            if (Dashing)
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
            transform.localScale = _originalScale;
            Renderer.SetGlobalColor(_origColor);
            Renderer.Regular.flipX = false;
            Renderer.Bloomed.flipX = false;
            Renderer.SetActive(true);
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.drag = 0;
            UnFreeze();
        }

        #endregion

        public SoundType GetSoundType()
        {
            return SoundType.Player;
        }
    }
}
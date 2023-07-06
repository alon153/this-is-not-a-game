using System;
using Audio;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

        [SerializeField] private TextMeshProUGUI _txtInteract;
        [SerializeField] private Transform _playerFront;
        [SerializeField] public PlayerEffect PlayerEffect;

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
        [SerializeField] private ParticleSystem _dashParticles;

        [Tooltip("How much time will a player be pushed after dash is finished")] 
        [SerializeField] private float _postDashPushTime = 0.1f;

        [Tooltip("The time a player is knocked back")] 
        [SerializeField] private float _knockBackDelay = 0.15f;
        #endregion

        #region Non-Serialized Fields

        private Vector3 _origScale;

        private bool _ready;

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
        private Guid _dashingId = Guid.Empty;
        private Guid _postDashId = Guid.Empty;
        private Guid _vibrationId = Guid.Empty;
        private static readonly int Moving = Animator.StringToHash("moving");

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
                Renderer.ToggleOutline(value);
            }
        }

        public bool Stunned
        {
            get => _stunned;
            private set
            {
                bool shouldChange = _stunned != value;
                _stunned = value;
            }
        }

        public bool Frozen => _frozen;

        private Vector2 DesiredVelocity => _direction * _speed;
        private float DashSpeed => _maxSpeed + _dashBonus;

        public Vector2 Direction
        {
            get => _direction;
            set
            {
                bool shouldChangeAnimation = (_direction.magnitude == 0 && value.magnitude != 0) ||
                                             (_direction.magnitude != 0 && value.magnitude == 0);

                if (value == Vector2.zero && _direction != Vector2.zero ||
                    value != Vector2.zero && _direction == Vector2.zero)
                    Renderer.Animator.SetBool(Moving,value != Vector2.zero);
                
                _direction = value.normalized;
                bool facingRight = _direction.x > 0;
                if (facingRight != Renderer.Regular.flipX && Mathf.Abs(_direction.x) >= 0.1f)
                {
                    Renderer.Regular.flipX = facingRight;
                    var pos = _playerFront.localPosition;
                    pos.x = Mathf.Abs(pos.x) * (facingRight ? 1 : -1);
                    _playerFront.localPosition = pos;
                    FlipAllFrontObjects(facingRight);
                }
                    
            }
        }

        private bool Dashing
        {
            get => _dashing;
            set
            {
                bool shouldChange = _dashing != value;
                if(shouldChange)
                    SetDashAnimation(value);
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
                GameManager.Instance.SetReady(Index, value);
            }
        }

        public Color Color { get; private set; }

        public PlayerAddon Addon { get; set; }

        #endregion

        #region Function Events

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInput>();

            if (_input.currentControlScheme == "Gamepad")
                Gamepad = _input.devices[0] as Gamepad;

            _lastPosition = transform.position;
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            Index = GameManager.Instance.RegisterPlayer(this);
            _origColor = GameManager.Instance.PlayerColor(Index);
            Color = _origColor;
            Renderer.Init(Color);
            GameManager.Instance.SetDefaultSprite(this);
            _txtInteract.enabled = false;
            

            SetParticlesColors();
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

        private void OnDestroy()
        {
            StopVibration();
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

                    if (!CanDash || Frozen)
                    {
                        // ((IAudible<PlayerSounds>) this).PlayOneShot(PlayerSounds.DashCooldown);
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
                    if (GameManager.Instance.State is GameState.Lobby or GameState.Instructions)
                    {
                        Ready = !Ready;
                        return;
                    }
                    
                    if (Interactable != null)
                    {
                        if(Interactable.IsHold)
                            SetLongActionAnimation(true);
                        else
                            SetActionAnimation();
                        Interactable.OnInteract(this);
                    }
                    else if (Addon is PlayerActionAddOn)
                    {
                        ((PlayerActionAddOn) Addon).OnAction(this);
                    }
                    break;
                case InputActionPhase.Canceled:
                    if (Interactable != null)
                    {
                        if(Interactable.IsHold)
                            SetLongActionAnimation(false);
                        Interactable.OnInteract(this, false);
                    }
                    break;
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {   
           UIManager.Instance.OnPressStart();
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
                PlayerEffect.PlayStunAnimation();
        }

        public void UnFreeze()
        {
            if(GameManager.Instance.State == GameState.Instructions)
                return;
            
            if (_freezeId != Guid.Empty)
            {
                TimeManager.Instance.CancelInvoke(_freezeId);
                _freezeId = Guid.Empty;
            }

            _frozen = false;
            PlayerEffect.StopStunAnimation();
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

        public void SetObjectInFront(GameObject objectToSet)
        {
            objectToSet.transform.parent = _playerFront;
            objectToSet.transform.localPosition = Vector3.zero;
            var spriteRenderer = objectToSet.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "Player";
            spriteRenderer.sortingOrder = 1;
        }

        public void SetVibration(float amp, float offTimer=-1)
        {
            if (_vibrationId != Guid.Empty)
            {
                TimeManager.Instance.CancelInvoke(_vibrationId);
                _vibrationId = Guid.Empty;
            }

            Gamepad?.SetMotorSpeeds(amp, amp);
            if (offTimer >= 0)
                _vibrationId = TimeManager.Instance.DelayInvoke(StopVibration, offTimer);
        }

        public void StopVibration() => SetVibration(0);

        #endregion

        #region Private Methods
        
        private void SetParticlesColors()
        {
            var color = _dashParticles.colorOverLifetime;
            color.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys( new GradientColorKey[]
            {
                new GradientColorKey(Color.Intensify(0.7f), 0.0f), new GradientColorKey(Color, 1.0f)
            }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.9f, 0.8f), new GradientAlphaKey(0.0f, 1.0f) } );
            color.color = grad;

            var fallParticlesMain = _fallParticles.main;
            fallParticlesMain.startColor = Color;
        }

        private void Dash()
        {
            _dashDirection = _direction.normalized;

            Dashing = true;
            CanDash = false;

            float z = Vector2.Angle(Vector2.right, Direction);
            _dashParticles.transform.rotation = Quaternion.Euler(0,0,z);
            _dashParticles.Play();
            
            TimeManager.Instance.DelayInvoke(() => { CanDash = true; }, _dashCooldown);
            AudioManager.PlayDash();

            _dashingId = TimeManager.Instance.DelayInvoke(() =>
            {
                Dashing = false;
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
                    (DesiredVelocity.magnitude > Rigidbody.velocity.magnitude ? _acceleration : _deceleration) * Time.fixedDeltaTime);

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
            Renderer.Regular.flipX = false;
            Renderer.SetActive(true);
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.drag = 0;
            UnFreeze();
        }

        private void FlipAllFrontObjects(bool flip)
        {
            foreach (Transform frontObj in _playerFront)
                frontObj.GetComponent<SpriteRenderer>().flipX = flip;
        }

        #endregion

        public SoundType GetSoundType()
        {
            return SoundType.Player;
        }

        public void ResetPlayer()
        {
            GameManager.Instance.SetDefaultSprite(this);
            Addon = null;
        }
    }
}
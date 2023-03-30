using System;
using System.Collections;
using GameMode;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Basics.Player
{
    public partial class PlayerController : MonoBehaviour
    {
        #region Constants

        private const int DEFAULT_INDEX = -1;

        #endregion
        
        #region Serialized Fields

        [Header("Movement")] 
        [SerializeField] private float _speed = 2;
        [SerializeField] private float _maxSpeed = 2;
        [SerializeField] private float _acceleration = 2;
        [SerializeField] private float _deceleration = 2;
        

        [Header("Dash")] 
        [field: SerializeField] public float DashTime = 0.5f;
        [SerializeField] private float _dashBonus = 1;
        [SerializeField] private float _dashCooldown = 0.5f;
        [SerializeField] private float _knockBackForce = 3f;
        [SerializeField] private float _mutualKnockBackForce = 1.5f;
        [SerializeField] private float _knockBackDelay = 0.15f;
        [SerializeField] private UnityEvent _onBeginKickBack;
        [SerializeField] private UnityEvent _onDoneKickBack;

        #endregion

        #region Non-Serialized Fields

        private Vector3 _dashDirection; // used so we can keep tracking the input direction without changing dash direction

        private bool _canDash = true;
        private bool _dashing;

        private Vector2 _direction;

        private const float DEC_THRESHOLD = 0.2f;

        private Vector2 _pushbackVector;

        private bool _frozen = false;
        private bool _canMove = true;
        private Guid _freezeId = Guid.Empty;

        private SpriteRenderer _renderer;
        private Vector3 _originalScale;

        #endregion

        #region Properties

        public int Index { get; set; } = DEFAULT_INDEX;

        private Vector2 DesiredVelocity => _direction * _speed;
        private float DashSpeed => _maxSpeed + _dashBonus;

        private Vector2 Direction
        {
            get => _direction;
            set => _direction = value.normalized;
        }

        public Rigidbody2D Rigidbody { get; set; }

        #endregion

        #region Function Events

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            GameManager.Instance.RegisterPlayer(this);
        }

        private void FixedUpdate()
        {
            ModifyPhysics();
            if(!_frozen)
                MoveCharacter();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player") && _dashing)
            {
                _dashing = false;
                bool isMutual = other.gameObject.GetComponent<PlayerController>().GetIsDashing();
                Debug.Log("is mutual: " + isMutual);
                KnockBackPlayer(other.gameObject, isMutual);
            }
        }

        #endregion

        #region ActionMap

        // =============================== Action Map ======================================================================

        public void OnMove(InputAction.CallbackContext context)
        {
            print("move");
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
                    _dashDirection = _direction.normalized;
                    
                    _dashing = true;
                    _canDash = false;

                    TimeManager.Instance.DelayInvoke(() => { _canDash = true; }, _dashCooldown);
                    
                    TimeManager.Instance.DelayInvoke(() =>
                    {
                        _dashing = false;
                    }, DashTime);
                    break;
            }
        }

        #endregion

        #region Public Methods

        public void Freeze(float time = 2)
        {
            if (_freezeId != Guid.Empty) {
                TimeManager.Instance.CancelInvoke(_freezeId);
                _freezeId = Guid.Empty;
            }
            Rigidbody.velocity = Vector2.zero;
            _frozen = true;
            _freezeId = TimeManager.Instance.DelayInvoke((() => { _frozen = false; }), time);
        }

        public void UnFreeze()
        {
            if (_freezeId != Guid.Empty) {
                TimeManager.Instance.CancelInvoke(_freezeId);
                _freezeId = Guid.Empty;
            }
            _frozen = false;
        }

        public bool GetIsDashing()
        { 
            return _dashing;
        }

        public void SetMovementAbility(bool canMove)
        {
            _canMove = canMove;
        }

        #endregion

        #region Private Methods

        private void MoveCharacter()
        { 
            if (!_canMove) return;
            
            if (_dashing)
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

            if (_direction.magnitude == 0 && Rigidbody.velocity.magnitude < DEC_THRESHOLD)
            {
                Rigidbody.velocity *= Vector2.zero;
            }
        }

        private void Reset()
        {
            var color = _renderer.color;
            color.a = 1;
            _renderer.color = color;
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.drag = 0;
            UnFreeze();
        }

        private void Respawn()
        {
            transform.position = GameManager.Instance.Arena.GetRespawnPosition(gameObject);
            Reset();
        }

        private void KnockBackPlayer(GameObject player, bool mutualCollision)
        {
            StopAllCoroutines();
            _onBeginKickBack?.Invoke();
            
            Rigidbody2D otherPlayerRb = player.GetComponent<Rigidbody2D>();
            PlayerController otherPlayerController = player.GetComponent<PlayerController>();
            
            Vector2 knockDir = (player.transform.position - transform.position).normalized;
            float force = mutualCollision ? _mutualKnockBackForce : _knockBackForce;
            otherPlayerController.SetMovementAbility(false);
           
            otherPlayerRb.AddForce(knockDir * force, ForceMode2D.Impulse);
            StartCoroutine(ResetMovementAfterKnockBack(otherPlayerRb, otherPlayerController));
        }

        private IEnumerator ResetMovementAfterKnockBack(Rigidbody2D otherPlayerRb, PlayerController otherPlayerControl)
        {
            yield return new WaitForSeconds(_knockBackDelay);
            otherPlayerRb.velocity = Vector2.zero;
            otherPlayerControl.SetMovementAbility(true);
            _onDoneKickBack?.Invoke();
        }

        #endregion
    }
}
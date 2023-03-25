using System;
using System.Collections;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
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

        #endregion

        #region Non-Serialized Fields

        private Vector3 _dashDirection; // used so we can keep tracking the input direction without changing dash direction

        private bool _canDash = true;
        private bool _dashing;

        private Vector2 _direction;

        private const float DEC_THRESHOLD = 0.2f;

        private Vector2 _pushbackVector;

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
        }

        private void Start()
        {
            GameManager.Instance.RegisterPlayer(this);
        }

        private void FixedUpdate()
        {
            ModifyPhysics();
            MoveCharacter();
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

        #region Private Methods
        
        
        private void MoveCharacter()
        {
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

        #endregion
    }
}
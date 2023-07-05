using System;
using Basics;
using Basics.Player;
using Managers;
using UnityEngine;
using Utilities.Interfaces;

namespace GameMode.Ikea
{
    public class IkeaPart : InteractableObject, IFallable
    {
        #region Serialized Fields

        [SerializeField] private Sprite _blueprintSprite;
        [SerializeField] private Sprite _partSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private float _pointsPerPart = 10;

        #endregion
        
        #region Non-Serialized Fields

        private static int BlueprintCount_Inner = 0; 
        public static int BlueprintCount
        {
            get => BlueprintCount_Inner;
            set
            {
                BlueprintCount_Inner = value;
                if(value <= 0)
                    GameManager.Instance.EndRound();
            }
        }

        private Rigidbody2D _rigidbody;
        private SpriteRenderer _renderer;
        private Color _origColor;
        private bool _isBlueprint;
        private bool _isInPlace;
        
        #endregion
        
        #region Properties
        
        [field: SerializeField] public Type Type { get; private set; }

        public Color Color
        {
            get => _origColor;
            set
            {
                _origColor = value;
                _renderer.color = value;
            }
        }

        public Transform Holder
        {
            get => transform.parent;
            private set
            {
                CanInteract = value == null;
                _collider.enabled = value == null;
                _rigidbody.isKinematic = value != null;
                if(value == null)
                    transform.SetParent(null);
            }
        }

        public bool IsBlueprint
        {
            get => _isBlueprint;
            set
            {
                if (_isBlueprint != value) IsInPlace = !value;

                if (_isBlueprint && !value)
                    BlueprintCount--;
                else if (!_isBlueprint && value)
                    BlueprintCount++;
                
                _isBlueprint = value;
                // _renderer.sprite = value ? _blueprintSprite : _partSprite;
                _renderer.sprite = _partSprite;
                if(_isBlueprint)
                    _renderer.sprite = _blueprintSprite;

                if (_isBlueprint)
                {
                    Holder = null;
                }

                _collider.isTrigger = value || _isInPlace;
            }
        }

        public bool IsInPlace
        {
            get => _isInPlace;
            set
            {
                _isInPlace = value;
                _rigidbody.isKinematic = value;
                _collider.isTrigger = value || _isBlueprint;
            }
        }
        
        #endregion

        #region Event Functions
        
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _origColor = _renderer.color;
            IsBlueprint = true;
        }
        
        #endregion

        #region Public Methods

        public void Drop()
        {
            Holder = null;
        }

        #endregion

        #region InteractableObject

        protected override void TogglePrompt_Inner(bool showPrompt)
        {
            _renderer.color = showPrompt ? _hintColor : Color;
        }

        protected override bool CanPlayerInteract(PlayerController player)
        {
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Ikea);

            IkeaPart playerPart = ((IkeaPlayerAddon) player.Addon).Part;
            return
                !((_isBlueprint && (playerPart == null || playerPart.Type != Type)) || // is blueprint and player isn't holding a part or is holding a different part
                  (!_isBlueprint && (_isInPlace || Holder != null))); // is part but is in place or is being held
        }

        protected override void OnInteract_Inner(PlayerController player)
        {
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Ikea);

            IkeaPart playerPart = ((IkeaPlayerAddon) player.Addon).Part;
            
            if (_isBlueprint)
            {
                Destroy(playerPart.gameObject);
                ((IkeaPlayerAddon) player.Addon).Part = null;
                
                Color = player.Color;
                IsBlueprint = false;
                ScoreManager.Instance.SetPlayerScore(player.Index,_pointsPerPart);
            }
            else
            {
                var playerTrans = player.transform;

                if (playerPart != null) // Player holding a part
                {
                    playerPart.transform.position = transform.position;
                    playerPart.Holder = null;
                }
                    
                Color = player.Color;
                transform.position = playerTrans.position + new Vector3(0,playerTrans.lossyScale.y/2,0);
                transform.rotation = Quaternion.Euler(0,0,0);
                Holder = playerTrans;
                player.SetObjectInFront(this.gameObject);
                ((IkeaPlayerAddon) player.Addon).Part = this;

                CanInteract = false;
            }
        }

        #endregion

        #region IFallable
        public void Fall(bool shouldRespawn = true, bool stun = false)
        {
            if(Holder == null)
                Destroy(gameObject);
        }
    }
    #endregion
    
    #region Enum

    public enum Type
    {
        Body, Leg, Circle 
    }

    #endregion
}
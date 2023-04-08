using System;
using Basics;
using Basics.Player;
using UnityEngine;
using Utilities.Interfaces;

namespace GameMode.Ikea
{
    public class IkeaPart : InteractableObject
    {
        #region Serialized Fields

        [SerializeField] private Sprite _blueprintSprite;
        [SerializeField] private Sprite _partSprite;
        [SerializeField] private Collider2D _collider;

        #endregion
        
        #region Non-Serialized Fields

        private Rigidbody2D _rigidbody;
        private SpriteRenderer _renderer;
        private Color _origColor;
        private bool _isBlueprint;
        private bool _isInPlace;
        
        #endregion
        
        #region Properties
        
        [field: SerializeField] public Type Type { get; private set; }

        public Transform Holder
        {
            get => transform.parent;
            private set
            {
                CanInteract = value == null;
                _collider.enabled = value == null;
                _rigidbody.isKinematic = value != null;
                transform.SetParent(value);
            }
        }

        public bool IsBlueprint
        {
            get => _isBlueprint;
            set
            {
                if (_isBlueprint != value) _isInPlace = !value;
                
                _isBlueprint = value;
                _renderer.sprite = value ? _blueprintSprite : _partSprite;

                if (_isBlueprint) Holder = null;
            }
        }
        
        #endregion

        #region Event Functions
        
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _origColor = _renderer.color;
        }
        
        #endregion

        #region InteractableObject

        protected override void TogglePrompt_Inner(bool showPrompt)
        {
            _renderer.color = showPrompt ? Color.yellow : _origColor;
        }

        protected override void OnInteract_Inner(PlayerController player)
        {
            print("inner");
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Ikea);

            IkeaPart playerPart = ((IkeaPlayerAddon) player.Addon).Part;
            
            if (_isBlueprint)
            {
                if(playerPart == null || playerPart.Type != Type)
                    return;

                ((IkeaPlayerAddon) player.Addon).Part = null;
                
                IsBlueprint = false;
                _renderer.color = player.Color;
            }
            else // Not a bluePrint
            {
                if(Holder != null) // Already being held by someone
                    return;

                if(_isInPlace) // For now, a player can't pickup or switch a part that is in place
                    return;
                
                var playerTrans = player.transform;

                if (playerPart != null) // Player not holding a part
                {
                    playerPart.transform.position = transform.position;
                    playerPart.Holder = null;
                }
                
                _renderer.color = player.Color;
                transform.position = playerTrans.position + new Vector3(0,playerTrans.lossyScale.y/2,0);
                transform.rotation = Quaternion.Euler(0,0,0);
                Holder = playerTrans;
                ((IkeaPlayerAddon) player.Addon).Part = this;
            }
        }

        #endregion
        
    }
    
    #region Enum

    public enum Type
    {
        Bar, Bolt,
    }

    #endregion
}
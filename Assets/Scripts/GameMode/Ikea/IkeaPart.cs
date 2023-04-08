using System;
using Basics;
using Basics.Player;
using UnityEngine;
using Utilities.Interfaces;

namespace GameMode.Ikea
{
    public class IkeaPart : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [SerializeField] private Sprite _blueprintSprite;
        [SerializeField] private Sprite _partSprite;

        #endregion
        
        #region Non-Serialized Fields

        private SpriteRenderer _renderer;
        private bool _isBlueprint;
        private bool _isInPlace;
        
        #endregion
        
        #region Properties
        
        [field: SerializeField] public Type Type { get; private set; }

        public Transform Holder
        {
            get => transform.parent;
            private set => transform.SetParent(value);
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
        }
        
        #endregion

        #region IInteractable
        
        public void OnInteract(PlayerController player)
        {
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
                transform.position = playerTrans.position;
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
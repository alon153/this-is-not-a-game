using System;
using Basics;
using Basics.Player;
using UnityEngine;
using Utilities.Interfaces;

namespace GameMode.Ikea
{
    public class PartDispenser : InteractableObject
    {
        #region Non-Serializable Fields

        private SpriteRenderer _renderer;
        private Color _origColor;

        #endregion
        
        #region Properties

        public IkeaPart PartPrefab { get; set; }

        #endregion

        #region Event Functions

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
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
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Ikea);
            
            IkeaPart part = Instantiate(PartPrefab, transform.position, Quaternion.identity);
            part.OnInteract(player);
        }

        #endregion
    }
}
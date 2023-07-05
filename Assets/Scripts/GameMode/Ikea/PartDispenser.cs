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

        [field: SerializeField] public IkeaPart PartPrefab { get; set; }

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
            _renderer.color = showPrompt ? _hintColor : _origColor;
        }

        protected override bool CanPlayerInteract(PlayerController player)
        {
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Ikea);

            IkeaPart playerPart = ((IkeaPlayerAddon) player.Addon).Part;
            return playerPart == null || playerPart.Type != PartPrefab.Type;
        }

        protected override void OnInteract_Inner(PlayerController player)
        {
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Ikea);
            
            IkeaPart playerPart = ((IkeaPlayerAddon) player.Addon).Part;
            if (playerPart != null)
            {
                Destroy(playerPart.gameObject);
                ((IkeaPlayerAddon) player.Addon).Part = null;
            }
            
            IkeaPart part = Instantiate(PartPrefab, transform.position, Quaternion.identity);
            part.IsBlueprint = false;
            part.IsInPlace = false;
            part.OnInteract(player);
        }

        #endregion
    }
}
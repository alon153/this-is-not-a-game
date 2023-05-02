using System;
using System.Collections.Generic;
using Basics.Player;
using Managers;
using UnityEngine;

namespace Utilities.Interfaces
{
    public abstract class InteractableObject : MonoBehaviour
    {
        #region Non-Serializable Fields

        private bool _showingPrompt = false;
        private readonly HashSet<int> _playerTriggers = new HashSet<int>();

        #endregion

        #region Properties

        public bool CanInteract { get; protected set; } = true;
        public bool IsHold { get; protected set; } = false;

        #endregion

        #region Event Functions

        protected virtual void Update()
        {
            if(_showingPrompt && (!CanInteract || _playerTriggers.Count == 0))
                TogglePrompt(false);
            else if(!_showingPrompt && (CanInteract && _playerTriggers.Count > 0))
                TogglePrompt(true);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (_playerTriggers.Contains(other.gameObject.GetInstanceID()))
                {
                    if (!CanPlayerInteract(player))
                    {
                        _playerTriggers.Remove(other.gameObject.GetInstanceID());
                    }
                    return;
                }
                
                if(player.Interactable != null || !CanPlayerInteract(player))
                    return;

                _playerTriggers.Add(other.gameObject.GetInstanceID());
                player.Interactable = this;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if(!_playerTriggers.Contains(other.gameObject.GetInstanceID()))
                    return;

                _playerTriggers.Remove(other.gameObject.GetInstanceID());
                
                PlayerController player = other.GetComponent<PlayerController>();
                if (player.Interactable != null && player.Interactable.GetInstanceID() == GetInstanceID())
                    player.Interactable = null;
            }
        }

        #endregion

        #region Public Methods

        public void OnInteract(PlayerController player, bool pressed=true)
        {
            if (CanInteract && CanPlayerInteract(player))
            {
                if (pressed)
                    OnInteract_Inner(player);
                else
                    OnStopInteract_Inner(player);
            }
        }

        public void TogglePrompt(bool showPrompt)
        {
            if (showPrompt != _showingPrompt)
            {
                TogglePrompt_Inner(showPrompt);
                _showingPrompt = showPrompt;
            }
        }

        protected virtual void OnStopInteract_Inner(PlayerController player) { }

        #endregion
        
        #region Abstract Methods
        
        protected abstract void TogglePrompt_Inner(bool showPrompt);

        protected abstract void OnInteract_Inner(PlayerController player);

        protected abstract bool CanPlayerInteract(PlayerController player);

        #endregion
    }
}
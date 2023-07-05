using System.Collections.Generic;
using System.Linq;
using Basics.Player;
using UnityEngine;

namespace Basics
{
    public abstract class InteractableObject : MonoBehaviour
    {
        #region Serializable Fields

        [Header("InteractableObject")]
        
        [SerializeField] protected Color _hintColor = Color.yellow;

        #endregion
        #region Non-Serializable Fields

        private bool _showingPrompt = false;
        private readonly Dictionary<int, PlayerController> _playerTriggers = new();
        private bool _canInteract = true;

        #endregion

        #region Properties

        public bool CanInteract
        {
            get => _canInteract;
            protected set
            {
                if (_canInteract != value)
                {
                    foreach (var id in _playerTriggers.Keys)
                    {
                        _playerTriggers[id].Interactable = value ? this : null;
                    }
                }

                _canInteract = value;
            }
        }

        public bool IsHold { get; protected set; } = false;

        #endregion

        #region Event Functions

        protected virtual void Update()
        {
            var playersCanInteract = _playerTriggers.Values.Any(
                (player => player.Interactable != null
                           && player.Interactable.GetInstanceID() == GetInstanceID())
            );
            if (_showingPrompt && (!CanInteract || !playersCanInteract))
                TogglePrompt(false);
            else if (!_showingPrompt && (CanInteract && playersCanInteract))
                TogglePrompt(true);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (!_playerTriggers.ContainsKey(other.gameObject.GetInstanceID()))
                {
                    _playerTriggers[other.gameObject.GetInstanceID()] = player;
                }

                if (player.Interactable == null)
                    player.Interactable = this;

                if (player.Interactable != null
                    && player.Interactable.GetInstanceID() == GetInstanceID()
                    && (!CanInteract || !CanPlayerInteract(player)))
                    player.Interactable = null;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (!_playerTriggers.ContainsKey(other.gameObject.GetInstanceID()))
                    return;

                _playerTriggers.Remove(other.gameObject.GetInstanceID());

                PlayerController player = other.GetComponent<PlayerController>();
                if (player.Interactable != null && player.Interactable.GetInstanceID() == GetInstanceID())
                {
                    OnStopInteract_Inner(player);
                    player.Interactable = null;   
                }
            }
        }

        #endregion

        #region Public Methods

        public void OnInteract(PlayerController player, bool pressed = true)
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

        protected virtual void OnStopInteract_Inner(PlayerController player)
        {
        }

        #endregion

        #region Abstract Methods

        protected abstract void TogglePrompt_Inner(bool showPrompt);

        protected abstract void OnInteract_Inner(PlayerController player);

        protected abstract bool CanPlayerInteract(PlayerController player);

        #endregion
    }
}
using Basics;
using Basics.Player;
using UnityEngine;
using Utilities.Interfaces;

namespace GameMode.Ikea
{
    public class PartDispenser : MonoBehaviour, IInteractable
    {
        #region Properties

        public IkeaPart PartPrefab { get; set; }

        #endregion

        #region IInteractable

        public void OnInteract(PlayerController player)
        {
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Ikea);
            
            IkeaPart part = Instantiate(PartPrefab, transform.position, Quaternion.identity);
            part.OnInteract(player);
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Basics;
using Basics.Player;
using Managers;
using Utilities.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Boats
{   
    [Serializable]
    public class BoatsInRiverArena : Arena
    {
        protected override void OnTriggerExit2D(Collider2D other)
        {
            if (GameManager.Instance.CurrArena.GetInstanceID() == this.GetInstanceID())
            {
                IFallable fallable = other.gameObject.GetComponent<IFallable>();
                if (fallable != null)
                {
                    // player has fell from arena, so shouldn't respawn
                    fallable.Fall(false);

                    if (other.CompareTag("Player"))
                    {
                       var playerId = other.gameObject.GetComponent<PlayerController>().GetInstanceID();
                        OnPlayerDisqualified(playerId);
                    }
                }
            }
        }
        
        
        
    }
}

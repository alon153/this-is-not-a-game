using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Basics;
using Basics.Player;
using FMODUnity;
using Managers;
using Utilities.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Boats
{   
    [Serializable]
    public class BoatsInRiverArena : Arena
    {
        [SerializeField] private ParticleSystem _bloodParticles;
        [SerializeField] private EventReference _scream;
        
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
                       var player = other.gameObject.GetComponent<PlayerController>();
                       OnPlayerDisqualified(player.GetInstanceID());

                       Vector3 pos = player.transform.position;
                       pos.z = _bloodParticles.transform.position.z;
                       Instantiate(_bloodParticles,pos,Quaternion.identity).Play();
                       AudioManager.PlayOneShot(_scream);
                    }
                }
            }
        }
        
        
        
    }
}

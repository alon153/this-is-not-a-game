using System;
using UnityEngine;
using UnityEngine.Events;
using Basics.Player;
using Managers;


namespace GameMode.Juggernaut
{   
  
    public class Totem : MonoBehaviour
    {   
        public UnityAction<PlayerController> OnTotemPickedUp { set; get; }
        
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                foreach (var player in GameManager.Instance.Players)
                {
                    if (player.gameObject.GetInstanceID().Equals(other.gameObject.GetInstanceID()))
                    {   
                        Debug.Log("Player has picked up totem");
                        OnTotemPickedUp.Invoke(player);
                    }
                }
            }
        }
    }
}

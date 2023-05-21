using System;
using Basics;
using UnityEngine;
using UnityEngine.Serialization;
using Managers;


namespace GameMode.Juggernaut
{
    public class Projectile : MonoBehaviour
    {
        public Rigidbody2D rigidBody;

        // Start is called before the first frame update
        private void Start()
        {
            //rigidBody = GetComponent<Rigidbody2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                foreach (var player in GameManager.Instance.Players)
                {
                    if (player.gameObject.GetInstanceID().Equals(other.gameObject.GetInstanceID()))
                    {   
                        PlayerAddon.CheckCompatability(player.Addon, GameModes.Juggernaut);
                        ((JuggernautPlayerAddOn) player.Addon).OnHit(this, player);
                        break;
                    }
                }
            }
        }
    }
}

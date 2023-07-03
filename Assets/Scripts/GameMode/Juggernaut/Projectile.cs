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

        public SpriteRenderer spriteRenderer;

        // Start is called before the first frame update
       
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

        public void SetProjectileColor(Color newColor) => spriteRenderer.color = newColor;

        public void SetProjectileRotation(Vector2 playerDir)
        {
            // Rotate the arrow object around the Z-axis
            float angle = Vector2.SignedAngle(Vector2.down, playerDir);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        public void ResetRotation() => transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        



    }
}

using Basics.Player;
using UnityEngine;

namespace GameMode.Pool
{

    public class PoolBorder : MonoBehaviour
    {
        [SerializeField] private float knockBackForce = 15f;
    
        private void KnockBack(GameObject player)
        {   
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController.GetBashingPlayer() == null)
                return;
            
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            
            
            Vector2 knockDir = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(knockDir * knockBackForce, ForceMode2D.Impulse);

            StartCoroutine(playerController.ResetMovementAfterKnockBack(playerRb, playerController.Index));


        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                KnockBack(other.gameObject);
            }

        }
    }
}

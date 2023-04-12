using UnityEngine;

namespace GameMode.Boats
{
    public class RiverObstacle : MonoBehaviour
    {
        private Rigidbody2D obstacleRigidbody2D;

        private void Start()
        {
            obstacleRigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void SetDrag(float newDrag)
        {
            obstacleRigidbody2D.drag = newDrag;
        }

        public void SetGravity(float newGravity)
        {
            obstacleRigidbody2D.gravityScale = newGravity;
        }
    }
    
}

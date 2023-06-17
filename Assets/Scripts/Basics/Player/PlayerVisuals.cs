using UnityEngine;

namespace Basics.Player
{
    partial class PlayerController
    {
        private static readonly int MoveX = Animator.StringToHash("moveX");
        private static readonly int MoveY = Animator.StringToHash("moveY");
        
        private static readonly int LongX = Animator.StringToHash("longX");
        private static readonly int LongY = Animator.StringToHash("longY");
        private static readonly int ActionX = Animator.StringToHash("actionX");
        private static readonly int ActionY = Animator.StringToHash("actionY");
        private static readonly int DeathX = Animator.StringToHash("deathX");
        private static readonly int DeathY = Animator.StringToHash("deathY");
        
        private static readonly int Hold = Animator.StringToHash("hold");
        private static readonly int Action = Animator.StringToHash("action");
        private static readonly int Death = Animator.StringToHash("death");

        #region Properties

        private Vector2 CardinalDirection
        {
            get
            {
                var velocity = Rigidbody.velocity;
                if(velocity.magnitude <= 0.2f)
                    return Vector2.zero;
                return velocity.x > velocity.y
                    ? Vector2.right * Mathf.Sign(velocity.x)
                    : Vector2.up * Mathf.Sign(velocity.y);
            }
        }
        
        #endregion

        #region Private Methods

        private void SetAnimationDirection(int xHash, int yHash, Vector2 values)
        {
            Renderer.Animator.SetFloat(xHash, values.x);
            Renderer.Animator.SetFloat(yHash, values.y);
        }
        
        private void SetAnimationDirection(string xName, string yName, Vector2 values)
        {
            Renderer.Animator.SetFloat(xName, values.x);
            Renderer.Animator.SetFloat(yName, values.y);
        }

        private void UpdateMovementAnimation()
        {
            SetAnimationDirection(MoveX, MoveY, (Dashing ? 1 : 0.5f) * CardinalDirection);
        }
        
        private void SetActionAnimation()
        {
            SetAnimationDirection(ActionX, ActionY, CardinalDirection);
            Renderer.Animator.SetTrigger(Action);
        }
        
        private void SetDeathAnimation()
        {
            SetAnimationDirection(DeathX, DeathY, CardinalDirection);
            Renderer.Animator.SetTrigger(Death);
        }

        private void SetLongActionAnimation(bool holding)
        {
            if (holding)
            {
                SetAnimationDirection(LongX, LongY, CardinalDirection);
            }
            
            Renderer.Animator.SetBool(Hold,holding);
        }

        #endregion
        
    }
}
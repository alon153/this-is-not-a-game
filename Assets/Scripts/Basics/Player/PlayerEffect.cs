using UnityEngine;

namespace Basics.Player
{
    public class PlayerEffect : MonoBehaviour
    {
        [SerializeField] private Animator effectsAnimator;
    
        private static readonly int Poof = Animator.StringToHash("Poof");
    
        private static readonly int Stun = Animator.StringToHash("Stun");

        public void PlayPuffAnimation() => effectsAnimator.SetTrigger(Poof);

        public void PlayStunAnimation() => effectsAnimator.SetTrigger(Stun);

        public float GetCurAnimationTime() => effectsAnimator.GetCurrentAnimatorStateInfo(0).length;
    }
}

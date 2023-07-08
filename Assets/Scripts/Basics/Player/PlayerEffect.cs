using Audio;
using FMODUnity;
using UnityEngine;

namespace Basics.Player
{
    public class PlayerEffect : MonoBehaviour
    {
        [SerializeField] private Animator effectsAnimator;
        [SerializeField] private EventReference _poofSound;
    
        private static readonly int Poof = Animator.StringToHash("Poof");
    
        private static readonly int Stun = Animator.StringToHash("Stun");

        public void PlayPuffAnimation()
        {
            AudioManager.PlayOneShot(_poofSound);
            effectsAnimator.SetTrigger(Poof);
        }

        public void PlayStunAnimation() => effectsAnimator.SetBool(Stun, true);

        public void StopStunAnimation() => effectsAnimator.SetBool(Stun, false);

        public float GetCurAnimationTime() => effectsAnimator.GetCurrentAnimatorStateInfo(0).length;

        public void ResetPoofTrigger() => effectsAnimator.ResetTrigger(Poof);
    }
}

using System;
using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.Events;
using Basics.Player;
using FMODUnity;
using Managers;
using Utilities;


namespace GameMode.Juggernaut
{   
  
    public class Totem : MonoBehaviour
    {

        [SerializeField] private Animator totemAnimator;

        [SerializeField] private PlayerEffect effect;
        [SerializeField] private EventReference _appearSound;

        private bool _canPickUp = false;

        private float _time = 0f;
        
        private Guid _appearInvoke = Guid.Empty;

        private SpriteRenderer _spriteRenderer;

        private readonly Color _disabledColor = new Color(1, 1, 1, 0);
        public UnityAction<PlayerController> OnTotemPickedUp { set; get; }

        public float coolDownTime = 2f;
        
        private static readonly int TotemEnabled = Animator.StringToHash("TotemEnabled");

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (isActiveAndEnabled && GameManager.Instance.State == GameState.Playing && !_canPickUp)
            {   
               _time += Time.deltaTime;
                if (_time >= coolDownTime)
                {   
                    _time = 0f;
                    AudioManager.PlayOneShot(_appearSound);
                    effect.PlayPuffAnimation();
                    if (_appearInvoke != Guid.Empty)
                        TimeManager.Instance.CancelInvoke(_appearInvoke);
                    _appearInvoke = TimeManager.Instance.DelayInvoke(() => { 
                            _spriteRenderer.color = Color.white;
                            _appearInvoke = Guid.Empty;
                            totemAnimator.SetBool(TotemEnabled, true);
                            
                        },
                        0.26f);
                        // effect.GetCurAnimationTime() * 0.5f);
                    _canPickUp = true;
                }
            }
        }

        private void OnDestroy()
        {
            if (_appearInvoke != Guid.Empty)
                TimeManager.Instance.CancelInvoke(_appearInvoke);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && _canPickUp)
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
            
            else 
                Debug.Log("can't pick up totem");
        }
        private void OnDisable()
        {
            _canPickUp = false;
            _time = 0;
            _spriteRenderer.color = _disabledColor;
            totemAnimator.SetBool(TotemEnabled, false);
        }
    }
}

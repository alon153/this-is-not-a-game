using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Basics.Player;
using Managers;
using Utilities;


namespace GameMode.Juggernaut
{   
  
    public class Totem : MonoBehaviour
    {

        [SerializeField] private float maxSaturation;

        [SerializeField] private float timeToSaturate = 1f;

        [SerializeField] private Animator totemAnimator;

        [SerializeField] private Color saturatedColor;

        private bool _canPickUp = false;

        private float _time = 0f;

        private SpriteRenderer _spriteRenderer;

        private Sprite _disabledSprite;

        private Vector3 _hsvColor = new Vector3();
        
        public UnityAction<PlayerController> OnTotemPickedUp { set; get; }

        public float coolDownTime = 2f;
        private static readonly int TotemEnabled = Animator.StringToHash("TotemEnabled");

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            Color.RGBToHSV(_spriteRenderer.color, out _hsvColor.x, out _hsvColor.y, out _hsvColor.z);
            _disabledSprite = _spriteRenderer.sprite;
        }

        private void Update()
        {
            if (isActiveAndEnabled && GameManager.Instance.State == GameState.Playing)
            {   
               _time += Time.deltaTime;
                if (_time >= coolDownTime)
                {
                    StartCoroutine(SetTotemSaturation());
                    _time = 0;
                }
            }
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
            _spriteRenderer.color = Color.white;
            totemAnimator.SetBool(TotemEnabled, false);
            _spriteRenderer.sprite = _disabledSprite;

        }

        private IEnumerator SetTotemSaturation()
        {
            var time = 0f;
            Color curColor;

            while (time < timeToSaturate)
            {
                time += Time.deltaTime;
                var curSat = Mathf.Lerp(0, maxSaturation, time / timeToSaturate);
                curColor = Color.HSVToRGB(_hsvColor.x, curSat, _hsvColor.z);
                _spriteRenderer.color = curColor;
                yield return null;
            }
            
            curColor = Color.HSVToRGB(_hsvColor.x, maxSaturation, _hsvColor.z);
            _spriteRenderer.color = curColor;
            totemAnimator.SetBool(TotemEnabled, true);
            _canPickUp = true;
        }
        
        
    }
}

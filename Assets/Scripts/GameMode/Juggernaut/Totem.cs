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

        public float coolDownTime = 2f;
        
        private bool _canPickUp = false;

        private float _time = 0f;
        
        private readonly Color _pickUpEnabledColor = Color.yellow;

        private Color _pickUpDisabledColor;

        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _pickUpDisabledColor = _spriteRenderer.color;
        }

        private void Update()
        {
            if (isActiveAndEnabled)
            {   
               _time += Time.deltaTime;
                if (_time >= coolDownTime)
                {    
                    
                    _spriteRenderer.color = _pickUpEnabledColor;
                    _canPickUp = true;
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
            _spriteRenderer.color = _pickUpDisabledColor;
        }
    }
}

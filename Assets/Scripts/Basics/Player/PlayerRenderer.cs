using System;
using UnityEngine;
using Utilities;

namespace Basics.Player
{
    [Serializable]
    public class PlayerRenderer
    {
        [field: SerializeField] public SpriteRenderer Regular { get; private set; }
        [SerializeField] private float _bloomIntensity = 2;
        [SerializeField] private Material _playerMaterialOrigin;
        [SerializeField] private Material _defaultMaterial;
        private Material _playerMaterial;

        public Animator Animator
        {
            get
            {
                if (_animator == null)
                    _animator = Regular.gameObject.GetComponent<Animator>();
                return _animator;
            }
            set => _animator = value;
        }

        private Animator _animator;
        
        private static readonly int ColorFactor = Shader.PropertyToID("_ColorFactor");
        private static readonly int Color1 = Shader.PropertyToID("_OutlineColor");
        private static readonly int Thickness = Shader.PropertyToID("_Thickness");

        public Sprite RegularSprite
        {
            get => Regular.sprite;
            set => Regular.sprite = value;
        }

        public void ToggleOutline(bool showOutline)
        {
            Regular.material = showOutline ? _playerMaterial : _defaultMaterial;
        }

        public Color RegularColor
        {
            get => Regular.color;
            set => Regular.color = value;
        }

        public void SetActive(bool active)
        {
            Regular.gameObject.SetActive(active);
        }

        public void Init(Color playerColor)
        {
            _playerMaterial = new Material(_playerMaterialOrigin);
            _playerMaterial.SetColor(Color1, playerColor);
            Regular.material = _playerMaterial;
        }

        public void SetAnimatorOverride(AnimatorOverrideController newController)
        {
            _animator.runtimeAnimatorController = newController;
        }
    }
}
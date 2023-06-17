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
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int Thickness = Shader.PropertyToID("_Thickness");

        public Material OutlineMaterial
        {
            get => Regular.material;
            set => Regular.material = value;
        }

        public Sprite RegularSprite
        {
            get => Regular.sprite;
            set => Regular.sprite = value;
        }

        public void ToggleOutline(bool showOutline)
        {
            OutlineMaterial.SetFloat(ColorFactor, showOutline ? 1 : 0);
        }

        public void SetGlobalColor(Color c)
        {
            Regular.material.SetColor(Color1,c.Intensify(_bloomIntensity));
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
    }
}
using System;
using UnityEngine;
using Utilities;

namespace Basics.Player
{
    [Serializable]
    public class PlayerRenderer
    {
        [field: SerializeField] public SpriteRenderer Bloomed { get; private set; }
        [field: SerializeField] public SpriteRenderer Regular { get; private set; }
        [SerializeField] private float _bloomIntensity;
        
        private static readonly int ColorFactor = Shader.PropertyToID("_ColorFactor");
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        public Material BloomMaterial
        {
            get => Bloomed.material;
            set => Bloomed.material = value;
        }

        public Material RegularMaterial
        {
            get => Regular.material;
            set => Regular.material = value;
        }

        public Sprite BloomedSprite
        {
            get => Bloomed.sprite;
            set => Bloomed.sprite = value;
        }
        
        public Sprite RegularSprite
        {
            get => Regular.sprite;
            set => Regular.sprite = value;
        }

        public void ToggleBloom(bool bloomOn)
        {
            BloomMaterial.SetFloat(ColorFactor, bloomOn ? 1 : 0.2f);
        }

        public void SetGlobalColor(Color c)
        {
            Bloomed.material.SetColor(Color1,c.Intensify(_bloomIntensity));
            Regular.color = c;
        }

        public Color BloomedColor
        {
            get => Bloomed.material.GetColor(Color1);
            set => Bloomed.material.SetColor(Color1,value.Intensify(_bloomIntensity));
        }

        public Color RegularColor
        {
            get => Regular.color;
            set => Regular.color = value;
        }
    }
}
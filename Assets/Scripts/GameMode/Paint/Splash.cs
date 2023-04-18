using System;
using Managers;
using UnityEngine;

namespace GameMode.Modes
{
    public class Splash : MonoBehaviour
    {
        #region Serialized Fields
        
        public SpriteRenderer Renderer { get; private set; }

        #endregion

        private static int _count;
        private static int Count
        {
            get => _count;
            set
            {
                string state = _count > value ? "down" : "up";
                _count = value;
            }
        }

        #region Event Functions

        private void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            Count++;
        }

        private void Update()
        {
            if (!Renderer.isVisible)
            {
                Count--;
                Destroy(gameObject);
            }
        }

        #endregion
    }
}
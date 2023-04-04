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

        #region Event Functions

        private void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
        }

        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}
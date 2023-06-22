using System;
using UnityEngine;

namespace GameMode.Boats
{
  public class BoatsRoad : MonoBehaviour
  {
     #region Serialized Fields

     [SerializeField] private float roadSpeed = 2f;

     #endregion

     #region Non-Serialized Fields

     private Material _rendererMaterial;

     private float _offset;
     
     private static readonly int MainTex = Shader.PropertyToID("_MainTex");

     #endregion

     #region MonoBehaviour

     private void Start()
     {
         _rendererMaterial = GetComponent<SpriteRenderer>().material;
     }

     private void Update()
     {
         _offset -= (Time.deltaTime * roadSpeed) / 10f;
         _rendererMaterial.SetTextureOffset(MainTex, new Vector2(0, _offset));
     }

     #endregion
  }
}


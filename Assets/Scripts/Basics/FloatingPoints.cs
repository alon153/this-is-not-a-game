using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingPoints : MonoBehaviour
{
  #region Serialized Fields

  [SerializeField] private float _animationTime = 1.5f;
  [SerializeField] private float _height = 0.5f;

  #endregion

  #region Non-Serialized Fields

  private TextMeshPro _textMesh;
  private float _startY;
  private Vector3 _pos;

  #endregion

  #region Properties

  public Color Color
  {
    get => _textMesh.color;
    set => _textMesh.color = value;
  }

  public string Text
  {
    get => _textMesh.text;
    set => _textMesh.text = value;
  }

  #endregion

  #region Function Events

  private void Awake()
  {
    _textMesh = GetComponentInChildren<TextMeshPro>();
    _pos = transform.position;
    _startY = _pos.y;
  }

  #endregion

  #region Public Methods

  public void Float()
  {
    StartCoroutine(Float_Inner());
  }

  #endregion

  #region Private Methods
  
  private IEnumerator Float_Inner()
  {
    float time = 0;
    float t = 0;
    Color c = Color;
    c.a = 1;
    Color = c;
    yield return null;

    while (time < _animationTime)
    {
      t = time / _animationTime;
      
      c.a = 1 - t;
      Color = c;

      _pos.y = (1 - t) * _startY + t * (_startY + _height);
      transform.position = _pos;
      
      time += Time.deltaTime;
      yield return null;
    }
    
    Destroy(gameObject);
  }

  #endregion
}


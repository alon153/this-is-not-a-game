using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraScript : MonoBehaviour
{
  [SerializeField] private float _shakeSize = 4.8f;
  [SerializeField] private float _smallShakeDuration = 0.1f;
  [SerializeField] private float _smallShakeMagnitude = 0.1f;
  [SerializeField] private float _bigShakeDuration = 0.3f;
  [SerializeField] private float _bigShakeMagnitude = 0.2f;
  
  private Camera _camera;
  private float _origSize;
  private Vector3 _origPos;
  private float _size;

  private Coroutine _shakeCoroutine;

  private void Awake()
  {
    _camera = GetComponent<Camera>();
    _origSize = _camera.orthographicSize;
    _origPos = transform.position;
  }

  public void Shake(bool big=false)
  {
    if (_shakeCoroutine != null)
      StopCoroutine(_shakeCoroutine);
    _shakeCoroutine = StartCoroutine(Shake_Inner(big));
  }

  public void CancelShake()
  {
    var pos = transform.position;
    pos.x = 0;
    pos.y = 0;
    transform.position = pos;
    _camera.orthographicSize = _origSize;
    _shakeCoroutine = null;
  }

  private IEnumerator Shake_Inner(bool big)
  {
    _camera.orthographicSize = _shakeSize;
    yield return null;
    
    float time = 0;

    float duration = big ? _bigShakeDuration : _smallShakeDuration;
    float magnitude = big ? _bigShakeMagnitude : _smallShakeMagnitude;
    var pos = transform.position;
    
    while (time < duration)
    {
      time += Time.deltaTime;
      pos.x = Random.Range(-magnitude, magnitude);
      pos.y = Random.Range(-magnitude, magnitude);
      transform.position = pos;
      yield return null;
    }

    CancelShake();
  }
}


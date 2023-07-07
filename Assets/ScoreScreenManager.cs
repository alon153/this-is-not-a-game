using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using FMODUnity;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

public class ScoreScreenManager : MonoBehaviour
{
  #region Serialized Fields

  [SerializeField] private List<GameObject> _bars;
  [SerializeField] private List<TextMeshProUGUI> _txts;
  [SerializeField] private GameObject _buttons;
  [SerializeField] private float _singleDuration = 1f;
  
  [SerializeField] private EventReference _clickButton;
  [SerializeField] private EventReference _selectButton;
  
  #endregion
  #region Non-Serialized Fields

  private GameObject _currArena = null;
  
  private List<RectTransform> _rects = new List<RectTransform>();
  private List<bool> _expanding = new List<bool>();
  private List<bool> _moving = new List<bool>();
  private List<float> _ys = new List<float>();
  
  private int _maxScore;

  private float _origMinX;
  private float _origMaxX;

  private float _widthFactor=1;
  
  #endregion
  #region Properties

  public Queue<GameObject> Arenas { get; set; } = new Queue<GameObject>();
  public Queue<int> Scores { get; set; } = new Queue<int>();

  public int MaxScore
  {
    get => _maxScore;
    set
    {
      _maxScore = value;
      _widthFactor = (1 - (_origMaxX - _origMinX)) / _maxScore;
    }
  }
  
  #endregion
  #region Function Events

  private void Start()
  {
    AudioManager.SetMusic(MusicSounds.Lobby);
    GameManager.Instance.SetScoreManager(this);
  }

  #endregion
  #region Public Methods

  public void ShowBars()
  {
    for (int i = 0; i < GameManager.Instance.Players.Count; i++)
    {
      _txts[i].text = "0";
      _bars[i].SetActive(true);
      _rects.Add(_bars[i].GetComponent<RectTransform>());
      _expanding.Add(false);
      _moving.Add(false);
      _ys.Add(_rects[i].anchoredPosition.y);

      if (i == 0)
      {
        _origMinX = _rects[i].anchorMin.x;
        _origMaxX = _rects[i].anchorMax.x;
      }
    }
  }

  public void ShowNextArena()
  {
    if (Arenas.Count == 0)
    {
      _buttons.gameObject.SetActive(true);
      return;
    }
    
    for (int i = 0; i < _expanding.Count; i++)
    {
      _expanding[i] = true;
      _moving[i] = true;
    }
    UIManager.Instance.ToggleFlash(true);
    
    if(_currArena != null)
      Destroy(_currArena.gameObject);
    
    _currArena = Instantiate(Arenas.Dequeue());
    
    TimeManager.Instance.DelayInvoke((UpdateBars), 0.25f);
  }

  private void UpdateBars()
  {
    Dictionary<int,int> scores = new ();
    Dictionary<int,int> places = new ();
    UIManager.Instance.ToggleFlash(false);
    for (int i = 0; i < _rects.Count; i++)
    {
      float score = Scores.Dequeue();
      scores[i] = (int) score;
      _txts[i].text = $"{(int) score}";
      float to = (_origMaxX - _origMinX) + _widthFactor * score;
      StartCoroutine(Expand_Inner(i, to));
    }
    for (int i = 0; i < scores.Count; i++)
    {
      if (places.Count == 0) places[i] = 0;
      else
      {
        var keys = new List<int>(places.Keys);
        int place = places.Count;
        foreach (var k in keys)
        {
          if (scores[i] > scores[k])
          {
            if (places[k] < place)
              place = places[k];
            places[k]++;
          }
        }
        places[i] = place;
      }
    }
    for (int i = 0; i < places.Count; i++)
    {
      StartCoroutine(Move_Inner(i, places[i]));
    }
  }

  public IEnumerator Move_Inner(int i, int newPlace)
  {
    RectTransform rect = _rects[i];
    
    Vector2 anchorPos = rect.anchoredPosition;
    float from = anchorPos.y;

    float to = _ys[newPlace];
    float y = from;
    float time = 0; 
    
    while (time < _singleDuration)
    {
      time += Time.deltaTime;
      float t = Mathf.Min(time / _singleDuration,1);
      y = (1 - t) * from + t * to;

      anchorPos.y = y;

      rect.anchoredPosition = anchorPos;

      yield return null;
    }

    _moving[i] = false;
    CheckContinue();
  }
  
  public IEnumerator Expand_Inner(int i, float to)
  {
    RectTransform rect = _rects[i];
    float from = rect.anchorMax.x - rect.anchorMin.x;
    Vector2 anchorMax = rect.anchorMax;
    Vector2 anchorMin = rect.anchorMin;
    
    float width = from;
    float time = 0; 
    
    while (time < _singleDuration)
    {
      time += Time.deltaTime;
      float t = Mathf.Min(time / _singleDuration,1);
      width = (1 - t) * from + t * to;

      anchorMax.x = 0.5f + width / 2;
      anchorMin.x = 0.5f - width / 2;

      rect.anchorMax = anchorMax;
      rect.anchorMin = anchorMin;
      
      yield return null;
    }

    _expanding[i] = false;
    CheckContinue();
  }

  private void CheckContinue()
  {
    if (_expanding.All((b => !b)) && _moving.All((b => !b)))
    {
      TimeManager.Instance.DelayInvoke(ShowNextArena, 1);
    }
  }

  public void OnMainMenu()
  {
    DestroyManagers();
    SceneManager.LoadScene("Opening");
  }
  
  public void OnLobby()
  {
    DestroyManagers();
    SceneManager.LoadScene("Main");
  }
  
  #endregion
  #region Private Methods

  private void DestroyManagers()
  {
    Destroy(GameManager.Instance.gameObject);
    Destroy(UIManager.Instance.gameObject);
    Destroy(TimeManager.Instance.gameObject);
  }
  
  #endregion

  public void StartShow()
  {
    TimeManager.Instance.DelayInvoke(ShowNextArena, 1);
  }
  
  public void SelectSound()
  {
    AudioManager.PlayOneShot(_selectButton);
  }

  public void ClickSound()
  {
    AudioManager.PlayOneShot(_clickButton);
  }
}


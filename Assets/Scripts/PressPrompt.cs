using System;
using Basics;
using GameMode;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class PressPrompt : MonoBehaviour
{
  #region Serialized Fields

  [SerializeField] private Image _pressImage;
  [SerializeField] private Image _holdImage;

  [SerializeField] private Sprite _pressUp;
  [SerializeField] private Sprite _pressDown;
  [SerializeField] private Sprite _holdUp;
  [SerializeField] private Sprite _holdDown;

  #endregion

  #region Non-Serialized Fields

  private Sprite _up;
  private Sprite _down;
  private Image _currImage;
  private bool _pressed;
  private Guid _hideInvoke = Guid.Empty;
  
  #endregion

  #region Properties

  public GameModes Mode { get; set; }

  public bool Pressed
  {
    get => _pressed;
    set
    {
      _currImage.sprite = value ? _down : _up;
      _pressed = value;
    }
  }
  
  #endregion

  #region Function Events

  private void Awake()
  {
    HidePrompt();
  }

  #endregion

  #region Public Methods

  public void SetPrompt(InteractableObject interactableObject)
  {
    HidePrompt();
    if(interactableObject == null) return;
    
    bool hold = interactableObject.IsHold;
    _currImage = hold ? _holdImage : _pressImage;
    _up = hold ? _holdUp : _pressUp;
    _down = hold ? _holdDown : _pressDown;

    _currImage.sprite = _up;
  }

  public void HidePrompt()
  {
    _pressImage.gameObject.SetActive(false);
    _holdImage.gameObject.SetActive(false);
  }

  public void ShowPrompt()
  {
    _currImage.gameObject.SetActive(true);
  }
  
  public void Toggle(bool show)
  {
    if (show)
    {
      if (_hideInvoke != Guid.Empty)
        TimeManager.Instance.CancelInvoke(_hideInvoke);
      ShowPrompt();
    }
    else
    {
      _hideInvoke = TimeManager.Instance.DelayInvoke(
        () =>
        {
          HidePrompt();
          _hideInvoke = Guid.Empty;
        }, 
        Pressed ? 0.2f : 0);
    }
  }
  
  #endregion

  #region Private Methods

  #endregion
}


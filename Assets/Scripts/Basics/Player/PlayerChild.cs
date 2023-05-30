using System;
using Basics.Player;
using UnityEngine;

public class PlayerChild : MonoBehaviour
{
  [SerializeField] private PlayerController _player;

  public void AfterDeath()
  {
    _player.AfterDeathAnimation();
  }
}


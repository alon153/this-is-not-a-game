using Basics;
using System;
using UnityEngine;

namespace GameMode
{
  [Serializable]
  public abstract class GameModeBase
  {
      [field:SerializeField] public Arena ModeArena { get; private set;}
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }

    public abstract void InitRound();
    public abstract void InitArena();
    public abstract void ClearRound();
    public abstract void OnTimeOVer();
    
    
  }
}


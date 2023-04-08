using Basics;
using UnityEngine;

namespace GameMode
{
  public abstract class GameModeBase
  {
    [field:SerializeField] public Arena ModeArena { get; private set;} 
    
    public abstract void InitRound();
    public abstract void InitArena();
    public abstract void ClearRound();
    public abstract void OnTimeOVer();
    
    
  }
}


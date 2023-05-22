using Basics;
using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace GameMode
{
  [Serializable]
  public abstract class GameModeBase
  {
    #region Serialized Fields

    [field:SerializeField] public Arena ModeArena { get; private set;}
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }

    #endregion

    #region Abstract Functions

    protected abstract void InitRound_Inner();
    protected abstract void InitArena_Inner();
    protected abstract void ClearRound_Inner();
    protected abstract void OnTimeOver_Inner();
    protected virtual void EndRound_Inner() {}
    protected abstract Dictionary<int, float> CalculateScore_Inner();

    #endregion

    #region Public Methods

    public virtual void InitRound()
    {
      InitArena_Inner();
      InitRound_Inner();
    }
    public virtual void OnTimerOver()
    {
      OnTimeOver_Inner();
      EndRound();
    }
    
    public virtual void EndRound()
    {
      EndRound_Inner();
      // ScoreManager.Instance.SetPlayerScores(CalculateScore_Inner());
      ClearRound_Inner();
      GameManager.Instance.NextRound();
    }

    #endregion
  }
}


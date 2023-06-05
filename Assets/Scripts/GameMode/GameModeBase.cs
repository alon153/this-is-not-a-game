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
    [field: SerializeField] public Sprite InstructionsSprite { get; set; }
    [field: SerializeField] public List<Sprite> CharacterSprites { get; private set; } = new();

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

    public void ClearRound()
    {
      ClearRound_Inner();
    }

    public virtual void InitRound()
    {
      SetPlayerSprites();
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
      GameManager.Instance.NextRound();
    }

    #endregion

    #region Private Methods

    private void SetPlayerSprites()
    {
      var players = GameManager.Instance.Players;
      for (int i = 0; i < players.Count; i++)
      {
        if (CharacterSprites == null || CharacterSprites.Count < players.Count)
        {
          GameManager.Instance.SetDefaultSprite(players[i]);
        }
        else
        {
          players[i].Renderer.RegularSprite = CharacterSprites[i];
          players[i].Renderer.RegularColor = Color.white;
        }
      }
    }    

    #endregion
  }
}


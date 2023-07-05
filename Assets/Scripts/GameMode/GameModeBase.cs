using Basics;
using System;
using System.Collections.Generic;
using Audio;
using FMODUnity;
using Managers;
using Utilities;
using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode
{
  [Serializable]
  public abstract class GameModeBase
  {
    #region Serialized Fields
    
    [SerializeField] private GameModeObject sObj;
    [field:SerializeField] public Arena ModeArena { get; private set;}
    [field: SerializeField] public Sprite InstructionsSprite { get; set; }
    [field: SerializeField] public Sprite ChannelSprite { get; set; }
    [field: SerializeField] public List<Sprite> CharacterSprites { get; private set; } = new();
    [field: SerializeField] public List<AnimatorOverrideController> AnimatorOverride { get; private set; } = new();

    [field: SerializeField] public List<Sprite> CollisionParticlesSprites = new List<Sprite>();
    [field: SerializeField] public EventReference DashSound { get; private set; }
    [field: SerializeField] public MusicSounds Music { get; private set; } = MusicSounds.Lobby;

    #endregion

    #region Abstract Functions

    protected abstract void ExtractScriptableObject(GameModeObject input);
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
      ExtractScriptableObject(sObj);
      SetPlayerSprites();
      SetCollisionParticles();
      AudioManager.DashEvent = DashSound;
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
        if (AnimatorOverride == null || AnimatorOverride.Count < players.Count)
        {
          GameManager.Instance.SetDefaultAnimator(players[i]);
        }
        else
        {
          players[i].Renderer.Animator.runtimeAnimatorController = AnimatorOverride[i];
        }
        
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

    private void SetCollisionParticles()
    {
      var players = GameManager.Instance.Players;
      foreach (var player in players)
        player.SetCollisionParticles(CollisionParticlesSprites);
    }

    #endregion
  }
}


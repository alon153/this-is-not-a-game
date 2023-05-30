﻿using System;
using System.Collections.Generic;
using Basics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameMode.Rhythm
{
    [Serializable]
    public class RhythmMode : GameModeBase
    {
        private HashSet<RhythmPanel> _panels = new();

        protected override void InitRound_Inner() { }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            
            foreach (Transform arenaObjects in arena.transform)
            {
                RhythmPanel p = arenaObjects.GetComponent<RhythmPanel>();
                if (p != null)
                {
                    _panels.Add(p);
                    AudioManager.RegisterBeatListener(p);
                }
            }
        }

        protected override void ClearRound_Inner()
        {
            foreach (var p in _panels)
            {
                AudioManager.UnRegisterBeatListener(p);
                p.StopRings();
            }
        }

        protected override void OnTimeOver_Inner()
        {
            
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            return null;
        }
    }
}
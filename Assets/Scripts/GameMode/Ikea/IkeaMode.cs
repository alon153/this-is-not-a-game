using System;
using System.Collections.Generic;
using Basics;
using Basics.Player;
using Managers;
using ScriptableObjects.GameModes.Modes;
using UnityEngine;
using Utilities.Interfaces;
using Object = UnityEngine.Object;

namespace GameMode.Ikea
{
    [Serializable]
    public class IkeaMode : GameModeBase, IOnPushedListener
    {
        #region ScriptableObject Fields

        private List<IkeaPart> _partsPrefabs;
        private PartDispenser _dispenserPrefab;
        private float _pointsPerPart = 10;

        #endregion
        
        #region Non-Serialized Fields

        private IkeaPart[] parts;
        private PartDispenser[] dispensers;

        #endregion

        #region GameModeBase

        protected override void ExtractScriptableObject(GameModeObject input)
        {
            IkeaModeObject sObj = (IkeaModeObject) input;
            _partsPrefabs = sObj._partsPrefabs;
            _dispenserPrefab = sObj._dispenserPrefab;
            _pointsPerPart = sObj._pointsPerPart;
        }

        protected override void InitRound_Inner()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                player.Addon = new IkeaPlayerAddon();
                player.RegisterPushedListener(this);
            }
        }

        protected override void InitArena_Inner()
        {
            var arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            GameManager.Instance.CurrArena = arena;
        }

        protected override void ClearRound_Inner()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                player.Addon = null;
                player.UnRegisterPushedListener(this);
            }

            parts = (IkeaPart[]) Object.FindObjectsOfType (typeof(IkeaPart));
            dispensers = (PartDispenser[]) Object.FindObjectsOfType (typeof(PartDispenser));
            
            foreach (var part in parts)
            {
                Object.Destroy(part.gameObject);
            }
            
            foreach (var dispenser in dispensers)
            {
                Object.Destroy(dispenser.gameObject);
            }
        }

        protected override void OnTimeOver_Inner() { }

        protected override Dictionary<int,float> CalculateScore_Inner()
        {
            parts = (IkeaPart[]) Object.FindObjectsOfType (typeof(IkeaPart));
            dispensers = (PartDispenser[]) Object.FindObjectsOfType (typeof(PartDispenser));
            
            Dictionary<int, float> scores = new();
            foreach (var part in parts)
            {
                if(!part.IsInPlace) continue;

                int index = -1;
                for (int i = 0; i < GameManager.Instance.Players.Count; i++)
                {
                    if (GameManager.Instance.PlayerColor(i) == part.Color)
                    {
                        index = i;
                        break;
                    }
                }
                if (index == -1) continue;

                if (!scores.ContainsKey(index))
                    scores[index] = 0;
                
                scores[index]+=_pointsPerPart;
            }

            return scores;
        }

        #endregion

        #region IOnPushedListener        

        public void OnPushed(PlayerController pushed, PlayerController pusher)
        {
            PlayerAddon.CheckCompatability(pushed.Addon, GameModes.Ikea);

            IkeaPart playerPart = ((IkeaPlayerAddon) pushed.Addon).Part;
            
            if (playerPart == null)
                return;

            playerPart.Drop();
            ((IkeaPlayerAddon) pushed.Addon).Part = null;
        }
        
        #endregion
    }
}
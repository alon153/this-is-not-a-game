using System;
using System.Collections.Generic;
using Basics;
using Basics.Player;
using Managers;
using UnityEngine;
using Utilities.Interfaces;
using Object = UnityEngine.Object;

namespace GameMode.Ikea
{
    [Serializable]
    public class IkeaMode : GameModeBase, IOnPushedListener
    {
        #region Serialized Fields

        [SerializeField] private List<IkeaPart> _partsPrefabs;
        [SerializeField] private PartDispenser _dispenserPrefab;

        #endregion

        #region Non-Serialized Fields

        private IkeaPart[] parts;
        private PartDispenser[] dispensers;

        #endregion

        #region GameModeBase

        public override void InitRound()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                player.Addon = new IkeaPlayerAddon();
                player.RegisterPushedListener(this);
            }
            InitArena();
        }

        public override void InitArena()
        {
            var arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            var dispenserWidth = Vector3.right * _dispenserPrefab.transform.lossyScale.x;
            var dispenserPos = (_partsPrefabs.Count % 2 == 0)
                ? arena.TopMiddle - dispenserWidth / 2
                : arena.TopMiddle;
            var padding = new Vector3(0.2f,0,0);
            dispenserPos -= dispenserWidth * Mathf.Floor(_partsPrefabs.Count / 2) + padding * Mathf.Floor((_partsPrefabs.Count)/2 - 1);
            foreach (IkeaPart part in _partsPrefabs)
            {
                var dispenser = Object.Instantiate(_dispenserPrefab, dispenserPos, Quaternion.identity);
                dispenser.PartPrefab = part;

                var transform = dispenser.transform;
                var disPart = Object.Instantiate(part, transform.position, Quaternion.identity, transform);
                
                disPart.GetComponent<Collider2D>().enabled = false;
                IkeaPart.BlueprintCount--; //don't count the items on the dispensers as blueprints

                dispenserPos += dispenserWidth + padding;
            }

            GameManager.Instance.CurrArena = arena;
        }

        public override void ClearRound()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                player.Addon = null;
                player.UnRegisterPushedListener(this);
            }
            
            parts ??= (IkeaPart[]) Object.FindObjectsOfType(typeof(IkeaPart));
            dispensers ??= (PartDispenser[]) Object.FindObjectsOfType(typeof(PartDispenser));
            
            foreach (var part in parts)
            {
                Object.Destroy(part.gameObject);
            }
            
            foreach (var dispenser in dispensers)
            {
                Object.Destroy(dispenser.gameObject);
            }
        }

        public override void OnTimeOVer()
        {
            EndRound();
        }

        public override void EndRound()
        {
            parts = (IkeaPart[]) Object.FindObjectsOfType (typeof(IkeaPart));
            dispensers = (PartDispenser[]) Object.FindObjectsOfType (typeof(PartDispenser));

            ScoreManager.Instance.SetPlayerScores(CalculateScore());
            
            GameManager.Instance.ClearRound();
        }

        public override Dictionary<int,float> CalculateScore()
        {
            Dictionary<int, float> scores = new();
            foreach (var part in parts)
            {
                if(!part.IsInPlace) continue;
                
                int index = GameManager.Instance.PlayerColors.FindIndex((color => color == part.Color));
                if (index == -1) continue;

                if (!scores.ContainsKey(index))
                    scores[index] = 0;
                
                scores[index]++;
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
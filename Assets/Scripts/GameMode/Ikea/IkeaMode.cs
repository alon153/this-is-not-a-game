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
        [SerializeField] private float _pointsPerPart = 10;

        #endregion

        #region Non-Serialized Fields

        private IkeaPart[] parts;
        private PartDispenser[] dispensers;

        #endregion

        #region GameModeBase

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
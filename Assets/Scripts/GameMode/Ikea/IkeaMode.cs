using System;
using System.Collections.Generic;
using Basics;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameMode.Ikea
{
    [Serializable]
    public class IkeaMode : GameModeBase
    {
        #region Serialized Fields

        [SerializeField] private List<IkeaPart> _partsPrefabs;
        [SerializeField] private PartDispenser _dispenserPrefab;

        #endregion
        
        #region Non-Serialized Fields

        private GameObject _partsParent;
        
        #endregion

        public override void InitRound()
        {
            _partsParent = new GameObject();
            
            var arena = GameManager.Instance.Arena;
            var dispenserWidth = Vector3.right * _dispenserPrefab.transform.lossyScale.x;
            var dispenserPos = (_partsPrefabs.Count % 2 == 0)
                ? arena.TopMiddle
                : arena.TopMiddle - dispenserWidth / 2;
            dispenserPos -= (dispenserWidth * Mathf.Floor(_partsPrefabs.Count / 2));
            foreach (IkeaPart part in _partsPrefabs)
            {
                var dispenser = Object.Instantiate(_dispenserPrefab, dispenserPos, Quaternion.identity);
                dispenser.transform.SetParent(_partsParent.transform);
                dispenser.PartPrefab = part;

                dispenserPos += dispenserWidth;
            }

            foreach (var player in GameManager.Instance.Players)
            {
                player.Addon = new IkeaPlayerAddon();
            }
            
        }

        public override void ClearRound()
        {
            throw new NotImplementedException();
        }

        public override void OnTimeOVer()
        {
            throw new NotImplementedException();
        }
    }
}
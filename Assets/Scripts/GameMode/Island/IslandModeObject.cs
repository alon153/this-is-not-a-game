using System.Collections.Generic;
using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode.Island
{
    [CreateAssetMenu(fileName = "IslandModeObjects", menuName = "ScriptableObjects/GameModes/IslandModeObject")]
    public class IslandModeObject : GameModeObject
    {
        public int _numTreasures;
        public List<IslandMode.TreasureValue> _treasureValues;
        public List<IslandMode.DigTime> _digTimes;
        public Treasure _treasurePrefab;
        public float _vibrationRadius = 2;
        public float _vibrationMaxForce = 0.5f;
    }
}
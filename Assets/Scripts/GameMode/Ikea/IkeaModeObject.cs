using System.Collections.Generic;
using FMODUnity;
using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode.Ikea
{
    [CreateAssetMenu(fileName = "IkeaModeObjects", menuName = "ScriptableObjects/GameModes/IkeaModeObject")]
    public class IkeaModeObject : GameModeObject
    {
        public List<IkeaPart> _partsPrefabs;
        public PartDispenser _dispenserPrefab;
        public float _pointsPerPart = 10;
    }
}
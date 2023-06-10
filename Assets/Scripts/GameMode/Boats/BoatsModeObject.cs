using System.Collections.Generic;
using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode.Boats
{
    [CreateAssetMenu(fileName = "BoatsModeObjects", menuName = "ScriptableObjects/GameModes/BoatsModeObject")]
    public class BoatsModeObject : GameModeObject
    {
        public float _yOffset = 1;
        [Tooltip("Drag all the prefabs that are used as obstacles for this round.\n Can be found on " +
                 "Prefabs/Modes/Boats)")]
        public List<RiverObstacle> obstaclesPrefab = new List<RiverObstacle>();
        [Range(5, 15)]
        public int defaultObstaclesCapacity = 8;
        [Range(15, 20)]
        public int maxObstacleCapacity = 20;
        [Tooltip("Initial Interval, it will go lower as round time progress.")]
        public float maxSpawnInterval = 3f;
        [Tooltip("Lowest Boundary interval, it won't go lower than this.")]
        public float minSpawnInterval = 0.5f;
        [Range(2,7)]
        public int obstacleSpawnMultiplier = 3;
        public float score = 30f;
    }
}
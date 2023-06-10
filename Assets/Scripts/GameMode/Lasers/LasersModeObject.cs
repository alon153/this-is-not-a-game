using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode.Lasers
{
    [CreateAssetMenu(fileName = "LasersModeObjects", menuName = "ScriptableObjects/GameModes/LasersModeObject")]
    public class LasersModeObject : GameModeObject
    {
        [Header("\nLasers")] 
        [Tooltip("When hit by laser, how much time the player freezes?")] 
        public float freezeTime = 2;
        [Tooltip("When hit by laser, how much force is applied")] 
        public float laserKnockBackForce = 1.5f;
        [Header("\nDiamonds")]
        public DiamondCollectible[] diamondPrefabs;
        [Tooltip("How many (regular) diamonds will be spawned initially")]
        [Range(10, 30)] 
        public int diamondCount = 12;
        [Tooltip("check this box if you want more diamonds to continue spawning after all diamonds are collected")]
        public bool shouldContinueSpawn = true;
        [Tooltip("timer for new diamond to be summoned")] 
        public float timeToSpawnNewDiamond = 1;
        [Tooltip("How many diamonds will the player drop when hitting a laser?")] 
        public int diamondsDropOnLaser = 2;
        [Tooltip("in which radius from player will diamonds fall when it gets hit by laser.")]
        public float onHitSpreadRadius = 3f;
    }
}
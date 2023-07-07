using System.Collections.Generic;
using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode.Juggernaut
{
    [CreateAssetMenu(fileName = "JuggernautModeObjects", menuName = "ScriptableObjects/GameModes/JuggernautModeObject")]
    public class JuggernautModeObject : GameModeObject
    {
        [Header("Totem")]
        public Totem totemPrefab;
        public float totemDisableTime = 3f;
        public float totemDropRadius = 1.5f;

        [Header("\nPlayers")] 
        public List<AnimatorOverrideController> gorillaAnimatorOverride = new List<AnimatorOverrideController>();
        public Vector2 gorillaColliderSize;

        [Header("\nProjectile")]
        public Projectile projectilePrefab;
        [Tooltip("speed given to projectile while shooting")]
        public float projectileSpeed = 10f;
        public float projectileDestroyTime = 3f;
        public float shotCooldown = 0.5f;
        [Header("\nGameMode ui")]
        [Tooltip("how many hits can a player take before dropping the totem")]
        public int juggernautLives = 5;
        [Tooltip("How many points will be added per frame to the totem holder")]
        public float scorePerSecondHolding = 1f;
        public float timeToAddScore = 1f;
        public JuggerCanvasAddOn canvasAddOnPrefab;
        public GameObject lifePrefab;
       
    }
}
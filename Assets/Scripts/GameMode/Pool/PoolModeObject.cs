using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode.Pool
{
    [CreateAssetMenu(fileName = "PoolModeObjects", menuName = "ScriptableObjects/GameModes/PoolModeObject")]
    public class PoolModeObject : GameModeObject
    {
        public float scoreOnHit = 10f;
    }
}
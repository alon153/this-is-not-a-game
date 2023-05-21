using Basics;
using Managers;
using UnityEngine;
using UnityEngine.Pool;


namespace GameMode.Juggernaut
{
    public class JuggernautPlayerAddOn : PlayerAddon
    {
        #region Private Fields

        private ObjectPool<Projectile> _projectilePool;

        private float _coolDown;

        private bool _canShoot = true;

        private float _projectileDestroyTime;
        
        #endregion
        
        #region Properties
        public bool YieldsTotem { get; set; } = false;

        public float TotalTimeYieldingTotem { get; set; } = 0f;

        #endregion

        public JuggernautPlayerAddOn(ObjectPool<Projectile> projectilePool, float coolDown, float projectileDestroyTime)
        {
            _projectilePool = projectilePool;
            _coolDown = coolDown;
            _projectileDestroyTime = projectileDestroyTime;
        }

        public override GameModes GameMode()
        {
            return GameModes.Juggernaut;
        }
        
        /// <summary>
        /// shoot a projectile
        /// </summary>
        /// <param name="speed">
        /// projectile speed.
        /// </param>
        /// <param name="direction">
        /// projectile direction
        /// </param>
        /// <param name="position">
        /// player position 
        /// </param>
        public void Shoot(float speed, Vector2 direction, Vector3 position)
        {
            if (YieldsTotem) return;

            var projectile = _projectilePool.Get();
            projectile.gameObject.transform.position = position;
            projectile.RigidBody.velocity = speed * direction;
            _canShoot = false;
            TimeManager.Instance.DelayInvoke(() => _canShoot = true, _coolDown);
            TimeManager.Instance.DelayInvoke(() =>
            {
                if (projectile.isActiveAndEnabled)
                    _projectilePool.Release(projectile);
            }, _projectileDestroyTime);
        }
    }
}

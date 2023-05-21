using Basics;
using Basics.Player;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;


namespace GameMode.Juggernaut
{
    public class JuggernautPlayerAddOn : PlayerActionAddOn
    {
        #region Private Fields

        // shooting
        private readonly ObjectPool<Projectile> _projectilePool;

        private bool _canShoot = true;
        
        private readonly float _coolDown;

        private readonly float _projectileDestroyTime;

        private float _shotSpeed;
        
        // health
        private float _maxHealth;
        
        private float _curHealth;

        private readonly float _damagePerHit;

        public UnityAction OnTotemDropped;

        #endregion
        
        #region Properties
        public bool YieldsTotem { get; set; } = false;

        public float TotalTimeYieldingTotem { get; set; } = 0f;

        #endregion

        #region Constants

        private const int ZeroHealth = 0;
        

        #endregion

        public JuggernautPlayerAddOn(ObjectPool<Projectile> projectilePool, float coolDown, float shotSpeed, float projectileDestroyTime
            , float maxHealth, float damagePerHit, UnityAction totemDroppedAction)
        {
            _projectilePool = projectilePool;
            _coolDown = coolDown;
            _projectileDestroyTime = projectileDestroyTime;
            _maxHealth = maxHealth;
            _curHealth = maxHealth;
            _damagePerHit = damagePerHit;
            _shotSpeed = shotSpeed;
            OnTotemDropped += totemDroppedAction;
            
        }

        public override GameModes GameMode()
        {
            return GameModes.Juggernaut;
        }

        public override void OnAction(PlayerController player)
        {
            Shoot(_shotSpeed, player.Direction, player.transform.position);
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
        /// player's (shooter) position 
        /// </param>
        private void Shoot(float speed, Vector2 direction, Vector3 position)
        {
            if (YieldsTotem || !_canShoot) return;
           
            var projectile = _projectilePool.Get();
            projectile.gameObject.transform.position = position;
            Vector2 velocity = direction.Equals(Vector2.zero) ? Vector2.down : direction;
            velocity *= speed;
            projectile.rigidBody.velocity = velocity;
            _canShoot = false;
            TimeManager.Instance.DelayInvoke(() => _canShoot = true, _coolDown);
            TimeManager.Instance.DelayInvoke(() =>
            {
                if (projectile.isActiveAndEnabled)
                    _projectilePool.Release(projectile);
            }, _projectileDestroyTime);
        }
        
        
        /// <summary>
        /// called by projectile that hits the player.
        /// </summary>
        /// <param name="projectile">
        /// the projectile that has hit the player.
        /// </param>
        /// <param name="player">
        /// the player that was hit by the projectile (if the player is not the juggernaut, nothing will happen).
        /// </param>
        public void OnHit(Projectile projectile, PlayerController player)
        {
            if (!YieldsTotem) return;
            
            _projectilePool.Release(projectile);
            _curHealth -= _damagePerHit;

            if (_curHealth <= ZeroHealth)
            {
                _curHealth = _maxHealth;
                OnTotemDropped.Invoke();
            }


        }
    }
}

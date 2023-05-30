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

        private readonly float _shotSpeed;
        
        // health
        private readonly float _maxLives;
        
        private float _curLives;

        public UnityAction OnTotemDropped;
        
        // health ui
        private readonly JuggernautLifeGrid _juggernautLifeGrid; 
        
        private bool _yieldsTotem = false;

        #endregion
        
        #region Properties
       

        public float TotalTimeYieldingTotem { get; set; } = 0f;

        #endregion

        #region Constants

        private const int ZeroHealth = 0;
        

        #endregion

        public JuggernautPlayerAddOn(ObjectPool<Projectile> projectilePool, float coolDown, float shotSpeed,
            float projectileDestroyTime, float maxLives, UnityAction totemDroppedAction, 
            JuggernautLifeGrid playerLifeGrid)
        {
            _projectilePool = projectilePool;
            _coolDown = coolDown;
            _projectileDestroyTime = projectileDestroyTime;
            _maxLives = maxLives;
            _curLives = maxLives;
            _shotSpeed = shotSpeed;
            OnTotemDropped += totemDroppedAction;
            _juggernautLifeGrid = playerLifeGrid;
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
            if (_yieldsTotem || !_canShoot) return;
           
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
        
        private void ReduceHealth()
        {
            _curLives -= 1;
           
            _juggernautLifeGrid.EliminateLife();
            
            if (_curLives <= ZeroHealth)
                OnTotemDropped.Invoke();
            
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
            // the player isn't the juggernaut
            if (!_yieldsTotem) return;
            
            Debug.Log("player hit");
            // the player is juggernaut so we hot a hit.
            
            _projectilePool.Release(projectile);
            
            CheckCompatability(player.Addon, GameModes.Juggernaut);
            ((JuggernautPlayerAddOn) player.Addon).ReduceHealth();
        }



        public void RemoveTotemFromPlayer()
        {
            if (!_yieldsTotem)
            {
                Debug.LogWarning("player is not holding the totem but you tried to remove it from him.");
                return;
            }
            
            _juggernautLifeGrid.DisableAllLifeGrid();
            _yieldsTotem = false;
            _curLives = _maxLives;
        }

        public void AddTotemToPlayer()
        {
            _yieldsTotem = true;
            _juggernautLifeGrid.EnableLifeGrid();
        }
    }
}

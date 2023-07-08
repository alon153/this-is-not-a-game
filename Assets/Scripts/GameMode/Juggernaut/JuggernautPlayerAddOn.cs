using Audio;
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
        #region Public Fields
        
        public UnityAction OnTotemDropped;

        #endregion
        
        #region Private Fields

        // shooting
        private readonly ObjectPool<Projectile> _projectilePool;

        private bool _canShoot = true;
        
        private readonly float _coolDown;

        private readonly float _projectileDestroyTime;

        private readonly float _shotSpeed;

        private Vector2 _lastDir = Vector2.down;
        
        // health
        private readonly float _maxLives;
        
        private float _curLives;

        // health ui
        private readonly JuggerCanvasAddOn _juggernautCanvasAddOn; 
        
        private bool _yieldsTotem = false;

        #endregion
        
        #region Properties
        public bool YieldsTotem => _yieldsTotem;

        #endregion

        #region Constants
        
        private const int ZeroHealth = 0;
        
        #endregion

        public JuggernautPlayerAddOn(ObjectPool<Projectile> projectilePool, float coolDown, float shotSpeed,
            float projectileDestroyTime, float maxLives, UnityAction totemDroppedAction, 
            JuggerCanvasAddOn playerLifeGrid)
        {
            _projectilePool = projectilePool;
            _coolDown = coolDown;
            _projectileDestroyTime = projectileDestroyTime;
            _maxLives = maxLives;
            _curLives = maxLives;
            _shotSpeed = shotSpeed;
            OnTotemDropped += totemDroppedAction;
            _juggernautCanvasAddOn = playerLifeGrid;
        }

        public override GameModes GameMode()
        {
            return GameModes.Juggernaut;
        }

        public override void OnAction(PlayerController player)
        {
            if (!_canShoot) return;
            var projectile = Shoot(_shotSpeed, player.Direction, player.transform.position);
            projectile.SetProjectileColor(player.Color);
            
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
        private Projectile Shoot(float speed, Vector2 direction, Vector3 position)
        {
            if (_yieldsTotem || !_canShoot) return null;
           
            var projectile = _projectilePool.Get();
            projectile.gameObject.transform.position = position;
           
            Vector2 velocity = direction.Equals(Vector2.zero) ? _lastDir : direction;
            velocity *= speed;
            projectile.rigidBody.velocity = velocity;
            projectile.SetProjectileRotation(velocity);
            AudioManager.PlayOneShot(projectile._shootSound);
            _canShoot = false;
            
            TimeManager.Instance.DelayInvoke(() => _canShoot = true, _coolDown);
            TimeManager.Instance.DelayInvoke(() =>
            {
                if (projectile.isActiveAndEnabled)
                    _projectilePool.Release(projectile);
            }, _projectileDestroyTime);

            return projectile;
        }
        
        public void ReduceHealth(PlayerController player = null)
        {
            _curLives -= 1;
           
            _juggernautCanvasAddOn.EliminateLife();

            if (_curLives <= ZeroHealth)
            {
                OnTotemDropped.Invoke();
            }

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
            
            player.SetVibration(1, 0.3f);

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
            
            SetJuggerCanvas(JuggernautGameMode.PlayerState.Shooter);
            _yieldsTotem = false;
            _curLives = _maxLives;
            _canShoot = true;

        }

        public void AddTotemToPlayer()
        {   
            SetJuggerCanvas(JuggernautGameMode.PlayerState.Juggernaut);
            _yieldsTotem = true;
            _canShoot = false;

        }

        public void SetDir(Vector2 playerDir)
        {
            if (!playerDir.Equals(Vector2.zero))
            {
                _lastDir = playerDir;
                _juggernautCanvasAddOn.SetArrowDirection(playerDir);
            }
        }
        public void SetArrowColor(Color newColor) => _juggernautCanvasAddOn.SetArrowColor(newColor);
        
        public void SetJuggerCanvas(JuggernautGameMode.PlayerState state) =>
            _juggernautCanvasAddOn.SetAddOnCanvas(state);
    }
}

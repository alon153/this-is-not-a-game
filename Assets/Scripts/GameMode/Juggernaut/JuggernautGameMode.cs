using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Basics;
using Basics.Player;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;


namespace GameMode.Juggernaut
{   
    [Serializable]
    public class JuggernautGameMode : GameModeBase
    {   
        
        #region Serialized Fields  
        
        [SerializeField] private Totem totemPrefab; 
        
        [SerializeField] private Projectile projectilePrefab;

        [Tooltip("how many hits can a player take before dropping the totem")]
        [SerializeField] private int juggernautLives = 5;

        [Tooltip("speed given to projectile while shooting")]
        [SerializeField] private float projectileSpeed = 10f;

        [SerializeField] private float projectileDestroyTime = 3f;

        [SerializeField] private float totemDropRadius = 1.5f;

        [SerializeField] private float shotCooldown = 0.5f;

        [SerializeField] private int totalRoundScore = 100;

        [SerializeField] private JuggernautLifeGrid lifeGridPrefab;

        [SerializeField] private GameObject lifePrefab;
        
        #endregion

        #region Non-Serialized Fields
        
        // totem
        private Totem _totem;

        private bool _isAPlayerHoldingTotem = false;

        private PlayerController _currTotemHolder = null;
        
        // shooting
        private ObjectPool<Projectile> _projectilePool;      
      
        #endregion

        #region GameModeBase
        protected override void InitRound_Inner()
        {
            _isAPlayerHoldingTotem = false;
            GameManager.Instance.GameModeUpdateAction += JuggernautModeUpdate;
            
            _projectilePool = new ObjectPool<Projectile>(CreateProjectile, OnTakeProjectileFromPool,
                OnReturnProjectileToPool, OnDestroyProjectile, true);
           
            SetUpPlayerAddOn();
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            GameManager.Instance.CurrArena = arena;
            _totem = Object.Instantiate(totemPrefab, arena.Center, Quaternion.identity);
            _totem.OnTotemPickedUp += OnTotemPickedUp;
        }

        protected override void ClearRound_Inner()
        {
            Object.Destroy(_totem.gameObject);
            foreach (var player in GameManager.Instance.Players)
                player.Addon = null;
        }

        protected override void OnTimeOver_Inner()
        {
            GameManager.Instance.GameModeUpdateAction -= JuggernautModeUpdate;
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            Dictionary<int, float> scores = new Dictionary<int, float>();
            int roundLen = GameManager.Instance.GetRoundLength();
            foreach (var player in GameManager.Instance.Players)
            {   
                PlayerAddon.CheckCompatability(player.Addon, GameModes.Juggernaut);
                var timeWithTotem = ((JuggernautPlayerAddOn) player.Addon).TotalTimeYieldingTotem / roundLen;
                scores.Add(player.Index, timeWithTotem * totalRoundScore);    
            }

            return scores;
        }
        #endregion
        
        #region Private Methods

        private void SetUpPlayerAddOn()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                JuggernautLifeGrid newLifeGrid = null;
                
                // find the canvas object in the player
                foreach (Transform obj in player.transform)
                {
                    if (obj.gameObject.CompareTag("PlayerCanvas"))
                    {
                        newLifeGrid = Object.Instantiate(lifeGridPrefab, obj.transform, false);
                        newLifeGrid.SetLifeGrid(juggernautLives, lifePrefab);
                       
                        break;
                    }
                    
                }
                
                player.Addon = new JuggernautPlayerAddOn(_projectilePool, shotCooldown, projectileSpeed,
                    projectileDestroyTime, juggernautLives, OnTotemDropped, newLifeGrid);
            }
        }

        private void OnTotemPickedUp(PlayerController player)
        {
            _totem.gameObject.SetActive(false);
            _currTotemHolder = player;
            
            PlayerAddon.CheckCompatability(_currTotemHolder.Addon, GameModes.Juggernaut);
            ((JuggernautPlayerAddOn) _currTotemHolder.Addon).AddTotemToPlayer();
            _isAPlayerHoldingTotem = true;
            
        }

        private void OnTotemDropped()
        {
            _totem.gameObject.SetActive(true);
            
            // remove totem from current player
            PlayerAddon.CheckCompatability(_currTotemHolder.Addon, GameModes.Juggernaut);
            ((JuggernautPlayerAddOn) _currTotemHolder.Addon).RemoveTotemFromPlayer();
            _isAPlayerHoldingTotem = false;

            _totem.gameObject.transform.position = GenerateTotemPosition();
            
            _currTotemHolder = null;
        }

        private void JuggernautModeUpdate()
        {
            if (_isAPlayerHoldingTotem)
            {   
                PlayerAddon.CheckCompatability(_currTotemHolder.Addon, GameModes.Juggernaut);
                ((JuggernautPlayerAddOn) _currTotemHolder.Addon).TotalTimeYieldingTotem += Time.deltaTime;
            }
        }
        
        private Vector3 GenerateTotemPosition()
        {
            bool valid = false;
            var position = new Vector3();
            while (valid == false)
            {   
                // drop the totem in a random position around player that dropped it. 
                float angle = Random.Range(0f, Mathf.PI * 2f);
                position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 1) *
                           Random.Range(0f, totemDropRadius);

                valid = !ModeArena.OutOfArena(position);

            }
            return position;
        }

        #endregion
        
        #region Projectile Pooling
        private Projectile CreateProjectile()
        {
            var projectile = Object.Instantiate(projectilePrefab);
            return projectile;
        }

        private void OnTakeProjectileFromPool(Projectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        private void OnReturnProjectileToPool(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        private void OnDestroyProjectile(Projectile projectile)
        {
            Object.Destroy(projectile.gameObject);
        }

        #endregion
        
    }
}
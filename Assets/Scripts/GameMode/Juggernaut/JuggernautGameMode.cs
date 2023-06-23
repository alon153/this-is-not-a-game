using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Basics;
using Basics.Player;
using ScriptableObjects.GameModes.Modes;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Utilities.Interfaces;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;


namespace GameMode.Juggernaut
{   
    [Serializable]
    public class JuggernautGameMode : GameModeBase, IOnFallListener
    {   
        
        #region ScriptableObject Fields  
        
        private Totem totemPrefab;
        private float totemDisableTime = 3f;
        private float totemDropRadius = 1.5f;
        private Projectile projectilePrefab;
        private float projectileSpeed = 10f;
        private float projectileDestroyTime = 3f;
        private float shotCooldown = 0.5f;
        private int juggernautLives = 5;
        private float scorePerSecondHolding = 1f;
        private float timeToAddScore = 1f;
        private JuggerCanvasAddOn canvasAddOnPrefab;
        private GameObject lifePrefab;

        #endregion

        #region Non-Serialized Fields

        private float _time = 0f;
        
        // totem
        private Totem _totem;

        private bool _isAPlayerHoldingTotem = false;

        private PlayerController _currTotemHolder = null;
        
        // shooting
        private ObjectPool<Projectile> _projectilePool;

        private List<JuggerCanvasAddOn> _playerCanvasAddOns = new List<JuggerCanvasAddOn>();

        #endregion

        #region GameModeBase

        protected override void ExtractScriptableObject(GameModeObject input)
        {
            JuggernautModeObject sObj = (JuggernautModeObject) input;
            totemPrefab = sObj.totemPrefab;
            totemDisableTime = sObj.totemDisableTime;
            totemDropRadius = sObj.totemDropRadius;
            projectilePrefab = sObj.projectilePrefab;
            projectileSpeed = sObj.projectileSpeed;
            projectileDestroyTime = sObj.projectileDestroyTime;
            shotCooldown = sObj.shotCooldown;
            juggernautLives = sObj.juggernautLives;
            scorePerSecondHolding = sObj.scorePerSecondHolding;
            timeToAddScore = sObj.timeToAddScore;
            canvasAddOnPrefab = sObj.canvasAddOnPrefab;
            lifePrefab = sObj.lifePrefab;
        }

        protected override void InitRound_Inner()
        {
            _isAPlayerHoldingTotem = false;
            _playerCanvasAddOns.Clear();
            GameManager.Instance.GameModeUpdateAction += JuggernautModeUpdate;
            
            _projectilePool = new ObjectPool<Projectile>(CreateProjectile, OnTakeProjectileFromPool,
                OnReturnProjectileToPool, OnDestroyProjectile, true);
           
            SetUpPlayerAddOn();
            
            foreach (PlayerController player in GameManager.Instance.Players)
                player.RegisterFallListener(this);            
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            GameManager.Instance.CurrArena = arena;
            
            // totem instantiation.
            _totem = Object.Instantiate(totemPrefab, arena.Center, Quaternion.identity);
            _totem.OnTotemPickedUp += OnTotemPickedUp;
            _totem.coolDownTime = totemDisableTime;
        }

        protected override void ClearRound_Inner()
        {
            Object.Destroy(_totem.gameObject);
            for (int i = 0; i < _playerCanvasAddOns.Count; ++i)
            {
                Object.Destroy(_playerCanvasAddOns[i].gameObject);
                GameManager.Instance.Players[i].Addon = null;
            }
            
            foreach (PlayerController player in GameManager.Instance.Players)
                player.UnRegisterFallListener(this);              
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
               
            }

            return scores;
        }
        #endregion
        
        #region Private Methods

        private void SetUpPlayerAddOn()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                JuggerCanvasAddOn canvasAddOn = null;
                
                // find the canvas object in the player
                foreach (Transform obj in player.transform)
                {
                    if (obj.gameObject.CompareTag("PlayerCanvas"))
                    {   
                        canvasAddOn = Object.Instantiate(canvasAddOnPrefab, obj.transform, false);
                        //canvasAddOn.arrowColor = player.Color;
                        canvasAddOn.lifeObject = lifePrefab;
                        canvasAddOn.lives = juggernautLives;
                        _playerCanvasAddOns.Add(canvasAddOn);
                        break;
                    }
                    
                }
                
                player.Addon = new JuggernautPlayerAddOn(_projectilePool, shotCooldown, projectileSpeed,
                    projectileDestroyTime, juggernautLives, OnTotemDropped, canvasAddOn);
            }
        }

        private void OnTotemPickedUp(PlayerController player)
        {
            _totem.gameObject.SetActive(false);
            _currTotemHolder = player;
            
            PlayerAddon.CheckCompatability(_currTotemHolder.Addon, GameModes.Juggernaut);
            ((JuggernautPlayerAddOn) _currTotemHolder.Addon).AddTotemToPlayer();
            _isAPlayerHoldingTotem = true;
            _time = 0;

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
                _time += Time.deltaTime;
                if (_time >= timeToAddScore)
                {
                    _time = 0f; 
                    ScoreManager.Instance.SetPlayerScore(_currTotemHolder.Index, scorePerSecondHolding);
                }
            }

            foreach (var player in GameManager.Instance.Players)
            {
                PlayerAddon.CheckCompatability(player.Addon, GameModes.Juggernaut);
                JuggernautPlayerAddOn curAddon = (JuggernautPlayerAddOn) player.Addon;
                
                // player is a shooter so change it's direction 
                if (!curAddon.YieldsTotem)
                    curAddon.SetDir(player.Direction);
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
        
        public enum PlayerState
        {
            Shooter, Juggernaut 
        }

        public void OnFall(PlayerController playerFell)
        {   
            if (_isAPlayerHoldingTotem && _currTotemHolder.Index.Equals(playerFell.Index))
            {
                PlayerAddon.CheckCompatability(playerFell.Addon, GameModes.Juggernaut);
                ((JuggernautPlayerAddOn) playerFell.Addon).ReduceHealth();
            }
        }
    }
}

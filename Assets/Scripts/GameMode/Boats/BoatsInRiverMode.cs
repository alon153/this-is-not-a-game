using System;
using System.Collections.Generic;
using Basics;
using Basics.Player;
using FMODUnity;
using Managers;
using ScriptableObjects.GameModes.Modes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Utilities;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameMode.Boats
{   
    [Serializable]
    public class BoatsInRiverMode : GameModeBase
    {
        public override GameModes Mode => GameModes.Boats;
        
        // used to clear and freeze all obstacles still on screen when round ends. 
        public static ObjectPool<RiverObstacle> ObstaclesPool;

        #region ScriptableObject Fields

        private float _yOffset = 1;
        private List<RiverObstacle> obstaclesPrefab = new List<RiverObstacle>();
        private int defaultObstaclesCapacity = 8;
        private int maxObstacleCapacity = 20;
        private float maxSpawnInterval = 3f;
        private float minSpawnInterval = 0.5f;
        private int obstacleSpawnMultiplier = 3;
        private float score = 30f;
        private float dragUpSpeed = 0.01f;

        private EventReference _scream;

        #endregion

        #region Non-Serialized Fields

        private Vector3 _arenaMinCoord;
        
        private Vector3 _arenaMaxCoord;

        private float _curInterval;

        private float _timePassed;

        private float _timeProgress;

        private GameObject _obstaclesParent;

        private Vector2 _zeroVelocity = new Vector2(0, 0.01f);
        
        // has the round started? 
        private bool _started;

        // every time a player falls 
        private List<bool> _isInGame;

        private Dictionary<int, RiverObstacle> _obstacles = new Dictionary<int, RiverObstacle>();

        private Queue<Vector3> _spawnLocations = new Queue<Vector3>();
        
        private int _maxPrefabIndex;

        #endregion

        // Update is called once per frame
        private void Update()
        {
            if(GameManager.Instance.State != GameState.Playing) return;
            
            if (_started)
            {
                _timePassed += Time.deltaTime;
                if (_timePassed >= _curInterval)
                {   
                    //new (smaller) value is stored in '_curInterval' and new obstacles are spawned to the arena
                    //everytime the current interval has ended.
                    // So the logic here is that as the round progress more obstacles are spawned and more frequently.
                    _timePassed = 0f;
                    _timeProgress = (TimeManager.Instance.RoundDuration - TimeManager.Instance.TimeLeft)
                                    / TimeManager.Instance.RoundDuration;
                    SpawnNewObstacles();
                    _curInterval = CalcNextInterval();
                }
        
                foreach (var player in GameManager.Instance.Players)
                    if (!player.Frozen)
                        DragPlayerUp(player);
            }
        }

        private void DragPlayerUp(PlayerController playerController)
        {
            var velocity = playerController.Rigidbody.velocity;
            if (velocity.x is >= -0.1f and <= 0.1f && velocity.y is <= 0.1f and >= -0.1f)
            {
                var transform = playerController.transform;
                var draggingPosition = transform.position;
                draggingPosition.y += dragUpSpeed;
                transform.position = draggingPosition;
            }
        }

        #region GameModeBase Methods

        protected override void ExtractScriptableObject(GameModeObject input)
        {
            BoatsModeObject sObj = (BoatsModeObject) input;
            _yOffset = sObj._yOffset;
            obstaclesPrefab = sObj.obstaclesPrefab;
            defaultObstaclesCapacity = sObj.defaultObstaclesCapacity;
            maxObstacleCapacity = sObj.maxObstacleCapacity;
            maxSpawnInterval = sObj.maxSpawnInterval;
            minSpawnInterval = sObj.minSpawnInterval;
            obstacleSpawnMultiplier = sObj.obstacleSpawnMultiplier;
            score = sObj.score;
            dragUpSpeed = sObj.dragUpPlayerSpeed;
        }

        protected override void InitRound_Inner()
        {   
            // set up player and round logic.
            GameManager.Instance.GameModeUpdateAction += Update;
            _curInterval = maxSpawnInterval;
            GameManager.Instance.CurrArena.OnPlayerDisqualified += DisqualifyPlayer; 
            _isInGame = new List<bool>();
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                _isInGame.Add(true);
            }
            
            // set up obstacles pooling
            _maxPrefabIndex = obstaclesPrefab.Count;
            ObstaclesPool = new ObjectPool<RiverObstacle>(CreateObstacle,OnTakeObjectFromPool,
                 OnReturnObstacleToPool, OnDestroyObstacle, false, defaultObstaclesCapacity,
                 maxObstacleCapacity);

            _started = true;
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
       
             foreach (Transform child in arena.transform)
             {
                 if (child.CompareTag("WaterObstacle"))
                     _obstaclesParent = child.gameObject;
             } 
             GameManager.Instance.CurrArena = arena;
            _arenaMaxCoord = GameManager.Instance.CurrArena.BottomRight;
            _arenaMinCoord = GameManager.Instance.CurrArena.BottomLeft;
            
        }

        protected override void ClearRound_Inner()
        {
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (!_isInGame[i])
                    GameManager.Instance.Players[i].Respawn();
            }
            
            _obstacles.Clear();
            ObstaclesPool.Clear();
            GameManager.Instance.GameModeUpdateAction -= Update;
            GameManager.Instance.CurrArena.OnPlayerDisqualified -= DisqualifyPlayer; 
        }

        protected override void OnTimeOver_Inner()
        {
            
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            Dictionary<int, float> scoreForPlayers = new Dictionary<int, float>();
            
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (_isInGame[i])
                    scoreForPlayers[i] = score;
                else
                    scoreForPlayers[i] = 0;
            }

            return scoreForPlayers;
        }

        protected override void EndRound_Inner()
        {
            FreezeAllObstacles();
            GameManager.Instance.FreezePlayers(timed: false);
            ScoreManager.Instance.SetPlayerScores(CalculateScore_Inner());
        }

        #endregion

        /// <returns>
        /// The next interval to spawn new obstacles (becomes smaller as round progress). 
        /// </returns>
        private float CalcNextInterval()
        {
            float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, _timeProgress);
            return interval;
        }
        
        /// <summary>
        /// calculates the amount of objects to spawn as round progress.
        /// if the spawn amount makes the active obstacles to surpass the maximum of the object pool it normalizes
        /// the count. 
        /// </summary>
        /// <returns>how many obstacles should be spawned in this interval</returns>
        private int CalcSpawnAmount(float roundProgress)
        {
            int spawnAmount = Mathf.CeilToInt(roundProgress * obstacleSpawnMultiplier);
            if (spawnAmount + ObstaclesPool.CountAll > maxObstacleCapacity)
                spawnAmount = maxObstacleCapacity - ObstaclesPool.CountAll;
            return spawnAmount;
        }

        /// <summary>
        /// Method is called whenever an interval has ended on Update().
        /// Calculates how much objects needs to spawned this time then makes a list of random locations to spawn
        /// the obstacles and instantiates it (more objects as round progress). 
        /// </summary>
        private void SpawnNewObstacles()
        {   
            // calc amount of objects to spawn in this round
            float roundProgress = Mathf.Lerp(Constants.MinProgress, Constants.MaxProgress, _timeProgress);
            int spawnAmount = CalcSpawnAmount(roundProgress);

            for (int i = 0; i < spawnAmount; i++)
                ObstaclesPool.Get();
        }

        private bool AllPlayersFell()
        {
            foreach (var player in _isInGame)
            {
                if (player)
                    return false;
            }
            return true;
        }

        private void FreezeAllObstacles()
        {
            foreach (var obstacle in _obstacles)
            {
                obstacle.Value.FreezeObstacle();
                // This obstacle is no longer in game mode. 
                obstacle.Value.IsInMode = false;
            }
        }
        
        /// <summary>
        /// Gets a playerId and gets it out of the game.
        /// called as an event from the BoatsArena when player is caught on the arena's OnTriggerExit.
        /// If all players fell it stops the round. 
        /// </summary>
        /// <param name="playerId">
        /// unique playerId, will be given from arena based on GetInstanceId() of player fell. 
        /// </param>
        private void DisqualifyPlayer(int playerId)
        {

            // find the player that fell
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {   
                //player detected
                if (GameManager.Instance.Players[i].GetInstanceID() == playerId)
                {
                    _isInGame[i] = false;
                    break;
                }
            }

            if (AllPlayersFell())
                GameManager.Instance.EndRound();
        }
        
        #region Object Pooling methods
        
        /// <summary>
        /// used as a new pool item creation method for the pooling object. 
        /// </summary>
        private RiverObstacle CreateObstacle()
        {
            int idx = Random.Range(Constants.MinIndex, _maxPrefabIndex);
            var newObstacle = Object.Instantiate(obstaclesPrefab[idx], 
                _obstaclesParent.transform, true);
            _obstacles.Add(newObstacle.GetInstanceID(), newObstacle);
            return newObstacle;
        }

        private void OnTakeObjectFromPool(RiverObstacle obstacle)
        {
            if (obstacle._fadeCoroutine != null)
                obstacle.StopFadeCoroutine();
            // set the location of the object
            // obstacle.transform.position = _spawnLocations.Dequeue();
            var spawnYCor = _arenaMaxCoord.y - _yOffset;
            var spawnZCor = _arenaMaxCoord.z;
            obstacle.transform.position =
                GameManager.Instance.CurrArena.GetRespawnPosition(obstacle.gameObject, false, 
                    specY: true, givenY: spawnYCor, 
                    specZ: true, givenZ: spawnZCor);
            obstacle.gameObject.SetActive(true);
        }

        private void OnReturnObstacleToPool(RiverObstacle obstacle)
        {
            obstacle.DeactivateObstacleOnRound();
        }

        private void OnDestroyObstacle(RiverObstacle obstacle)
        {
            obstacle.IsInMode = false;
            _obstacles.Remove(obstacle.GetInstanceID());
            Object.Destroy(obstacle.gameObject);
        }
        
        #endregion
    }
}

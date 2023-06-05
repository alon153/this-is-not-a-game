using System;
using System.Collections.Generic;
using Basics;
using Managers;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameMode.Boats
{   
    [Serializable]
    public class BoatsInRiverMode : GameModeBase
    {
        #region Serialized Fields

        [SerializeField] private float _yOffset = 1;
        
        [Tooltip("Drag all the prefabs that are used as obstacles for this round.\n Can be found on " +
                 "Prefabs/Modes/Boats)")]
        [SerializeField] private List<RiverObstacle> obstaclesPrefab = new List<RiverObstacle>();
        
        [Range(5, 15)]
        [SerializeField] private int defaultObstaclesCapacity = 8;
        
        [Range(15, 20)]
        [SerializeField] private int maxObstacleCapacity = 20;
        
        [Tooltip("Initial Interval, it will go lower as round time progress.")]
        [SerializeField] private float maxSpawnInterval = 3f;
        
        [Tooltip("Lowest Boundary interval, it won't go lower than this.")]
        [SerializeField] private float minSpawnInterval = 0.5f;
        
        [Range(2,7)]
        [SerializeField] private int obstacleSpawnMultiplier = 3;

        [SerializeField] private float score = 30f;

        #endregion
        
        // used to clear and freeze all obstacles still on screen when round ends. 
        public static ObjectPool<RiverObstacle> ObstaclesPool;

        #region Non-Serialized Fields

        private Vector3 _arenaMinCoord;
        
        private Vector3 _arenaMaxCoord;

        private float _curInterval;

        private float _timePassed;

        private float _timeProgress;

        private GameObject _obstaclesParent;
        
        // has the round started? 
        private bool _started;

        // initial player positions for the round.
        private List<Vector3> _playerPositions = new List<Vector3>();
        
        // every time a player falls 
        private List<bool> _isInGame;

        private Dictionary<int, RiverObstacle> _obstacles = new Dictionary<int, RiverObstacle>();

        private Queue<Vector3> _spawnLocations = new Queue<Vector3>();
        
        private int _maxPrefabIndex;

        #endregion

        #region Constants

        private const int MaxProgress = 1;

        private const int MinProgress = 0;

        private const int MinPrefabIndex = 0;
        
        #endregion

        // Update is called once per frame
        private void Update()
        {
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
            }
        }

        #region GameModeBase Methods

        protected override void InitRound_Inner()
        {   
            // set up player and round logic.
            GameManager.Instance.GameModeUpdateAction += Update;
            _curInterval = maxSpawnInterval;
            GameManager.Instance.CurrArena.OnPlayerDisqualified += DisqualifyPlayer; 
            _isInGame = new List<bool>();
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                // GameManager.Instance.Players[i].transform.position = _playerPositions[i];
                _isInGame.Add(true);
            }
            
            // set up obstacles pooling
            _maxPrefabIndex = obstaclesPrefab.Count;
            ObstaclesPool = new ObjectPool<RiverObstacle>(CreateObstacle,OnTakeObjectFromPool,
                 OnReturnObstacleToPool, OnDestroyObstacle, true, defaultObstaclesCapacity,
                 maxObstacleCapacity);

            _started = true;
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
             _playerPositions.Clear();
             foreach (Transform child in arena.transform){
                if (child.CompareTag("spawnLocation"))
                    _playerPositions.Add(child.position);
                if (child.CompareTag("WaterObstacle"))
                    _obstaclesParent = child.gameObject;
             } 
             GameManager.Instance.CurrArena = arena;
            _arenaMaxCoord = GameManager.Instance.CurrArena.TopRight;
            _arenaMinCoord = GameManager.Instance.CurrArena.TopLeft;
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
            float roundProgress = Mathf.Lerp(MinProgress, MaxProgress, _timeProgress);
            int spawnAmount = CalcSpawnAmount(roundProgress);

            HashSet<Vector3> spawnSet = new HashSet<Vector3>();

            var spawnYCor = _arenaMaxCoord.y + _yOffset;
            var spawnZCor = _arenaMaxCoord.z;

            // calc spawn locations
            while (spawnSet.Count < spawnAmount)
            {
                float spawnXCor = Random.Range(_arenaMinCoord.x, _arenaMaxCoord.x);
                spawnSet.Add(new Vector3(spawnXCor, spawnYCor, spawnZCor));
            }

            _spawnLocations = new Queue<Vector3>(spawnSet);
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
            int idx = Random.Range(MinPrefabIndex, _maxPrefabIndex);
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
            obstacle.transform.position = _spawnLocations.Dequeue();
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

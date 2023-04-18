using System;
using System.Collections.Generic;
using Basics;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameMode.Boats
{   
    [Serializable]
    public class BoatsInRiverMode : GameModeBase
    {
        #region Serialized Fields
        
        [Tooltip("Drag all the prefabs that are used as obstacles for this round.\n Can be found on " +
                 "Prefabs/Modes/Boats)")]
        [SerializeField] private List<GameObject> obstaclesPrefab = new List<GameObject>();
        
        [Tooltip("Initial Interval, it will go lower as round time progress.")]
        [SerializeField] private float maxSpawnInterval = 3f;
        
        [Tooltip("Lowest Boundary interval, it won't go lower than this.")]
        [SerializeField] private float minSpawnInterval = 0.5f;

        [SerializeField] private float obstacleSpawnMultiplier = 10f;

        [SerializeField] private float score = 30f;

        #endregion

        #region Non-Serialized Fields

        private Vector3 _arenaMinCoord;
        
        private Vector3 _arenaMaxCoord;

        private float _curInterval = 0f;

        private float _timePassed = 0f;

        private float _timeProgress = 0f;
        
        // has the round started? 
        private bool _started = false;
        
        // initial player positions for the round.
        private List<Vector3> _playerPositions = new List<Vector3>();
        
        // every time a player falls 
        private List<bool> _isInGame;
        
        // used to clear and freeze all obstacles still on screen when round ends. 
        private RiverObstacle[] _obstaclesInGame;

        #endregion

        #region Constants
        
        private const int MaxProgress = 1;

        private const int MinProgress = 0;

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
            GameManager.Instance.GameModeUpdateAction += Update;
            _curInterval = maxSpawnInterval;
            GameManager.Instance.CurrArena.OnPlayerDisqualified += DisqualifyPlayer; 
            _isInGame = new List<bool>();
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                GameManager.Instance.Players[i].transform.position = _playerPositions[i];
                _isInGame.Add(true);
            }

            _started = true;
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
             _playerPositions.Clear();
             foreach (Transform child in arena.transform){
                if (child.CompareTag("spawnLocation"))
                    _playerPositions.Add(child.position);
             } 
             GameManager.Instance.CurrArena = arena;
            _arenaMaxCoord = GameManager.Instance.CurrArena.TopRight;
            _arenaMinCoord = GameManager.Instance.CurrArena.TopLeft;
        }

        protected override void ClearRound_Inner()
        {
            for (int i = 0; i < _obstaclesInGame.Length; i++)
                Object.Destroy(_obstaclesInGame[i].gameObject);

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
        /// Method is called whenever an interval has ended on Update().
        /// Calculates how much objects needs to spawned this time then makes a list of random locations to spawn
        /// the obstacles and instantiates it (more objects as round progress). 
        /// </summary>
        private void SpawnNewObstacles()
        {   
            // calc amount of objects to spawn in this round
            float roundProgress = Mathf.Lerp(MinProgress, MaxProgress, _timeProgress);
            int spawnAmount = Mathf.CeilToInt(roundProgress * obstacleSpawnMultiplier);

            HashSet<Vector3> spawnSet = new HashSet<Vector3>();

            var spawnYCor = _arenaMaxCoord.y;
            var spawnZCor = _arenaMaxCoord.z;

            // calc spawn locations
            while (spawnSet.Count < spawnAmount)
            {
                float spawnXCor = Random.Range(_arenaMinCoord.x, _arenaMaxCoord.x);
                spawnSet.Add(new Vector3(spawnXCor, spawnYCor, spawnZCor));
            }
            List<Vector3> spawnLocations = new List<Vector3>(spawnSet);
            
            
            // finally spawn the new objects
            foreach (var pos in spawnLocations)
            {
                int obstacleIdx = Random.Range(0, obstaclesPrefab.Count);
                GameObject obstacle =
                    Object.Instantiate(obstaclesPrefab[obstacleIdx], pos, Quaternion.identity);
            }
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
            RiverObstacle[] obstaclesInGame = Object.FindObjectsOfType<RiverObstacle>();
            for (int i = 0; i < obstaclesInGame.Length; i++)
                obstaclesInGame[i].ObstacleRigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
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
    }
}

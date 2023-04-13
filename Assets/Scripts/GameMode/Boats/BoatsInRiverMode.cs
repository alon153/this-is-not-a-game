using System;
using System.Collections;
using System.Collections.Generic;
using Basics;
using GameMode;
using Managers;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameMode.Boats
{   
    [Serializable]
    public class BoatsInRiverMode : GameModeBase
    {
        #region Serialized Fields
        
        [SerializeField] private List<GameObject> obstaclesPrefab = new List<GameObject>();

        [SerializeField] private float maxSpawnInterval = 3f;
        
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

        private float _spawnStep = 0f;

        private bool _started = false;

        private List<Vector3> _playerPositions = new List<Vector3>();

        private List<bool> _isInGame;

        #endregion

        #region Constants
        
        private const int MaxProgress = 1;

        private const int MinProgress = 0;

        #endregion


        #region Mono Behaviour Methods

        // Start is called before the first frame update
        void Start()
        {
            
                    
                    
                    
        }
        
        // Update is called once per frame
        private void Update()
        {
            if (_started)
            {   
                
                _timePassed += Time.deltaTime;
                if (_timePassed >= _curInterval)
                {
                    _timePassed = 0f;
                    _timeProgress = (TimeManager.Instance.RoundDuration - TimeManager.Instance.TimeLeft)
                                    / TimeManager.Instance.RoundDuration;
                    SpawnNewObstacles();
                    _curInterval = CalcNextInterval();
                }
            }
        }

        #endregion

        #region GameModeBase Methods

        public override void InitRound()
        {   
            GameManager.Instance.GameModeUpdateAction += Update;
            _curInterval = maxSpawnInterval;
            InitArena();
            ModeArena.OnPlayerDisqualified += DisqualifyPlayer; 
            _isInGame = new List<bool>();
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                GameManager.Instance.Players[i].transform.position = _playerPositions[i];
                _isInGame.Add(true);
            }

            _started = true;
        }

        public override void InitArena()
        {
             Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
             GameManager.Instance.CurrArena = arena;
            _arenaMaxCoord = ModeArena.TopRight;
            _arenaMinCoord = ModeArena.TopLeft;
            
            _playerPositions.Clear();
            foreach (Transform child in ModeArena.transform)
            {
                if (child.CompareTag("spawnLocation"))
                    _playerPositions.Add(child.position);
            } 
        
        }

        public override void ClearRound()
        {   
            // todo check if this is working 
            GameManager.Instance.GameModeUpdateAction -= Update;
            ModeArena.OnPlayerDisqualified -= DisqualifyPlayer; 
        }

        public override Dictionary<int, float> CalculateScore()
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

        public override void OnTimeOver()
        {
            GameManager.Instance.FreezePlayers(timed: false);
            ScoreManager.Instance.SetPlayerScores(CalculateScore());
            GameManager.Instance.ClearRound();
        }
        
        #endregion

        private float CalcNextInterval()
        {
            float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, _timeProgress);
            return interval;
        }

        /// <summary>
        /// Method is called whenever an interval has ended on Update().
        /// Calculates how much objects needs to spawned this time then makes a list of random locations to spawn
        /// the obstacles and instantiates it. 
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

        private void DisqualifyPlayer(int playerId)
        {   
            
            Debug.Log("player fell");
            // find the player that fell
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {   
                //player detected
                // todo what should be done with layers that fell? 
                if (GameManager.Instance.Players[i].GetInstanceID() == playerId)
                {
                    _isInGame[i] = false;
                    break;
                }
            }

            if (AllPlayersFell())
            {
                OnTimeOver();
            }
            
            
        }
        
        
        
       
    }
    
}

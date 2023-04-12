using System;
using System.Collections;
using System.Collections.Generic;
using GameMode;
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
        
        [SerializeField] private List<GameObject> obstaclesPrefab = new List<GameObject>();

        [SerializeField] private float maxSpawnInterval = 3f;
        
        [SerializeField] private float minSpawnInterval = 0.5f;

        [SerializeField] private float obstacleSpawnMultiplier = 10f;

        #endregion

        #region Non-Serialized Fields

        private Vector3 _arenaMinCoord;
        
        private Vector3 _arenaMaxCoord;

        private float _curInterval = 0f;

        private float _timePassed = 0f;

        private float _timeProgress = 0f;

        private float _spawnStep = 0f;

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
        void Update()
        {
            _timePassed += Time.deltaTime;
            if (_timePassed >= _curInterval)
            {
                _timePassed = 0f;
                _timeProgress = TimeManager.Instance.TimeLeft / TimeManager.Instance.RoundDuration;
                SpawnNewObstacles();
                _curInterval = CalcNextInterval();
            }
        }

        #endregion

        #region GameModeBase Methods

        public override void InitRound()
        {
            _curInterval = maxSpawnInterval;
            _arenaMaxCoord = ModeArena.TopRight;
            _arenaMinCoord = ModeArena.TopLeft;
            _spawnStep = CalcStep();



        }

        public override void InitArena()
        {
             GameObject arena = Object.Instantiate(ModeArena.gameObject, Vector3.zero, Quaternion.identity);
            
            
            
        }

        public override void ClearRound()
        {

        }

        public override void OnTimeOVer()
        {

        }
        
        #endregion

        private float CalcNextInterval()
        {
            float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, _timeProgress);
            Debug.Log("cur interval is: " + _curInterval);
            return interval;
        }

        private float CalcStep()
        {
            return 0.5f;
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
            Debug.Log("now spawning " + spawnAmount + " objects");

            HashSet<Vector3> spawnSet = new HashSet<Vector3>();

            var spawnYCor = _arenaMaxCoord.y;
            var spawnZCor = _arenaMaxCoord.z;

            
            // calc spawn locations
            while (spawnSet.Count < spawnAmount)
            {
                float spawnXCor = _arenaMinCoord.x +  Random.Range(0, (_arenaMaxCoord.x - _arenaMinCoord.y)
                                                                       / _spawnStep + 1);
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
    }
}

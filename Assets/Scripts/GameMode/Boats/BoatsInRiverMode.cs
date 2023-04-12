using System;
using System.Collections;
using System.Collections.Generic;
using GameMode;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameMode.Boats
{   
    [Serializable]
    public class BoatsInRiverMode : GameModeBase
    {
        #region Serialized Fields
        
        [SerializeField] private List<GameObject> obstacles = new List<GameObject>();

        [SerializeField] private float spawnIntervals = 1f;

        #endregion

        #region Non-Serialized Fields

        private Vector3 _arenaMinCoord;
        
        private Vector3 _arenaMaxCoord;

        private float _curInterval = 0f;

        #endregion


        #region Mono Behaviour Methods

        // Start is called before the first frame update
        void Start()
        {
            
                    
                    
                    
        }
        
        // Update is called once per frame
        void Update()
        {
            _curInterval += Time.deltaTime;
            if (_curInterval >= spawnIntervals)
            {
                _curInterval = 0f;
                SpawnObstacles(TimeManager.Instance.TimeRemained);
            }

        }

        #endregion

        #region GameModeBase Methods

        public override void InitRound()
        {
            _arenaMaxCoord = ModeArena.TopRight;
            _arenaMinCoord = ModeArena.TopLeft;
           
            
            
            

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

        private void SpawnObstacles(float timeRemained)
        {
            
            
            
        }
    }
}

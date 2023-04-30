using System;
using System.Collections.Generic;
using UnityEngine;
using Basics;
using Managers;
using Unity.Mathematics;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameMode.Lasers
{   
    [Serializable]
    public class LasersMode : GameModeBase
    {
        #region Serialized Fields
        
        [Header("Lasers\n")]
        [Tooltip("When hit by laser, how much time the player freezes?")]
        [SerializeField] private float freezeTime = 2;

        [Header("Diamonds\n")]
        [SerializeField] private DiamondCollectible[] diamondPrefabs;
        
        [Tooltip("How many (regular) diamonds will be spawned initially")]
        [Range(10,30)]
        [SerializeField] private int diamondCount = 12;
        
        [Tooltip("check this box if you want more diamonds to continue spawning after all diamonds are collected")]
        [SerializeField] private bool shouldContinueSpawn = true;

        [SerializeField] private float timeToSpawnNewDiamond = 1; 

        #endregion

        #region Non-Serialzed Fields
        
        // this dictionary is holding reference for newly created diamonds which have not yet been collected. 
        private Dictionary< int ,DiamondCollectible> _initialDiamondsNotCollected = 
            new Dictionary<int, DiamondCollectible>();
        
        // this queue is for diamonds initially created and then collected. they will go here to be used for pooling 
        // later.
        private Queue<DiamondCollectible> _collectedInitialDiamonds = new Queue<DiamondCollectible>();
        
        // this list contains the locations of diamonds spawned after all start diamonds has been collected.
        private List<Vector3> _postCollectionDiamondPos = new List<Vector3>();
        
        private ObjectPool<DiamondCollectible> _diamondPool;

        private bool _allDiamondsCollected;

        private float _diamondSpawnTimer;

        private int _diamondsCollected;

        private bool _inRound;

        #endregion
        
        #region Properties


        #endregion
        
        #region Constants

        private const int MinIndex = 0;

        private const int Empty = 0;

        private const float ResetTime = 0f;

        #endregion
        
        #region GameModeBase Methods
        
        protected override void InitRound_Inner()
        {
            foreach (var player in GameManager.Instance.Players)
                player.Addon = new LaserPlayerAddon();
            
            GameManager.Instance.GameModeUpdateAction += LaserModeUpdate;
            
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);

            // get all diamond positions and then started creating diamonds in the locations
            foreach (Transform child in arena.transform)
            {
                if (child.CompareTag("DiamondPos"))
                {   
                    var idx = Random.Range(MinIndex, diamondPrefabs.Length);
                    DiamondCollectible newDiamond = Object.Instantiate(diamondPrefabs[idx], child.transform.position,
                        quaternion.identity);
                    _initialDiamondsNotCollected.Add(newDiamond.GetInstanceID(), newDiamond);
                    newDiamond.OnDiamondPickedUp += DiamondPickedUp;
                }
            }

            GameManager.Instance.CurrArena = arena;
            _inRound = true;
        }

        protected override void ClearRound_Inner()
        {
            DestroyAllDiamonds();
            GameManager.Instance.GameModeUpdateAction -= LaserModeUpdate;
            foreach (var player in GameManager.Instance.Players)
                player.Addon = null;
        }

        protected override void OnTimeOver_Inner()
        {
            _inRound = false;
        }
        

        protected override void EndRound_Inner()
        {   
           
            GameManager.Instance.FreezePlayers(timed: false);
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            throw new System.NotImplementedException();
        }
        
        #endregion

        #region Diamond Pooling

        private DiamondCollectible CreateDiamond()
        {
            // all initial diamonds are already in the pool. so 
            // a new one needs to be created.
            if (_collectedInitialDiamonds.Count == Empty)
            {  
                
                int idx = Random.Range(MinIndex, diamondPrefabs.Length);
                var newDiamond = Object.Instantiate(diamondPrefabs[idx]);
                newDiamond.OnDiamondPickedUp += DiamondPickedUp;
                return newDiamond;
            }

            // otherwise, there is an inactive diamond that can be used. 
            return _collectedInitialDiamonds.Dequeue();
        }

        private void OnTakeDiamondFromPool(DiamondCollectible diamond)
        {
            // spawning diamonds in random locations
            diamond.transform.position =  GetRandomArenaPosition();
            diamond.gameObject.SetActive(true);
        }

        private void OnReturnDiamondToPool(DiamondCollectible diamond)
        {   
            // remove the location from active locations
            if (_allDiamondsCollected)
                RemoveFromPositions(diamond.transform.position);

            diamond.gameObject.SetActive(false);
        }

        private void OnDestroyDiamond(DiamondCollectible diamond)
        {
            if (_allDiamondsCollected)
                RemoveFromPositions(diamond.transform.position);
            
            Object.Destroy(diamond.gameObject);
        }

        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Removes a given position from the list that contains location of diamonds
        /// that are currently on the field.
        /// </summary>
        /// <param name="position">
        /// Vector3, a position of a diamond on the field. 
        /// </param>
        private void RemoveFromPositions(Vector3 position)
        {
            for (int i = 0; i < _postCollectionDiamondPos.Count; i++)
            {
                if (_postCollectionDiamondPos[i].Equals(position))
                {
                    _postCollectionDiamondPos.RemoveAt(i);
                    break;
                }
            }
        }        
        
        /// <summary>
        /// method returns a randomly generated spawn location for a new diamond from the
        /// diamond pool.
        /// </summary>
        /// <returns>
        /// A position vector3. 
        /// </returns>
        private Vector3 GetRandomArenaPosition()
        {
            var arena = GameManager.Instance.CurrArena;
            Vector3 randomVector = Vector3.zero;
            
            bool locationValid = false;
            while (!locationValid)
            {   
                float xCoord = Random.Range(arena.BottomLeft.x, arena.BottomRight.x);
                float yCoord = Random.Range(arena.BottomLeft.y, arena.TopLeft.y);
                randomVector = new Vector3(xCoord, yCoord, 1);

                foreach (var position in _postCollectionDiamondPos)
                {
                    if (position.Equals(randomVector))
                        break;
                }
                locationValid = true;
            }
            
            _postCollectionDiamondPos.Add(randomVector);
            return randomVector;
        }
        
        /// <summary>
        /// Method serves as this game mode's update. it will be called in the game manager's update while this
        /// mode is active.
        /// </summary>
        private void LaserModeUpdate()
        {
            if (!_allDiamondsCollected || !_inRound) return;
            _diamondSpawnTimer += Time.deltaTime;
            if (_diamondSpawnTimer >= timeToSpawnNewDiamond)
            {
                _diamondSpawnTimer = ResetTime;
                _diamondPool.Get();
            }
        }
        
        /// <summary>
        /// method is called as an invoked event for when a diamond is picked up.
        /// if the diamond is in the pool it returns it to the diamond pool. otherwise
        /// the diamond is from the initial diamonds scattered across map, so it is deactivated
        /// and saved for future use. 
        /// </summary>
        /// <param name="diamondPicked">
        /// the diamond that was picked up and invoked the event. 
        /// </param>
        private void DiamondPickedUp(DiamondCollectible diamondPicked)
        {   
            // it is from the pool so it needs to go back.
            if (_allDiamondsCollected)
            {
                _diamondPool.Release(diamondPicked);
            }

            else
            {
                diamondPicked.gameObject.SetActive(false);
                _initialDiamondsNotCollected.Remove(diamondPicked.GetInstanceID());
                _collectedInitialDiamonds.Enqueue(diamondPicked);
                _diamondsCollected++;
                if (diamondCount == _diamondsCollected)
                { 
                  // all initial diamonds collected so instantiate the pool
                    if (shouldContinueSpawn) _allDiamondsCollected = true;
                    Debug.Log("all diamonds collected!");
                    _diamondPool = new ObjectPool<DiamondCollectible>(CreateDiamond, OnTakeDiamondFromPool,
                        OnReturnDiamondToPool, OnDestroyDiamond, true);
                }
            }
        }
        
        /// <summary>
        /// Method will be called when clearing a round.
        /// it will iterate through all the databases of this class that contains
        /// Diamonds and will destroy it. 
        /// </summary>
        private void DestroyAllDiamonds()
        {
            if (_initialDiamondsNotCollected.Count != Empty)
            {
                foreach (var pair in _initialDiamondsNotCollected)
                    Object.Destroy(pair.Value);
            }

            if (_collectedInitialDiamonds.Count != Empty)
            {
                for (int i = 0; i < _collectedInitialDiamonds.Count; i++)
                    Object.Destroy(_collectedInitialDiamonds.Dequeue());
            }
            
            _diamondPool.Clear();
        }
        
        
        #endregion
    }
}
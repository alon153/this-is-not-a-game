using System;
using System.Collections.Generic;
using UnityEngine;
using Basics;
using Basics.Player;
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

        [Header("\nLasers")] [Tooltip("When hit by laser, how much time the player freezes?")] [SerializeField]
        private float freezeTime = 2;

        [Tooltip("When hit by laser, how much force is applied")] [SerializeField]
        private float laserKnockBackForce = 1.5f;

        [Header("\nDiamonds")] [SerializeField]
        private DiamondCollectible[] diamondPrefabs;

        [Tooltip("How many (regular) diamonds will be spawned initially")] [Range(10, 30)] [SerializeField]
        private int diamondCount = 12;

        [Tooltip("check this box if you want more diamonds to continue spawning after all diamonds are collected")]
        [SerializeField]
        private bool shouldContinueSpawn = true;

        [Tooltip("timer for new diamond to be summoned")] [SerializeField]
        private float timeToSpawnNewDiamond = 1;

        [Tooltip("How many diamonds will the player drop when hitting a laser?")] [SerializeField]
        private int diamondsDropOnLaser = 2;

        [Tooltip("in which radius from player will diamonds fall when it gets hit by laser.")] [SerializeField]
        private float onHitSpreadRadius = 3f;

        #endregion

        #region Non-Serialzed Fields

        // this dictionary is holding reference for newly created diamonds which have not yet been collected. 
        private Dictionary<int, DiamondCollectible> _initialDiamondsNotCollected =
            new Dictionary<int, DiamondCollectible>();

        // this queue is for diamonds initially created and then collected. they will go here to be used for pooling 
        // later.
        private Queue<DiamondCollectible> _collectedInitialDiamonds = new Queue<DiamondCollectible>();
        
        // a reference to initial diamonds the has benn used by the object pool
        private List<DiamondCollectible> _initialDiamondsPooled = new List<DiamondCollectible>();

        // this list contains the locations of diamonds spawned after all start diamonds has been collected.
        private List<Vector3> _postCollectionDiamondPos = new List<Vector3>();

        private ObjectPool<DiamondCollectible> _diamondPool = null;

        private bool _allDiamondsCollected;

        private float _diamondSpawnTimer;

        private int _diamondsCollected;

        private bool _inRound;

        private GameObject _laserParent;

        private Vector3? _playerPosition;

        #endregion

        #region Properties

        #endregion

        #region Constants

        private const int MinIndex = 0;

        private const int Empty = 0;

        private const float ResetTime = 0f;

        private const int None = 0;

        #endregion

        #region GameModeBase Methods

        protected override void InitRound_Inner()
        {
            _diamondsCollected = None;
            foreach (var player in GameManager.Instance.Players)
                player.Addon = new LaserPlayerAddon();

            GameManager.Instance.GameModeUpdateAction += LaserModeUpdate;
            _allDiamondsCollected = false;
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

                if (child.CompareTag("Lasers"))
                {
                    _laserParent = child.gameObject;
                    foreach (Transform laserObj in child.transform)
                    {
                        laserObj.GetComponent<LaserBeam>().OnLaserHit += OnPlayerHitByLaser;
                        laserObj.gameObject.SetActive(true);
                    }
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

            Object.Destroy(_laserParent);
        }

        protected override void OnTimeOver_Inner()
        {
            _inRound = false;
        }


        protected override void EndRound_Inner()
        {
            GameManager.Instance.FreezePlayers(timed: false);
            ScoreManager.Instance.SetPlayerScores(CalculateScore_Inner());
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            Dictionary<int, float> scores = new Dictionary<int, float>();
            foreach (var player in GameManager.Instance.Players)
            {
                PlayerAddon.CheckCompatability(player.Addon, GameModes.Lasers);
                scores.Add(player.Index, ((LaserPlayerAddon) player.Addon).DiamondsCollected);
            }

            return scores;
        }

        #endregion

        #region Diamond Pooling

        private DiamondCollectible CreateDiamond()
        {
            // all initial diamonds are already in the pool. so 
            // a new one needs to be created.
            DiamondCollectible newDiamond;
            if (_collectedInitialDiamonds.Count == Empty)
            {
                int idx = Random.Range(MinIndex, diamondPrefabs.Length);
                newDiamond = Object.Instantiate(diamondPrefabs[idx]);
                newDiamond.OnDiamondPickedUp += DiamondPickedUp;
                return newDiamond;
            }
            
            newDiamond = _collectedInitialDiamonds.Dequeue();
            // otherwise, there is an inactive diamond that can be used.
            _initialDiamondsPooled.Add(newDiamond);
            return newDiamond;

        }

        private void OnTakeDiamondFromPool(DiamondCollectible diamond)
        {
            // spawning diamonds in random locations
            diamond.transform.position = GetRandomArenaPosition();
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
            Vector3 randomPosition = Vector3.zero;
            bool locationValid = false;
            while (!locationValid)
            {
                // generate random position in a radius from player.
                if (_playerPosition != null)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    randomPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 1) *
                                     Random.Range(0f, onHitSpreadRadius);

                    // check that new position is in arena
                    if (!IsInArena(randomPosition)) break;
                }

                // generate a position in random location.
                else
                {
                    float xCoord = Random.Range(arena.BottomLeft.x, arena.BottomRight.x);
                    float yCoord = Random.Range(arena.BottomLeft.y, arena.TopLeft.y);
                    randomPosition = new Vector3(xCoord, yCoord, 1);
                }

                foreach (var position in _postCollectionDiamondPos)
                {
                    if (position.Equals(randomPosition))
                        break;
                }

                locationValid = true;
            }

            _postCollectionDiamondPos.Add(randomPosition);
            return randomPosition;
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
                _diamondPool.Release(diamondPicked);

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
            
            foreach (var pair in _initialDiamondsNotCollected)
                    Object.Destroy(pair.Value.gameObject);
           
            for (int i = 0; i < _collectedInitialDiamonds.Count; i++)
                    Object.Destroy(_collectedInitialDiamonds.Dequeue().gameObject);

            for (int i = 0; i < _initialDiamondsPooled.Count; i++)
                Object.Destroy(_initialDiamondsPooled[i].gameObject);
            

            _diamondPool?.Clear();
           
        }

        /// <summary>
        /// method will be invoked as an event from the laser object.
        /// will freeze the player and remove diamonds.
        /// </summary>
        /// <param name="player"></param>
        private void OnPlayerHitByLaser(PlayerController player)
        {
            // adjust diamonds that need to reduce. 
            PlayerAddon.CheckCompatability(player.Addon, GameModes.Lasers);
            int diamondsToDrop = diamondsDropOnLaser > ((LaserPlayerAddon) player.Addon).DiamondsCollected
                ? ((LaserPlayerAddon) player.Addon).DiamondsCollected
                : diamondsDropOnLaser;

            // reduce diamonds from the player and spawn them on field.
            if (diamondsToDrop > None)
            {
                _playerPosition = player.transform.position;

                // take from object pool
                if (_allDiamondsCollected)
                {
                    for (int i = 0; i < diamondsToDrop; i++)
                        _diamondPool.Get();
                }

                // diamond pool is not yet set so take from queue.
                else
                {
                    for (int i = 0; i < diamondsToDrop; i++)
                    {
                        var diamond = _collectedInitialDiamonds.Dequeue();
                        _initialDiamondsNotCollected[diamond.GetInstanceID()] = diamond;
                        diamond.transform.position = GetRandomArenaPosition();
                        diamond.gameObject.SetActive(true);
                    }
                }

                _playerPosition = null;

                PlayerAddon.CheckCompatability(player.Addon, GameModes.Lasers);
                ((LaserPlayerAddon) player.Addon).DiamondsCollected -= diamondsToDrop;
            }

            // save current velocity since it will be zeroed by freeze().
            Vector2 velocityBeforeFreeze = -player.Rigidbody.velocity;
            player.Freeze(true, freezeTime, true);
            player.PlayerByItemKnockBack(laserKnockBackForce, velocityBeforeFreeze);
        }

        /// <summary>
        /// gets a position and returns true if it's in the game arena,
        /// false otherwise. 
        /// </summary>
        private bool IsInArena(Vector3 position) => !ModeArena.OutOfArena(position);

        #endregion
    }
}
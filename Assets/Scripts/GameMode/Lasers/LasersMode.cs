using System;
using System.Collections.Generic;
using UnityEngine;
using Basics;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

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
        [SerializeField] private DiamondCollectible bigDiamond;

        [SerializeField] private DiamondCollectible smallDiamond;
        
        [Tooltip("How many small (regular) diamonds will be spawned")]
        [Range(10,30)]
        [SerializeField] private int smallDiamondCount = 10;
        
        [Tooltip("How many big diamonds (with more value) will be spawned")]
        [SerializeField] private int bigDiamondsCount = 5;

        #endregion

        #region Non-Serialzed Fields

        private Queue<Vector3> _bigDiamondLocations = new Queue<Vector3>();
        
        private ObjectPool<DiamondCollectible> _diamondPool;

        private int _curBigDiamond = 0;

        private int _curSmallDiamond = 0;

        /// <summary>
        /// key: PlayerId, Value: diamond score collected
        /// </summary>
        private Dictionary<int, int> _diamondsCollectedByPlayer = new Dictionary<int, int>();

        #endregion
        
        #region Properties
        

        #endregion
        
        #region Constants

        private const int DiamondCreated = 1;
        
        
        #endregion
        
        #region GameModeBase Methods
        
        protected override void InitRound_Inner()
        {
            throw new System.NotImplementedException();
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            
            foreach (Transform child in arena.transform)
            {
                if (child.CompareTag("BigDiamondPos"))
                    _bigDiamondLocations.Enqueue(child.transform.position);
            }
        }

        protected override void ClearRound_Inner()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnTimeOver_Inner()
        {
            throw new System.NotImplementedException();
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            throw new System.NotImplementedException();
        }
        
        #endregion

        #region Diamond Pooling

        private DiamondCollectible CreateDiamond()
        {
            DiamondCollectible newDiamond;
            
            // create a big diamond
            if (_curBigDiamond < bigDiamondsCount)
            {
                newDiamond = Object.Instantiate(bigDiamond);
                _curBigDiamond += DiamondCreated;
            }
            else
            {
                newDiamond = Object.Instantiate(smallDiamond);
                _curSmallDiamond += DiamondCreated;
            }
            return newDiamond;
        }

        private void OnTakeDiamondFromPool(DiamondCollectible diamond)
        {   
            if (diamond.isBig)
                diamond.transform.position = _bigDiamondLocations.Dequeue();
            
            
            diamond.gameObject.SetActive(true);
        }

        private void OnReturnDiamondToPool(DiamondCollectible diamond)
        {
            
        }

        private void OnDestroyDiamond(DiamondCollectible diamond)
        {
            
        }

        #endregion
        
        #region Private Methods

        
        #endregion
    }
}
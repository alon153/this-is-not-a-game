using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Basics;
using Basics.Player;
using GameMode;
using GameMode.Island;
using Managers;
using ScriptableObjects.GameModes.Modes;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Pool;
using Utilities.Interfaces;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameMode.Island
{
    [Serializable]
    public class IslandMode : GameModeBase, IOnMoveListener, IOnPushedListener
    {
        public override GameModes Mode => GameModes.Island;
        
        #region ScriptableObject Fields

        private int _numTreasures;
        private List<TreasureValue> _treasureValues;
        private List<DigTime> _digTimes;
        private Treasure _treasurePrefab;
        private float _vibrationRadius = 2;
        private float _vibrationMaxForce = 0.5f;        

        #endregion
        
        #region Non-Serialized Fields

        private GameObject _treasureParent;
        private int _digTimeCount;
        private int _treasureValCount;
        private ObjectPool<Treasure> _treasures;
        private int _treasureLayer;

        #endregion
        
        #region GameModeBase

        protected override void ExtractScriptableObject(GameModeObject input)
        {
            IslandModeObject sObj = (IslandModeObject) input;
            _numTreasures = sObj._numTreasures;
            _treasureValues = sObj._treasureValues;
            _digTimes = sObj._digTimes;
            _treasurePrefab = sObj._treasurePrefab;
            _vibrationRadius = sObj._vibrationRadius;
            _vibrationMaxForce = sObj._vibrationMaxForce;
        }

        protected override void InitRound_Inner()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                player.Addon = new IslandPlayerAddon();
                player.RegisterMoveListener(this);
                player.RegisterPushedListener(this);
            }

            _treasureLayer = LayerMask.NameToLayer("Treasure");
            
            SetCounts();
            _treasures = new ObjectPool<Treasure>(
                createFunc: CreateTreasure, 
                actionOnGet: OnGetTreasure, 
                actionOnRelease: OnReleaseTreasure, 
                actionOnDestroy: OnDestroyTreasure,
                true, _numTreasures);

            for (int i = 0; i < _numTreasures; i++)
                _treasures.Get();
        }

        protected override void InitArena_Inner()
        {
            GameManager.Instance.CurrArena = Object.Instantiate(ModeArena);
            _treasureParent = new GameObject();
        }

        protected override void ClearRound_Inner()
        {
            _treasures.Clear();
            Object.Destroy(_treasureParent.gameObject);
            foreach (var player in GameManager.Instance.Players)
            {
                player.Addon = null;
                player.StopVibration();
                player.UnRegisterMoveListener(this);
                player.UnRegisterPushedListener(this);
            }
        }

        protected override void OnTimeOver_Inner() { }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            var scores = new Dictionary<int, float>();
            foreach (var player in GameManager.Instance.Players)
            {
                PlayerAddon.CheckCompatability(player.Addon, GameModes.Island);
                scores[player.Index] = ((IslandPlayerAddon) player.Addon).Score;
            }

            return scores;
        }
        
        #endregion

        #region IOnPushedListener

        public void OnPushed(PlayerController pushed, PlayerController pusher)
        {
            if(pushed.Interactable == null)
                return;
            
            pushed.Interactable.OnInteract(pushed, false);
            pushed.Interactable = null;
        }

        #endregion

        #region IOnMoveListener

        public void OnMove(PlayerController player, Vector3 @from, Vector3 to)
        {
            Vector3 playerPos = player.transform.position;
            var collisions = Physics2D.OverlapCircleAll(playerPos, _vibrationRadius, _treasureLayer);
            if (collisions.Length == 0)
            {
                player.StopVibration();
                return;
            }

            int minIndex = -1;
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < collisions.Length; i++)
            {
                if(!collisions[i].gameObject.CompareTag("Treasure")) continue;
                float dist = (playerPos - collisions[i].gameObject.transform.position).magnitude;
                if (minIndex == -1 || dist < minDistance)
                {
                    minIndex = i;
                    minDistance = dist;
                }
            }

            if (minIndex == -1)
            {
                player.StopVibration();
                return;
            }

            minDistance = Mathf.Clamp(minDistance, 0, _vibrationRadius);
            float vibration = (1 - Mathf.Pow(minDistance / _vibrationRadius,2)) * _vibrationMaxForce;
            player.SetVibration(vibration);
        }

        #endregion

        #region Private Methods
        
        private void GenerateTreasureData(Treasure treasure)
        {
            int valCount = Random.Range(0, _treasureValCount);
            int digTimeCount = Random.Range(0, _digTimeCount);
            
            treasure.transform.position = GameManager.Instance.CurrArena.GetRespawnPosition(treasure.gameObject);
            (treasure.Score, treasure.Sprite) = GetTreasureValue(valCount);
            treasure.DiggingTime = GetDigTime(digTimeCount);
        }

        private void SetCounts()
        {
            _digTimeCount = 0;
            _treasureValCount = 0;

            foreach (var value in _treasureValues) { _treasureValCount += value.count; }
            foreach (var dig in _digTimes) { _digTimeCount += dig.count; }
        }

        private float GetDigTime(int count)
        {
            int countLeft = count;
            for (int i = 0; i < _digTimes.Count; i++)
            {
                countLeft -= _digTimes[i].count;
                if (countLeft <= 0)
                    return Random.Range(_digTimes[i].minTime, _digTimes[i].maxTime);
            }

            return 0;
        }
        
        private (float score, Sprite sprite) GetTreasureValue(int count)
        {
            int countLeft = count;
            for (int i = 0; i < _treasureValues.Count; i++)
            {
                countLeft -= _treasureValues[i].count;
                if (countLeft <= 0)
                {
                    var t = _treasureValues[i];
                    var score = t.score;
                    var sprite = t.sprites[Random.Range(0, t.sprites.Count)];
                    return (score, sprite);
                }
            }

            return (0, null);
        }

        #endregion

        #region Pooling Methods

        private Treasure CreateTreasure()
        {
            Treasure treasure = Object.Instantiate(_treasurePrefab, _treasureParent.transform);
            treasure.Pool = _treasures;
            GenerateTreasureData(treasure);   

            return treasure;
        }

        private void OnGetTreasure(Treasure treasure)
        {
            GenerateTreasureData(treasure);
            treasure.gameObject.SetActive(true);
        }
        
        private void OnReleaseTreasure(Treasure treasure)
        {
            treasure.Release();
        }

        private void OnDestroyTreasure(Treasure treasure)
        {
            Object.Destroy(treasure.gameObject);
        }

        #endregion

        #region Classes

        [Serializable]
        public class TreasureValue
        {
            public float score;
            public int count;
            public List<Sprite> sprites;
        }
        
        [Serializable]
        public class DigTime
        {
            public float minTime;
            public float maxTime;
            public int count;
        }

        #endregion
    }
}
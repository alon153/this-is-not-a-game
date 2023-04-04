﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class TimeManager : SingletonPersistent<TimeManager>
    {
        #region Non-Serialized Fields

        private readonly Dictionary<Guid, (Action action, float time)> _actions = new();
        private readonly Dictionary<Guid, (Action action, float time)> _fixedActions = new();

        private Coroutine _countDownCoroutine;

        #endregion
        
        #region Event Functions
      

        private void Update()
        {
            if(_actions.Count > 0)
                CountDown(Time.time, _actions);
        }

        private void FixedUpdate()
        {
            if(_fixedActions.Count > 0)
                CountDown(Time.time, _fixedActions);
        }

        #endregion

        #region Public Methods

        public void StartCountDown(int duration)
        {
            if(_countDownCoroutine != null)
                StopCoroutine(_countDownCoroutine);
                
            _countDownCoroutine = StartCoroutine(CountDown_Inner(duration));
        }

        public Guid DelayInvoke(Action action, float delayTime)
        {
            Guid key = Guid.NewGuid();
            _actions[key] = (action, Time.time + delayTime);
            return key;
        }
  
        public Guid FixedDelayInvoke(Action action, float delayTime)
        {
            Guid key = Guid.NewGuid();
            _fixedActions[key] = (action, Time.time + delayTime);
            return key;
        }

        public bool CancelInvoke(Guid id)
        {
            return Cancel_Inner(id, false);
        }
        
        public bool Invoke(Guid id)
        {
            return Cancel_Inner(id, true);
        }

        #endregion

        #region Private Methods

        private IEnumerator CountDown_Inner(int duration)
        {
            UIManager.Instance.UpdateTime(duration);
            yield return null;
            
            float timeLeft = duration;
            while (timeLeft > 0)
            {
                UIManager.Instance.UpdateTime(Mathf.Ceil(timeLeft));
                yield return null;
                
                timeLeft -= Time.deltaTime;
            }
            
            UIManager.Instance.UpdateTime(0);
            yield return null;
            
            GameManager.Instance.OnTimeOver();
        }

        private bool Cancel_Inner(Guid id, bool invoke)
        {
            if (_fixedActions.ContainsKey(id) || _actions.ContainsKey(id))
            {
                if (_fixedActions.ContainsKey(id))
                {
                    if(invoke) _fixedActions[id].action.Invoke();
                    _fixedActions.Remove(id);
                }
                if (_actions.ContainsKey(id))
                {
                    if(invoke) _actions[id].action.Invoke();
                    _actions.Remove(id);
                }
                return true;
            }

            return false;
        }

        private void CountDown(float time, Dictionary<Guid, (Action action, float time)> actions)
        {
            var toDelete = new HashSet<Guid>();
            var keys = actions.Keys.ToList();
            foreach (var key in keys)
            {
                var val = actions[key];
                if (time >= val.time)
                {
                    val.action.Invoke();
                    toDelete.Add(key);
                }
            }

            foreach (var key in toDelete)
                if(actions.ContainsKey(key))
                    actions.Remove(key);
        }

        #endregion
    }
}
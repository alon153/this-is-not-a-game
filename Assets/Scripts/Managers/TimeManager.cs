using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class TimeManager : Singleton<TimeManager>
    {
        #region Non-Serialized Fields

        private readonly Dictionary<Guid, (Action action, float time)> _actions = new();
        private readonly Dictionary<Guid, (Action action, float time)> _fixedActions = new();

        private Coroutine _countDownCoroutine;
        private UIManager.CountDownTimer _currTimer;
        public float TimeLeft { get; private set; }
        public float RoundDuration { get; private set; }

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

        public void StartCountDown(int duration, Action onEnd, UIManager.CountDownTimer timer = UIManager.CountDownTimer.Game)
        {
            if(_countDownCoroutine != null)
                StopCoroutine(_countDownCoroutine);
                
            _countDownCoroutine = StartCoroutine(CountDown_Inner(duration, onEnd, timer));
        }

        public void StopCountDown()
        {
            if(_countDownCoroutine == null)
                return;
            
            StopCoroutine(_countDownCoroutine);
            UIManager.Instance.StopFade();
            UIManager.Instance.UpdateTime(0,_currTimer);
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

        private IEnumerator CountDown_Inner(int duration, Action onEnd, UIManager.CountDownTimer timer = UIManager.CountDownTimer.Game)
        {
            _currTimer = timer;
            
            UIManager.Instance.UpdateTime(duration, timer);
            yield return null;

            RoundDuration = duration;
            TimeLeft = duration;
            int lastUpdate = 0;
            
            while (TimeLeft > 0)
            {
                if (Mathf.CeilToInt(TimeLeft) != lastUpdate)
                {
                    UIManager.Instance.UpdateTime(Mathf.Ceil(TimeLeft), timer);
                    lastUpdate = Mathf.CeilToInt(TimeLeft);
                }
                
                yield return null;
                
                TimeLeft -= Time.deltaTime;
            }
            
            UIManager.Instance.UpdateTime(0, timer);
            yield return null;
            
            onEnd.Invoke();
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
                if (!actions.ContainsKey(key))
                {
                    toDelete.Add(key);
                    continue;
                }
                
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

        public void CancelAll()
        {
            List<Guid> keys = new List<Guid>(_actions.Keys);
            keys.AddRange(_fixedActions.Keys);
            foreach (var k in keys)
            {
                Cancel_Inner(k, false);
            }
        }
    }
}
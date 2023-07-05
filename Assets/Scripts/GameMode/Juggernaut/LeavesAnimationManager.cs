using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace GameMode.Juggernaut
{
    public class LeavesAnimationManager : MonoBehaviour
    {   
        [Tooltip("drag all leaves animators to here")]
        [SerializeField] private List<Animator> leavesAnimators = new List<Animator>();
        
        [Tooltip("every time an interval passes we animate the next leave")]
        [SerializeField] private float timeInterval = 2f;

        private float _time = 0f;

        private int _curIdx = Constants.MinIndex;
        
        private static readonly int ShakeLeaves = Animator.StringToHash("shakeLeaves");

        // Update is called once per frame
        void Update()
        {

            _time += Time.deltaTime;

            if (_time >= timeInterval)
            {
                _time = 0f;
                
                // leaves come in pairs, so animate the pair 
                leavesAnimators[_curIdx].SetTrigger(ShakeLeaves);
                leavesAnimators[_curIdx + 1].SetTrigger(ShakeLeaves);

                _curIdx += 2;

                if (_curIdx == leavesAnimators.Count)
                    _curIdx = Constants.MinIndex;

            }

        }
    }
}

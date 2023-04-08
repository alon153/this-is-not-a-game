using System.Collections.Generic;
using Utilities.Interfaces;

namespace Basics.Player
{
    public partial class PlayerController
    {
        private HashSet<IOnMoveListener> _moveListeners = new HashSet<IOnMoveListener>();
        private HashSet<IOnFallListener> _fallListeners = new HashSet<IOnFallListener>();
        
        public void RegisterMoveListener(IOnMoveListener l)
        {
            if(!_moveListeners.Contains(l))
                _moveListeners.Add(l);
        }
        
        public void UnRegisterMoveListener(IOnMoveListener l)
        {
            if(_moveListeners.Contains(l))
                _moveListeners.Remove(l);
        }

        public void RegisterFallListener(IOnFallListener l)
        {
            if (!_fallListeners.Contains(l))
                _fallListeners.Add(l);
        }

        public void UnRegisterFallListener(IOnFallListener l)
        {
            if (_fallListeners.Contains(l))
                _fallListeners.Remove(l);
        }
    }
}
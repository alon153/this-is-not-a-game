using System.Collections.Generic;
using Utilities.Interfaces;

namespace Basics.Player
{
    public partial class PlayerController
    {
        private HashSet<IOnMoveListener> _moveListeners = new HashSet<IOnMoveListener>();

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
    }
}
using Basics.Player;
using UnityEngine;

namespace Utilities.Listeners
{
    public interface IOnMoveListener
    {
        public void OnMove(PlayerController player, Vector3 from, Vector3 to);
    }
}
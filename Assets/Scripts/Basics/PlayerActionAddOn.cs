using Basics.Player;
using GameMode;

namespace Basics
{
    public abstract class PlayerActionAddOn : PlayerAddon
    {
        public abstract void OnAction(PlayerController player);
    }
}
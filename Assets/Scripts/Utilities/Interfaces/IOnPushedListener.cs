using Basics.Player;

namespace Utilities.Interfaces
{
    public interface IOnPushedListener
    {
        void OnPushed(PlayerController pushed, PlayerController pusher);
    }
}
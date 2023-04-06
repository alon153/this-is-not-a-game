using Basics.Player;

namespace Utilities.Listeners
{
    public interface IOnFallIntoHoleListener
    {
        public void OnFallIntoHall(PlayerController playerFell);
        
    }
}
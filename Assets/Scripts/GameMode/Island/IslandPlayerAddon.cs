using Basics;

namespace GameMode.Island
{
    public class IslandPlayerAddon : PlayerAddon
    {
        public float Score { get; set; }
        
        public override GameModes GameMode()
        {
            return GameModes.Island;
        }
    }
}
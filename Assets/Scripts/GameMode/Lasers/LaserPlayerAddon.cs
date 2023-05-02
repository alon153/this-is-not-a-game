using Basics;

namespace GameMode.Lasers
{
    public class LaserPlayerAddon : PlayerAddon
    {
        public int DiamondsCollected { get; set; } = 0;
        public override GameModes GameMode()
        {
            return GameModes.Lasers;
        }
    }
}
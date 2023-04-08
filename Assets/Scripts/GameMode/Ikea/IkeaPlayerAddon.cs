using Basics;

namespace GameMode.Ikea
{
    public class IkeaPlayerAddon : PlayerAddon
    {
        public IkeaPart Part { get; set; }
        
        public override GameModes GameMode()
        {
            return GameModes.Ikea;
        }
    }
}
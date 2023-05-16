using Basics;


namespace GameMode.Juggernaut
{
    public class JuggernautPlayerAddOn : PlayerAddon
    {
        public bool YieldsTotem { get; set; } = false;

        public float TotalTimeYieldingTotem { get; set; } = 0f;
        
        public override GameModes GameMode()
        {
            return GameModes.Juggernaut;
        }
    }
}

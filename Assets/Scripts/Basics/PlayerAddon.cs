using System;
using Basics.Player;
using GameMode;

namespace Basics
{
    public abstract class PlayerAddon
    {
        public abstract GameModes GameMode();

        public static void CheckCompatability(PlayerAddon addon, GameModes mode)
        {
            if(addon == null || addon.GameMode() != mode)
                throw new IncompatibleGameModesException(
                    m1: addon?.GameMode() ?? GameModes.Null,
                    mode);
        }

        public class IncompatibleGameModesException : Exception
        {
            public IncompatibleGameModesException(GameModes m1, GameModes m2) : base($"Incompatible modes {m1}, {m2}"){ }
        }
    }
    
}
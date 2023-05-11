namespace Utilities.Interfaces
{
    public interface IFallable
    {
        void Fall(bool shouldRespawn = true, bool stun = true);
    }
}
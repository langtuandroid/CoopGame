using Fusion;

namespace Main.Scripts.Player.Data
{
    public struct UserId : INetworkStruct
    {
        public NetworkString<_32> Id;

        public UserId(string id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            return obj is UserId userId && Equals(userId);
        }

        public bool Equals(UserId other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
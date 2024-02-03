namespace TUI
{
    public interface INetworkBehaviour
    {
        bool IsClient { get; }
        bool IsServer { get; }
        bool IsOwner { get; }

    }
}

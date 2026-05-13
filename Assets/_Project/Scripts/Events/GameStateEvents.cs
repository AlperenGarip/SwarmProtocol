using SwarmProtocol.Core;

namespace SwarmProtocol.Events
{
    public struct GameStateChangedEvent
    {
        public GameState NewState;
        public GameState PreviousState;
    }
}

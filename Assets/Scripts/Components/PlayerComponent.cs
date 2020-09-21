using Unity.Entities;

namespace SimpleECS
{
    // Component data for a Player entity.
    [GenerateAuthoringComponent]
    public struct Player : IComponentData
    {
        public int Score;
    }
}
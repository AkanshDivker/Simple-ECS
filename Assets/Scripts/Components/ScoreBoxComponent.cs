using Unity.Entities;

namespace SimpleECS
{
    // Component data for a ScoreBox entity.
    [GenerateAuthoringComponent]
    public struct ScoreBox : IComponentData
    {
        public int Points;
    }
}